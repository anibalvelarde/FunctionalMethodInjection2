using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPortal
{

    public class HandleMethodAttribute : Attribute
    {
        public string MethodName { get; private set; }
        public HandleMethodAttribute(string methodName)
        {
            this.MethodName = methodName;
        }
    }

    public enum ObjectPortalMethod
    {
        Create, CreateChild,
        Fetch, FetchChild,
        Update, UpdateChild,
        Insert, InsertChild
    }


    public interface IHandleRegistration
    {
        ObjectPortalMethod method { get; }
    }

    public interface IHandleRegistrationNoCriteria<T> : IHandleRegistration
    {
        void ExecuteMethod(T obj, ILifetimeScope scope);
    }

    public interface IHandleRegistrationWithCriteria<T, C> : IHandleRegistration
    {
        void ExecuteMethod(T obj, ILifetimeScope scope, C criteria);
    }

    public class Handle<T> : IHandleRegistrationNoCriteria<T>
    {
        public Action<T> callMethod { get; set; }

        public ObjectPortalMethod method { get; set; }

        public virtual void ExecuteMethod(T obj, ILifetimeScope scope)
        {
            callMethod(obj);
        }

    }

    public class HandleDep<T, D> : IHandleRegistrationNoCriteria<T>
    {
        public Action<T, D> callMethod { get; set; }

        public ObjectPortalMethod method { get; set; }

        public void ExecuteMethod(T obj, ILifetimeScope scope)
        {
            // TODO: Discuss sending in the scope
            // I think we have to do this because the HandleRegistrations are Static (Singleton)

            // Use type of D to create Action<T>

            D dep = scope.Resolve<D>();

            callMethod(obj, dep);

        }

    }

    public class HandleWithCriteria<T, C> : IHandleRegistrationWithCriteria<T, C>
    {
        public Action<T, C> callMethod { get; set; }

        public ObjectPortalMethod method { get; set; }

        public void ExecuteMethod(T obj, ILifetimeScope scope, C criteria)
        {
            callMethod(obj, criteria);
        }
    }

    public class HandleWithCriteriaDep<T, C, D> : IHandleRegistrationWithCriteria<T, C>
    {
        public Action<T, C, D> callMethod { get; set; }

        public ObjectPortalMethod method { get; set; }

        public void ExecuteMethod(T obj, ILifetimeScope scope, C criteria)
        {
            // Use type of D to create Action<T>

            D dep = scope.Resolve<D>();

            callMethod(obj, criteria, dep);

        }
    }

    internal interface IHandleRegistrations
    {
        // This can't be generic and Obj can't be T 
        // because then it can't be used in ObjectPortal
        // ObjectPortal is of the interface type
        // But IHandleRegistrations is of the concrete type


        bool TryExecuteMethod(object obj, ObjectPortalMethod method, ILifetimeScope scope);
        bool TryExecuteMethod<C>(object obj, ObjectPortalMethod method, ILifetimeScope scope, C criteria);

    }

    public interface IHandleRegistrations<T>
    {
        void Add(ObjectPortalMethod method, IHandleRegistrationNoCriteria<T> reg);
        void Add<C>(ObjectPortalMethod method, IHandleRegistrationWithCriteria<T, C> reg);

    }

    public class HandleRegistrations<T> : IHandleRegistrations<T>, IHandleRegistrations
    {

        public HandleRegistrations()
        {

            // Pass this object to the static Handle method defined for the type

            var type = typeof(T);

            var att = type.GetCustomAttribute<HandleMethodAttribute>();

            if (att == null)
            {
                throw new ObjectPortalOperationNotSupportedException($"No HandleMethodAttribute on {type.FullName}");
            }

            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Where(x => x.Name == att.MethodName).ToList();
            MethodInfo handleMethod;

            if (methods.Count == 1)
            {
                handleMethod = methods[0];
            }
            else
            {
                throw new ObjectPortalOperationNotSupportedException($"Invalid number of Handle methods or method not found [{methods.Count}] on {type.FullName}");
            }

            handleMethod.Invoke(null, new object[] { this });

        }

        private List<IHandleRegistration> regs { get; set; } = new List<IHandleRegistration>();

        public void Add(ObjectPortalMethod method, IHandleRegistrationNoCriteria<T> reg)
        {
            if (regs.OfType<IHandleRegistrationNoCriteria<T>>().Where(r => r.method == method).FirstOrDefault() != null)
            {
                throw new ObjectPortalOperationNotSupportedException($"Key is already present in registrations {method.ToString()}");
            }


            regs.Add(reg);
        }


        public void Add<C>(ObjectPortalMethod method, IHandleRegistrationWithCriteria<T, C> reg)
        {
            if (regs.OfType<IHandleRegistrationWithCriteria<T, C>>().Where(r => r.method == method).FirstOrDefault() != null)
            {
                throw new ObjectPortalOperationNotSupportedException($"Key is already present in registrations {method.ToString()}");
            }


            regs.Add(reg);
        }




        bool IHandleRegistrations.TryExecuteMethod(object obj, ObjectPortalMethod method, ILifetimeScope scope)
        {

            var reg = regs.OfType<IHandleRegistrationNoCriteria<T>>().Where(r => r.method == method).SingleOrDefault();

            if (reg == null) { return false; }

            // TODO : Discuss - At some point needs to go from IBO to BO
            // Right now that point is right here
            reg.ExecuteMethod((T)obj, scope);

            return true;
        }

        bool IHandleRegistrations.TryExecuteMethod<C>(object obj, ObjectPortalMethod method, ILifetimeScope scope, C criteria)
        {
            var reg = regs.OfType<IHandleRegistrationWithCriteria<T, C>>().Where(r => r.method == method).SingleOrDefault();

            if (reg == null) { return false; }

            reg.ExecuteMethod((T)obj, scope, criteria);

            return true;

        }

    }

    public static class HandleRegistrationExtensions
    {

        private static Action<T, D> CreateAction<T, D>(MethodInfo businessObjectMethod)
        {
            return (bo, d) => { businessObjectMethod.Invoke(bo, new object[] { d }); };
        }

        private static void _HandleDep<T, D>(IHandleRegistrations<T> regs, MethodInfo methodInfo, ObjectPortalMethod method)
        {
            var parameters = methodInfo.GetParameters();

            // Method has dependencies
            // Resolve each dependency

            // Need to create (BusinessItem bo, Guid c) => bo.CreateChild(c)

            if (parameters.Length == 1)
            {

                var createMethodInfo = CreateAction<T, D>(methodInfo);

                regs.Add(method, new HandleDep<T, D>() { callMethod = createMethodInfo, method = method });

                return;
            }

            throw new NotSupportedException("Only 1 parameter supported");

        }

        private static void Handle<T>(IHandleRegistrations<T> regs, string methodName, ObjectPortalMethod method)
        {


            var methodInfo = typeof(T).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            var parameters = methodInfo.GetParameters();

            if (parameters.Count() == 0)
            {
                Action<T> callMethod = null;

                callMethod = (T bo) =>
                {
                    methodInfo.Invoke(bo, null);
                };

                regs.Add(method, new Handle<T>() { callMethod = callMethod, method = ObjectPortalMethod.Create });

            }
            else if (parameters.Count() == 1)
            {

                var createMethodInfo = typeof(HandleRegistrationExtensions).GetMethod(nameof(_HandleDep), BindingFlags.Static | BindingFlags.NonPublic)
                    .MakeGenericMethod(new Type[] { typeof(T), parameters[0].ParameterType });

                createMethodInfo.Invoke(null, new object[] { regs, methodInfo, method });

            }

        }




        private static Action<T, C, D> CreateAction<T, C, D>(MethodInfo businessObjectMethod)
        {
            return (bo, c, d) => { businessObjectMethod.Invoke(bo, new object[] { c, d }); };
        }

        private static void _HandleCriteriaDep<T, C, D>(IHandleRegistrations<T> regs, MethodInfo methodInfo, ObjectPortalMethod method)
        {
            var parameters = methodInfo.GetParameters();

            // Method has dependencies
            // Resolve each dependency

            // Need to create (BusinessItem bo, Guid c) => bo.CreateChild(c)

            if (parameters.Length == 2)
            {

                var createMethodInfo = CreateAction<T, C, D>(methodInfo);

                regs.Add(method, new HandleWithCriteriaDep<T, C, D>() { callMethod = createMethodInfo, method = method });

                return;
            }

            throw new NotSupportedException("Only 2 parameter supported");

        }

        private static void Handle<T, C>(IHandleRegistrations<T> regs, string methodName, ObjectPortalMethod method)
        {


            var methodInfo = typeof(T).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            var parameters = methodInfo.GetParameters();

            if (parameters.Count() == 0)
            {
                throw new Exception("Method must take at least 1 parameter of criteria");
            }
            else if (parameters.Count() == 1)
            {
                // Criteria only

                Action<T, C> callMethod = null;

                callMethod = (T bo, C criteria) =>
                {
                    methodInfo.Invoke(bo, new object[] { criteria });
                };

                regs.Add(method, new HandleWithCriteria<T, C>() { callMethod = callMethod, method = ObjectPortalMethod.Create });


            }
            else if (parameters.Count() == 2)
            {
                // Criteria and Dependencies

                var createMethodInfo = typeof(HandleRegistrationExtensions).GetMethod(nameof(_HandleCriteriaDep), BindingFlags.Static | BindingFlags.NonPublic)
                    .MakeGenericMethod(new Type[] { typeof(T), parameters[0].ParameterType, parameters[1].ParameterType });

                createMethodInfo.Invoke(null, new object[] { regs, methodInfo, method });

            }

        }

        public static void HandleCreate<T>(this IHandleRegistrations<T> regs, string methodName)
        {
            Handle<T>(regs, methodName, ObjectPortalMethod.Create);
        }

        public static void HandleCreate<T, C>(this IHandleRegistrations<T> regs, string methodName)
        {
            Handle<T, C>(regs, methodName, ObjectPortalMethod.Create);
        }

        public static void HandleCreate<T>(this IHandleRegistrations<T> regs, Action<T> a)
        {
            regs.Add(ObjectPortalMethod.Create, new Handle<T>() { callMethod = a, method = ObjectPortalMethod.Create });
        }

        public static void HandleCreateWithDependency<T, D>(this IHandleRegistrations<T> regs, Action<T, D> a)
        {
            regs.Add(ObjectPortalMethod.Create, new HandleDep<T, D>() { callMethod = a, method = ObjectPortalMethod.Create });
        }

        public static void HandleCreate<T, C>(this IHandleRegistrations<T> regs, Action<T, C> a)
        {
            regs.Add<C>(ObjectPortalMethod.Create, new HandleWithCriteria<T, C>() { callMethod = a, method = ObjectPortalMethod.Create });
        }

        public static void HandleCreate<T, C, D>(this IHandleRegistrations<T> regs, Action<T, C, D> a)
        {
            regs.Add<C>(ObjectPortalMethod.Create, new HandleWithCriteriaDep<T, C, D>() { callMethod = a, method = ObjectPortalMethod.Create });
        }
        public static void HandleCreateChild<T>(this IHandleRegistrations<T> regs, Action<T> a)
        {
            regs.Add(ObjectPortalMethod.CreateChild, new Handle<T>() { callMethod = a, method = ObjectPortalMethod.CreateChild });
        }

        public static void HandleCreateChild<T>(this IHandleRegistrations<T> regs, string methodName)
        {
            Handle(regs, methodName, ObjectPortalMethod.CreateChild);
        }

        public static void HandleCreateChild<T, C>(this IHandleRegistrations<T> regs, string methodName)
        {
            Handle<T, C>(regs, methodName, ObjectPortalMethod.CreateChild);
        }

        public static void HandleCreateChildWithDependency<T, D>(this IHandleRegistrations<T> regs, Action<T, D> a)
        {
            regs.Add(ObjectPortalMethod.CreateChild, new HandleDep<T, D>() { callMethod = a, method = ObjectPortalMethod.CreateChild });
        }

        /// <summary>
        /// Register the method to call on ObjectPortal.Create with criteria and no dependencies
        /// </summary>
        /// <typeparam name="T">Business Object</typeparam>
        /// <typeparam name="C">Criteria</typeparam>
        /// <param name="regs"></param>
        /// <param name="a"></param>
        public static void HandleCreateChild<T, C>(this IHandleRegistrations<T> regs, Action<T, C> a)
        {
            regs.Add<C>(ObjectPortalMethod.CreateChild, new HandleWithCriteria<T, C>() { callMethod = a, method = ObjectPortalMethod.CreateChild });
        }

        public static void HandleCreateChild<T, C, D>(this IHandleRegistrations<T> regs, Action<T, C, D> a)
        {
            regs.Add<C>(ObjectPortalMethod.CreateChild, new HandleWithCriteriaDep<T, C, D>() { callMethod = a, method = ObjectPortalMethod.CreateChild });
        }

        public static void HandleFetch<T>(this IHandleRegistrations<T> regs, Action<T> a)
        {
            regs.Add(ObjectPortalMethod.Fetch, new Handle<T>() { callMethod = a, method = ObjectPortalMethod.Fetch });
        }

        public static void HandleFetchWithDependency<T, D>(this IHandleRegistrations<T> regs, Action<T, D> a)
        {
            regs.Add(ObjectPortalMethod.Fetch, new HandleDep<T, D>() { callMethod = a, method = ObjectPortalMethod.Fetch });
        }

        public static void HandleFetch<T, C>(this IHandleRegistrations<T> regs, Action<T, C> a)
        {
            regs.Add<C>(ObjectPortalMethod.Fetch, new HandleWithCriteria<T, C>() { callMethod = a, method = ObjectPortalMethod.Fetch });
        }

        public static void HandleFetch<T, C, D>(this IHandleRegistrations<T> regs, Action<T, C, D> a)
        {
            regs.Add<C>(ObjectPortalMethod.Fetch, new HandleWithCriteriaDep<T, C, D>() { callMethod = a, method = ObjectPortalMethod.Fetch });
        }
        public static void HandleFetchChild<T>(this IHandleRegistrations<T> regs, Action<T> a)
        {
            regs.Add(ObjectPortalMethod.FetchChild, new Handle<T>() { callMethod = a, method = ObjectPortalMethod.FetchChild });
        }

        public static void HandleFetchChildWithDependency<T, D>(this IHandleRegistrations<T> regs, Action<T, D> a)
        {
            regs.Add(ObjectPortalMethod.FetchChild, new HandleDep<T, D>() { callMethod = a, method = ObjectPortalMethod.FetchChild });
        }

        public static void HandleFetchChild<T, C>(this IHandleRegistrations<T> regs, Action<T, C> a)
        {
            regs.Add<C>(ObjectPortalMethod.FetchChild, new HandleWithCriteria<T, C>() { callMethod = a, method = ObjectPortalMethod.FetchChild });
        }

        public static void HandleFetchChild<T, C, D>(this IHandleRegistrations<T> regs, Action<T, C, D> a)
        {
            regs.Add<C>(ObjectPortalMethod.FetchChild, new HandleWithCriteriaDep<T, C, D>() { callMethod = a, method = ObjectPortalMethod.FetchChild });
        }

        public static void HandleUpdate<T>(this IHandleRegistrations<T> regs, Action<T> a)
        {
            regs.Add(ObjectPortalMethod.Update, new Handle<T>() { callMethod = a, method = ObjectPortalMethod.Update });
        }

        public static void HandleUpdateWithDependency<T, D>(this IHandleRegistrations<T> regs, Action<T, D> a)
        {
            regs.Add(ObjectPortalMethod.Update, new HandleDep<T, D>() { callMethod = a, method = ObjectPortalMethod.Update });
        }

        public static void HandleUpdate<T, C>(this IHandleRegistrations<T> regs, Action<T, C> a)
        {
            regs.Add<C>(ObjectPortalMethod.Update, new HandleWithCriteria<T, C>() { callMethod = a, method = ObjectPortalMethod.Update });
        }

        public static void HandleUpdate<T, C, D>(this IHandleRegistrations<T> regs, Action<T, C, D> a)
        {
            regs.Add<C>(ObjectPortalMethod.Update, new HandleWithCriteriaDep<T, C, D>() { callMethod = a, method = ObjectPortalMethod.Update });
        }
        public static void HandleUpdateChild<T>(this IHandleRegistrations<T> regs, Action<T> a)
        {
            regs.Add(ObjectPortalMethod.UpdateChild, new Handle<T>() { callMethod = a, method = ObjectPortalMethod.UpdateChild });
        }

        public static void HandleUpdateChildWithDependency<T, D>(this IHandleRegistrations<T> regs, Action<T, D> a)
        {
            regs.Add(ObjectPortalMethod.UpdateChild, new HandleDep<T, D>() { callMethod = a, method = ObjectPortalMethod.UpdateChild });
        }

        public static void HandleUpdateChild<T, C>(this IHandleRegistrations<T> regs, Action<T, C> a)
        {
            regs.Add<C>(ObjectPortalMethod.UpdateChild, new HandleWithCriteria<T, C>() { callMethod = a, method = ObjectPortalMethod.UpdateChild });
        }

        public static void HandleUpdateChild<T, C, D>(this IHandleRegistrations<T> regs, Action<T, C, D> a)
        {
            regs.Add<C>(ObjectPortalMethod.UpdateChild, new HandleWithCriteriaDep<T, C, D>() { callMethod = a, method = ObjectPortalMethod.UpdateChild });
        }

        public static void HandleInsert<T>(this IHandleRegistrations<T> regs, Action<T> a)
        {
            regs.Add(ObjectPortalMethod.Insert, new Handle<T>() { callMethod = a, method = ObjectPortalMethod.Insert });
        }

        public static void HandleInsertWithDependency<T, D>(this IHandleRegistrations<T> regs, Action<T, D> a)
        {
            regs.Add(ObjectPortalMethod.Insert, new HandleDep<T, D>() { callMethod = a, method = ObjectPortalMethod.Insert });
        }

        public static void HandleInsert<T, C>(this IHandleRegistrations<T> regs, Action<T, C> a)
        {
            regs.Add<C>(ObjectPortalMethod.Insert, new HandleWithCriteria<T, C>() { callMethod = a, method = ObjectPortalMethod.Insert });
        }

        public static void HandleInsert<T, C, D>(this IHandleRegistrations<T> regs, Action<T, C, D> a)
        {
            regs.Add<C>(ObjectPortalMethod.Insert, new HandleWithCriteriaDep<T, C, D>() { callMethod = a, method = ObjectPortalMethod.Insert });
        }
        public static void HandleInsertChild<T>(this IHandleRegistrations<T> regs, Action<T> a)
        {
            regs.Add(ObjectPortalMethod.InsertChild, new Handle<T>() { callMethod = a, method = ObjectPortalMethod.InsertChild });
        }

        public static void HandleInsertChildWithDependency<T, D>(this IHandleRegistrations<T> regs, Action<T, D> a)
        {
            regs.Add(ObjectPortalMethod.InsertChild, new HandleDep<T, D>() { callMethod = a, method = ObjectPortalMethod.InsertChild });
        }

        public static void HandleInsertChild<T, C>(this IHandleRegistrations<T> regs, Action<T, C> a)
        {
            regs.Add<C>(ObjectPortalMethod.InsertChild, new HandleWithCriteria<T, C>() { callMethod = a, method = ObjectPortalMethod.InsertChild });
        }

        public static void HandleInsertChild<T, C, D>(this IHandleRegistrations<T> regs, Action<T, C, D> a)
        {
            regs.Add<C>(ObjectPortalMethod.InsertChild, new HandleWithCriteriaDep<T, C, D>() { callMethod = a, method = ObjectPortalMethod.InsertChild });
        }
    }
}
