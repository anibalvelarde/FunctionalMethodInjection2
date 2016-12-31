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

    public interface IHandleRegistration<T> : IHandleRegistration
    {
        void ExecuteMethod(T obj, ILifetimeScope scope);
    }

    public interface IHandleRegistrationDependency<T, D> : IHandleRegistration<T>
    {
    }

    public interface IHandleRegistrationCriteria<T>
    {
        Type CriteriaType { get; }

        void ExecuteMethod(T obj, ILifetimeScope scope, object criteria);

    }

    public interface IHandleRegistrationCriteria<T, C> : IHandleRegistrationCriteria<T>, IHandleRegistration
    {
        void ExecuteMethod(T obj, ILifetimeScope scope, C criteria);
    }

    public interface IHandleRegistration<T, C, D> : IHandleRegistrationCriteria<T, C>
    {
    }

    public class Handle<T> : IHandleRegistration<T>
    {
        public Action<T> callMethod { get; set; }

        public ObjectPortalMethod method { get; set; }

        public virtual void ExecuteMethod(T obj, ILifetimeScope scope)
        {
            callMethod(obj);
        }

    }

    public class HandleDependency<T, D> : IHandleRegistrationDependency<T, D>
    {
        public Action<T, D> callMethod { get; set; }

        public ObjectPortalMethod method { get; set; }

        public void ExecuteMethod(T obj, ILifetimeScope scope)
        {
            D dep = scope.Resolve<D>();

            callMethod(obj, dep);
        }

    }

    public class HandleCriteria<T, C> : IHandleRegistrationCriteria<T, C>
    {
        public Action<T, C> callMethod { get; set; }

        public ObjectPortalMethod method { get; set; }

        public void ExecuteMethod(T obj, ILifetimeScope scope, C criteria)
        {
            callMethod(obj, criteria);
        }

        public void ExecuteMethod(T obj, ILifetimeScope scope, object criteria)
        {
            ExecuteMethod(obj, scope, (C)criteria);
        }

        public Type CriteriaType { get { return typeof(C); } }

    }

    public class Handle<T, C, D> : IHandleRegistration<T, C, D>
    {
        public Action<T, C, D> callMethod { get; set; }

        public ObjectPortalMethod method { get; set; }

        public Type CriteriaType { get { return typeof(C); } }

        public void ExecuteMethod(T obj, ILifetimeScope scope, C criteria)
        {
            // Use type of D to create Action<T>

            D dep = scope.Resolve<D>();

            callMethod(obj, criteria, dep);

        }

        public void ExecuteMethod(T obj, ILifetimeScope scope, object criteria)
        {
            ExecuteMethod(obj, scope, (C)criteria);
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
        void Add(ObjectPortalMethod method, IHandleRegistration<T> reg);
        void Add<C>(ObjectPortalMethod method, IHandleRegistrationCriteria<T, C> reg);
        bool IsRegistered(Type t);
    }

    public class HandleRegistrations<T> : IHandleRegistrations<T>, IHandleRegistrations
    {
        ILifetimeScope rootContainer;

        public HandleRegistrations(ILifetimeScope rootContainer) // this should be marked SingleInstance so this should be the root container
        {
            this.rootContainer = rootContainer;

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

        public bool IsRegistered(Type t)
        {
            return rootContainer.IsRegistered(t);
        }

        private List<IHandleRegistration> regs { get; set; } = new List<IHandleRegistration>();

        public void Add(ObjectPortalMethod method, IHandleRegistration<T> reg)
        {
            if (regs.OfType<IHandleRegistration<T>>().Where(r => r.method == method).FirstOrDefault() != null)
            {
                throw new ObjectPortalOperationNotSupportedException($"Key is already present in registrations {method.ToString()}");
            }

            regs.Add(reg);
        }


        public void Add<C>(ObjectPortalMethod method, IHandleRegistrationCriteria<T, C> reg)
        {
            if (regs.OfType<IHandleRegistrationCriteria<T, C>>().Where(r => r.method == method).FirstOrDefault() != null)
            {
                throw new ObjectPortalOperationNotSupportedException($"Key is already present in registrations {method.ToString()} with type ${typeof(C).FullName}");
            }


            regs.Add(reg);
        }


        bool IHandleRegistrations.TryExecuteMethod(object obj, ObjectPortalMethod method, ILifetimeScope scope)
        {

            var reg = regs.OfType<IHandleRegistration<T>>().Where(r => r.method == method).SingleOrDefault();

            if (reg == null) { return false; }

            // TODO : Discuss - At some point needs to go from IBO to BO
            // Right now that point is right here
            reg.ExecuteMethod((T)obj, scope);

            return true;
        }

        bool IHandleRegistrations.TryExecuteMethod<C>(object obj, ObjectPortalMethod method, ILifetimeScope scope, C criteria)
        {
            var reg = regs.OfType<IHandleRegistrationCriteria<T, C>>().Where(r => r.method == method).SingleOrDefault();

            if (reg == null)
            {

                // TODO : Discuss
                // This works..but...
                var regB = regs.OfType<IHandleRegistrationCriteria<T>>().SingleOrDefault(r => r.CriteriaType.IsAssignableFrom(typeof(C)));

                regB.ExecuteMethod((T)obj, scope, criteria);

                return true;
            }

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

        private static Action<T, C, D> CreateAction<T, C, D>(MethodInfo businessObjectMethod)
        {
            return (bo, c, d) => { businessObjectMethod.Invoke(bo, new object[] { c, d }); };
        }

        private static void HandleDependency<T, D>(IHandleRegistrations<T> regs, MethodInfo methodInfo, ObjectPortalMethod method)
        {

            // Method has dependencies
            // Resolve each dependency

            // Need to create (BusinessItem bo, Guid c) => bo.CreateChild(c)


            var createMethodInfo = CreateAction<T, D>(methodInfo);

            regs.Add(method, new HandleDependency<T, D>() { callMethod = createMethodInfo, method = method });


        }

        private static void HandleCriteria<T, C>(IHandleRegistrations<T> regs, MethodInfo methodInfo, ObjectPortalMethod method)
        {

            // Method has dependencies
            // Resolve each dependency

            // Need to create (BusinessItem bo, Guid c) => bo.CreateChild(c)


            var createMethodInfo = CreateAction<T, C>(methodInfo);

            regs.Add(method, new HandleCriteria<T, C>() { callMethod = createMethodInfo, method = method });

        }

        private static void HandleCriteriaDependency<T, C, D>(IHandleRegistrations<T> regs, MethodInfo methodInfo, ObjectPortalMethod method)
        {

            // Method has dependencies
            // Resolve each dependency

            // Need to create (BusinessItem bo, Guid c) => bo.CreateChild(c)


            var createMethodInfo = CreateAction<T, C, D>(methodInfo);

            regs.Add(method, new Handle<T, C, D>() { callMethod = createMethodInfo, method = method });


        }

        //private static void HandleCriteria<T>(IHandleRegistrations<T> regs, string methodName, ObjectPortalMethod method)
        //{


        //    var methodInfo = typeof(T).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        //    var parameters = methodInfo.GetParameters();

        //    if (parameters.Count() == 0)
        //    {
        //        Action<T> callMethod = null;

        //        callMethod = (T bo) =>
        //        {
        //            methodInfo.Invoke(bo, null);
        //        };

        //        regs.Add(method, new Handle<T>() { callMethod = callMethod, method = ObjectPortalMethod.Create });

        //    }
        //    else if (parameters.Count() == 1)
        //    {

        //        var createMethodInfo = typeof(HandleRegistrationExtensions).GetMethod(nameof(_HandleDependency), BindingFlags.Static | BindingFlags.NonPublic)
        //            .MakeGenericMethod(new Type[] { typeof(T), parameters[0].ParameterType });

        //        createMethodInfo.Invoke(null, new object[] { regs, methodInfo, method });

        //    }

        //}



        //private static void _HandleCriteriaDep<T, C, D>(IHandleRegistrations<T> regs, MethodInfo methodInfo, ObjectPortalMethod method)
        //{
        //    var parameters = methodInfo.GetParameters();

        //    // Method has dependencies
        //    // Resolve each dependency

        //    // Need to create (BusinessItem bo, Guid c) => bo.CreateChild(c)

        //    if (parameters.Length == 2)
        //    {

        //        var createMethodInfo = CreateAction<T, C, D>(methodInfo);

        //        regs.Add(method, new Handle<T, C, D>() { callMethod = createMethodInfo, method = method });

        //        return;
        //    }

        //    throw new NotSupportedException("Only 2 parameter supported");

        //}

        private static void Handle<T>(IHandleRegistrations<T> regs, string methodName, ObjectPortalMethod method, bool? HasDependencies = null)
        {


            var methodInfos = typeof(T).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name == methodName);

            // TODO : Discuss and crush all my hopes and dreams...again.. ;-)
            // If you have multiple methods with the same name...that's ok!
            // Register each of them
            foreach (var methodInfo in methodInfos)
            {

                var parameters = methodInfo.GetParameters();
                MethodInfo createMethodInfo = null;

                if (parameters.Length < 1 || parameters.Length > 2)
                {
                    throw new Exception($"{method.ToString()} must take 1 (Criteria or Dependency) or 2 (Criteria, Dependency) parameters");
                }
                else if (parameters.Length == 1)
                {
                    // If there is only one parameter we
                    // and HasDependencies isn't set we use the
                    // Container to see if the one parameter is a dependency
                    // or a criteria

                    if (!HasDependencies.HasValue)
                    {
                        HasDependencies = regs.IsRegistered(parameters[0].ParameterType);
                    }

                    // Criteria only
                    if (!HasDependencies.Value)
                    {
                        createMethodInfo = typeof(HandleRegistrationExtensions).GetMethod(nameof(HandleCriteria), BindingFlags.Static | BindingFlags.NonPublic)
                            .MakeGenericMethod(new Type[] { typeof(T), parameters[0].ParameterType });
                    }
                    else
                    {
                        createMethodInfo = typeof(HandleRegistrationExtensions).GetMethod(nameof(HandleDependency), BindingFlags.Static | BindingFlags.NonPublic)
                            .MakeGenericMethod(new Type[] { typeof(T), parameters[0].ParameterType });
                    }

                }
                else if (parameters.Length == 2)
                {
                    // Criteria and Dependencies

                    createMethodInfo = typeof(HandleRegistrationExtensions).GetMethod(nameof(HandleCriteriaDependency), BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(new Type[] { typeof(T), parameters[0].ParameterType, parameters[1].ParameterType });


                }

                createMethodInfo.Invoke(null, new object[] { regs, methodInfo, method });
            }
        }

        public static void Create<T>(this IHandleRegistrations<T> regs, string methodName)
        {
            Handle<T>(regs, methodName, ObjectPortalMethod.Create);
        }

        public static void Create<T>(this IHandleRegistrations<T> regs, Action<T> a)
        {
            regs.Add(ObjectPortalMethod.Create, new Handle<T>() { callMethod = a, method = ObjectPortalMethod.Create });
        }

        public static void CreateDependency<T, D>(this IHandleRegistrations<T> regs, Action<T, D> a)
        {
            regs.Add(ObjectPortalMethod.Create, new HandleDependency<T, D>() { callMethod = a, method = ObjectPortalMethod.Create });
        }

        public static void CreateCriteria<T, C>(this IHandleRegistrations<T> regs, Action<T, C> a)
        {
            regs.Add<C>(ObjectPortalMethod.Create, new HandleCriteria<T, C>() { callMethod = a, method = ObjectPortalMethod.Create });
        }

        public static void CreateCriteria<T, C, D>(this IHandleRegistrations<T> regs, Action<T, C, D> a)
        {
            regs.Add<C>(ObjectPortalMethod.Create, new Handle<T, C, D>() { callMethod = a, method = ObjectPortalMethod.Create });
        }

        public static void CreateChild<T>(this IHandleRegistrations<T> regs, string methodName)
        {
            Handle<T>(regs, methodName, ObjectPortalMethod.CreateChild);
        }

        public static void CreateChild<T>(this IHandleRegistrations<T> regs, Action<T> a)
        {
            regs.Add(ObjectPortalMethod.CreateChild, new Handle<T>() { callMethod = a, method = ObjectPortalMethod.CreateChild });
        }

        public static void CreateChildDependency<T, D>(this IHandleRegistrations<T> regs, Action<T, D> a)
        {
            regs.Add(ObjectPortalMethod.CreateChild, new HandleDependency<T, D>() { callMethod = a, method = ObjectPortalMethod.CreateChild });
        }

        public static void CreateChildCriteria<T, C>(this IHandleRegistrations<T> regs, Action<T, C> a)
        {
            regs.Add<C>(ObjectPortalMethod.CreateChild, new HandleCriteria<T, C>() { callMethod = a, method = ObjectPortalMethod.CreateChild });
        }

        public static void CreateChildCriteria<T, C, D>(this IHandleRegistrations<T> regs, Action<T, C, D> a)
        {
            regs.Add<C>(ObjectPortalMethod.CreateChild, new Handle<T, C, D>() { callMethod = a, method = ObjectPortalMethod.CreateChild });
        }


        public static void Fetch<T>(this IHandleRegistrations<T> regs, string methodName)
        {
            Handle<T>(regs, methodName, ObjectPortalMethod.Fetch);
        }

        public static void Fetch<T>(this IHandleRegistrations<T> regs, Action<T> a)
        {
            regs.Add(ObjectPortalMethod.Fetch, new Handle<T>() { callMethod = a, method = ObjectPortalMethod.Fetch });
        }

        public static void FetchDependency<T, D>(this IHandleRegistrations<T> regs, Action<T, D> a)
        {
            regs.Add(ObjectPortalMethod.Fetch, new HandleDependency<T, D>() { callMethod = a, method = ObjectPortalMethod.Fetch });
        }

        public static void FetchCriteria<T, C>(this IHandleRegistrations<T> regs, Action<T, C> a)
        {
            regs.Add<C>(ObjectPortalMethod.Fetch, new HandleCriteria<T, C>() { callMethod = a, method = ObjectPortalMethod.Fetch });
        }

        public static void FetchCriteria<T, C, D>(this IHandleRegistrations<T> regs, Action<T, C, D> a)
        {
            regs.Add<C>(ObjectPortalMethod.Fetch, new Handle<T, C, D>() { callMethod = a, method = ObjectPortalMethod.Fetch });
        }

        public static void FetchChild<T>(this IHandleRegistrations<T> regs, string methodName)
        {
            Handle<T>(regs, methodName, ObjectPortalMethod.FetchChild);
        }

        public static void FetchChild<T>(this IHandleRegistrations<T> regs, Action<T> a)
        {
            regs.Add(ObjectPortalMethod.FetchChild, new Handle<T>() { callMethod = a, method = ObjectPortalMethod.FetchChild });
        }

        public static void FetchChildDependency<T, D>(this IHandleRegistrations<T> regs, Action<T, D> a)
        {
            regs.Add(ObjectPortalMethod.FetchChild, new HandleDependency<T, D>() { callMethod = a, method = ObjectPortalMethod.FetchChild });
        }

        public static void FetchChildCriteria<T, C>(this IHandleRegistrations<T> regs, Action<T, C> a)
        {
            regs.Add<C>(ObjectPortalMethod.FetchChild, new HandleCriteria<T, C>() { callMethod = a, method = ObjectPortalMethod.FetchChild });
        }

        public static void FetchChildCriteria<T, C, D>(this IHandleRegistrations<T> regs, Action<T, C, D> a)
        {
            regs.Add<C>(ObjectPortalMethod.FetchChild, new Handle<T, C, D>() { callMethod = a, method = ObjectPortalMethod.FetchChild });
        }

    }
}
