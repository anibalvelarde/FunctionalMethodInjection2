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

    public enum Operation
    {
        Create, CreateChild,
        Fetch, FetchChild,
        Update, UpdateChild,
        Insert, InsertChild
    }


    public interface IHandleRegistration
    {
        Operation Operation { get; }
    }

    public interface IHandleRegistration<T> : IHandleRegistration
    {
        void ExecuteMethod(T obj, ILifetimeScope scope);
    }

    public interface IHandleRegistrationDependency<T, D> : IHandleRegistration<T>
    {
    }

    public interface IHandleRegistrationCriteria<T> : IHandleRegistration
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

        public Handle(Operation operation, MethodInfo method)
        {
            this.Operation = operation;
            this.Method = (o) => method.Invoke(o, null);
        }

        public Handle(Operation operation, Action<T> method)
        {
            this.Method = method;
            this.Operation = operation;
        }

        public Action<T> Method { get; private set; }

        public Operation Operation { get; private set; }

        public virtual void ExecuteMethod(T obj, ILifetimeScope scope)
        {
            Method(obj);
        }

    }


    public class HandleDependency<T, D> : IHandleRegistrationDependency<T, D>
    {



        public HandleDependency(Operation operation, MethodInfo method)
        {
            this.Operation = operation;
            this.Method = CreateActions.CreateAction<T, D>(method);
        }

        public HandleDependency(Operation operation, Action<T, D> method)
        {
            this.Operation = operation;
            this.Method = method;
        }

        public Action<T, D> Method { get; private set; }

        public Operation Operation { get; private set; }

        public void ExecuteMethod(T obj, ILifetimeScope scope)
        {
            D dep = scope.Resolve<D>();

            Method(obj, dep);
        }

    }

    public class HandleCriteria<T, C> : IHandleRegistrationCriteria<T, C>
    {



        public HandleCriteria(Operation operation, MethodInfo method)
        {
            this.Operation = operation;
            this.Method = CreateActions.CreateAction<T, C>(method);
        }

        public HandleCriteria(Operation operation, Action<T, C> method)
        {
            this.Operation = operation;
            this.Method = method;
        }

        public Action<T, C> Method { get; set; }

        public Operation Operation { get; set; }

        public void ExecuteMethod(T obj, ILifetimeScope scope, C criteria)
        {
            Method(obj, criteria);
        }

        public void ExecuteMethod(T obj, ILifetimeScope scope, object criteria)
        {
            ExecuteMethod(obj, scope, (C)criteria);
        }

        public Type CriteriaType { get { return typeof(C); } }

    }


    public class Handle<T, C, D> : IHandleRegistration<T, C, D>
    {

        internal static Handle<T1, C1, D1> CreateHandle<T1, C1, D1>(Operation operation, MethodInfo method)
        {
            return new Handle<T1, C1, D1>(operation, method);
        }

        public Handle(Operation operation, MethodInfo method)
        {
            this.Operation = operation;
            this.Method = CreateActions.CreateAction<T, C, D>(method);
        }

        public Handle(Operation operation, Action<T, C, D> method)
        {
            this.Operation = operation;
            this.Method = method;
        }

        public Action<T, C, D> Method { get; set; }

        public Operation Operation { get; set; }

        public Type CriteriaType { get { return typeof(C); } }

        public void ExecuteMethod(T obj, ILifetimeScope scope, C criteria)
        {
            // Use type of D to create Action<T>

            D dep = scope.Resolve<D>();

            Method(obj, criteria, dep);

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


        bool TryExecuteMethod(object obj, Operation operation, ILifetimeScope scope);
        bool TryExecuteMethod<C>(object obj, Operation operation, ILifetimeScope scope, C criteria);

    }

    public interface IHandleRegistrations<T>
    {
        void Add(Operation operation, string methodName, bool? hasDependencies = null);
        void Add(Operation operation, Action<T> action);
        void Add<CD>(Operation operation, Action<T, CD> action);
        void AddCriteria<D>(Operation operation, Action<T, D> action);
        void AddDependency<D>(Operation operation, Action<T, D> action);
        void Add<C, D>(Operation operation, Action<T, C, D> action);

    }

    public delegate void LoadHandleRegistrations<T>(IHandleRegistrations<T> regs);

    internal delegate List<IHandleRegistration> HandleRegistrationFromMethodName(Operation operation, string methodName, bool? hasDependencies, Type type, ILifetimeScope scope);

    public class HandleRegistrations<T> : IHandleRegistrations<T>, IHandleRegistrations
    {
        ILifetimeScope rootContainer;

        public HandleRegistrations(ILifetimeScope rootContainer) // this should be marked SingleInstance so this should be the root container
        {
            this.rootContainer = rootContainer;

            // TODO Discuss
            // Try to use as little reflection as possible
            // Let DI do the heavy lifting

            var load = rootContainer.ResolveOptional<LoadHandleRegistrations<T>>();

            if (load == null)
            {
                throw new Exception($"You must register a LoadHandleRegistration<{typeof(T).FullName}> in the container;");
            }

            load(this);

            //// Pass this object to the static Handle method defined for the type

            //var type = typeof(T);

            //var att = type.GetCustomAttribute<HandleMethodAttribute>();

            //if (att == null)
            //{
            //    throw new ObjectPortalOperationNotSupportedException($"No HandleMethodAttribute on {type.FullName}");
            //}

            //var methods = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Where(x => x.Name == att.MethodName).ToList();
            //MethodInfo handleMethod;

            //if (methods.Count == 1)
            //{
            //    handleMethod = methods[0];
            //}
            //else
            //{
            //    throw new ObjectPortalOperationNotSupportedException($"Invalid number of Handle methods or method not found [{methods.Count}] on {type.FullName}");
            //}

            //handleMethod.Invoke(null, new object[] { this });

        }

        public bool IsRegistered(Type t)
        {
            return rootContainer.IsRegistered(t);
        }

        private List<IHandleRegistration> regs { get; set; } = new List<IHandleRegistration>();


        public void Add(Operation operation, string methodName, bool? hasDependencies = null)
        {

            // TODO : Discuss and crush all my hopes and dreams...again.. ;-)
            // If you have multiple methods with the same name...that's ok!
            // Register each of them

            // TODO : Discuss, use DependencyInjection here? You could register Func<string, IHandleRegistration>
            // and keep all of the reflection code OUTSIDE of Ystari...
            // THoght: what about the error stack??

            var newRegs = rootContainer.Resolve<HandleRegistrationFromMethodName>()(operation, methodName, hasDependencies, typeof(T), rootContainer);

            regs.AddRange(newRegs);

            //var methodInfos = typeof(T).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name == methodName);

            //foreach (var methodInfo in methodInfos)
            //{

            //    var parameters = methodInfo.GetParameters();

            //    if (parameters.Length < 1 || parameters.Length > 2)
            //    {
            //        throw new Exception($"{operation.ToString()} must take 1 (Criteria or Dependency) or 2 (Criteria, Dependency) parameters");
            //    }
            //    else if (parameters.Length == 1)
            //    {
            //        // If there is only one parameter we
            //        // and HasDependencies isn't set we use the
            //        // Container to see if the one parameter is a dependency
            //        // or a criteria

            //        if (!hasDependencies.HasValue)
            //        {
            //            hasDependencies = rootContainer.IsRegistered(parameters[0].ParameterType);
            //        }


            //        // Criteria only
            //        // TODO : Discuss. Nice but probably not safe
            //        // Decide if it's criteria or dependency depending on whether it's registered or not
            //        if (!hasDependencies.Value)
            //        {

            //            Delegate func = (Delegate) rootContainer.Resolve(typeof(Func<,,>).MakeGenericType(new Type[] { typeof(Operation), typeof(MethodInfo)
            //                , typeof(IHandleRegistrationCriteria<,>).MakeGenericType(new Type[] { typeof(T), parameters[0].ParameterType }) }));

            //            var reg = (IHandleRegistration) func.DynamicInvoke(new object[] { operation, methodInfo });

            //            regs.Add(reg);

            //        }
            //        else
            //        {

            //            Delegate func = (Delegate)rootContainer.Resolve(typeof(Func<,,>).MakeGenericType(new Type[] { typeof(Operation), typeof(MethodInfo)
            //                , typeof(IHandleRegistrationDependency<,>).MakeGenericType(new Type[] { typeof(T), parameters[0].ParameterType }) }));

            //            var reg = (IHandleRegistration)func.DynamicInvoke(new object[] { operation, methodInfo });

            //            regs.Add(reg);
            //        }

            //    }
            //    else if (parameters.Length == 2)
            //    {
            //        // Criteria and Dependencies

            //        //createMethodInfo = typeof(HandleRegistrationExtensions).GetMethod(nameof(HandleCriteriaDependency), BindingFlags.Static | BindingFlags.NonPublic)
            //        //    .MakeGenericMethod(new Type[] { typeof(T), parameters[0].ParameterType, parameters[1].ParameterType });


            //        Delegate func = (Delegate)rootContainer.Resolve(typeof(Func<,,>).MakeGenericType(new Type[] { typeof(Operation), typeof(MethodInfo)
            //                , typeof(IHandleRegistration<,,>).MakeGenericType(new Type[] { typeof(T), parameters[0].ParameterType, parameters[1].ParameterType }) }));

            //        var reg = (IHandleRegistration)func.DynamicInvoke(new object[] { operation, methodInfo });

            //        regs.Add(reg);

            //    }

            //}

        }

        public void Add(Operation operation, Action<T> action)
        {
            IHandleRegistration<T> reg = rootContainer.Resolve<Func<Operation, Action<T>, IHandleRegistration<T>>>()(operation, action);

            if (regs.OfType<IHandleRegistration<T>>().Where(r => r.Operation == operation).FirstOrDefault() != null)
            {
                throw new ObjectPortalOperationNotSupportedException($"Key is already present in registrations {operation.ToString()}");
            }

            regs.Add(reg);

        }

        public void Add<CD>(Operation operation, Action<T, CD> action)
        {
            // Not defined whether it is criteria or dependency
            throw new NotImplementedException();
        }


        public void AddCriteria<C>(Operation operation, Action<T, C> action)
        {

            IHandleRegistrationCriteria<T, C> reg = rootContainer.Resolve<Func<Operation, Action<T, C>, IHandleRegistrationCriteria<T, C>>>()(operation, action);


            if (regs.OfType<IHandleRegistrationCriteria<T, C>>().Where(r => r.Operation == operation).FirstOrDefault() != null)
            {
                throw new ObjectPortalOperationNotSupportedException($"Key is already present in registrations {operation.ToString()} with type ${typeof(C).FullName}");
            }


            regs.Add(reg);
        }


        public void AddDependency<D>(Operation operation, Action<T, D> action)
        {

            IHandleRegistrationDependency<T, D> reg = rootContainer.Resolve<Func<Operation, Action<T, D>, IHandleRegistrationDependency<T, D>>>()(operation, action);


            if (regs.OfType<IHandleRegistrationDependency<T, D>>().Where(r => r.Operation == operation).FirstOrDefault() != null)
            {
                throw new ObjectPortalOperationNotSupportedException($"Key is already present in registrations {operation.ToString()} with type ${typeof(D).FullName}");
            }


            regs.Add(reg);
        }

        public void Add<C, D>(Operation operation, Action<T, C, D> action)
        {

            IHandleRegistration<T, C, D> reg = rootContainer.Resolve<Func<Operation, Action<T, C, D>, IHandleRegistration<T, C, D>>>()(operation, action);


            if (regs.OfType<IHandleRegistrationCriteria<T, C>>().Where(r => r.Operation == operation).FirstOrDefault() != null)
            {
                throw new ObjectPortalOperationNotSupportedException($"Key is already present in registrations {operation.ToString()} with type ${typeof(C).FullName}");
            }


            regs.Add(reg);
        }


        bool IHandleRegistrations.TryExecuteMethod(object obj, Operation operation, ILifetimeScope scope)
        {

            var reg = regs.OfType<IHandleRegistration<T>>().Where(r => r.Operation == operation).SingleOrDefault();

            if (reg == null) { return false; }

            // TODO : Discuss - At some point needs to go from IBO to BO
            // Right now that point is right here
            reg.ExecuteMethod((T)obj, scope);

            return true;
        }

        bool IHandleRegistrations.TryExecuteMethod<C>(object obj, Operation operation, ILifetimeScope scope, C criteria)
        {
            var reg = regs.OfType<IHandleRegistrationCriteria<T, C>>().Where(r => r.Operation == operation).SingleOrDefault();

            if (reg == null)
            {

                // TODO : Discuss
                // This works..but...
                var regB = regs.OfType<IHandleRegistrationCriteria<T>>().SingleOrDefault(r => r.Operation == operation && r.CriteriaType.IsAssignableFrom(typeof(C)));

                regB.ExecuteMethod((T)obj, scope, criteria);

                return true;
            }

            if (reg == null) { return false; }

            reg.ExecuteMethod((T)obj, scope, criteria);

            return true;

        }


    }

    internal static class CreateActions
    {
        public static Action<T, C> CreateAction<T, C>(MethodInfo businessObjectMethod)
        {
            return (bo, d) => { businessObjectMethod.Invoke(bo, new object[] { d }); };
        }

        public static Action<T, C, D> CreateAction<T, C, D>(MethodInfo businessObjectMethod)
        {
            return (bo, c, d) => { businessObjectMethod.Invoke(bo, new object[] { c, d }); };
        }

    }

    public static class HandleRegistrationExtensions
    {




        //private static void HandleDependency<T, D>(IHandleRegistrations<T> regs, MethodInfo methodInfo, Operation operation)
        //{

        //    // Method has dependencies
        //    // Resolve each dependency

        //    // Need to create (BusinessItem bo, Guid c) => bo.CreateChild(c)


        //    var createMethodInfo = CreateAction<T, D>(methodInfo);

        //    regs.Add(operation, createMethodInfo);


        //}

        //private static void HandleCriteria<T, C>(IHandleRegistrations<T> regs, MethodInfo methodInfo, Operation operation)
        //{

        //    // Method has dependencies
        //    // Resolve each dependency

        //    // Need to create (BusinessItem bo, Guid c) => bo.CreateChild(c)


        //    var createMethodInfo = CreateAction<T, C>(methodInfo);

        //    regs.Add(operation, new HandleCriteria<T, C>() { callMethod = createMethodInfo, Operation = method });

        //}

        //private static void HandleCriteriaDependency<T, C, D>(IHandleRegistrations<T> regs, MethodInfo methodInfo, Operation operation)
        //{

        //    // Method has dependencies
        //    // Resolve each dependency

        //    // Need to create (BusinessItem bo, Guid c) => bo.CreateChild(c)


        //    var createMethodInfo = CreateAction<T, C, D>(methodInfo);

        //    regs.Add(operation, new Handle<T, C, D>() { callMethod = createMethodInfo, Operation = method });


        //}

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

        //        regs.Add(operation, new Handle<T>() { callMethod = callMethod, method = ObjectPortalMethod.Create });

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

        //        regs.Add(operation, new Handle<T, C, D>() { callMethod = createMethodInfo, method = method });

        //        return;
        //    }

        //    throw new NotSupportedException("Only 2 parameter supported");

        //}

        //private static void Handle<T>(IHandleRegistrations<T> regs, string methodName, ObjectPortalMethod method, bool? HasDependencies = null)
        //{


        //    var methodInfos = typeof(T).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name == operationName);

        //    // TODO : Discuss and crush all my hopes and dreams...again.. ;-)
        //    // If you have multiple methods with the same name...that's ok!
        //    // Register each of them
        //    foreach (var methodInfo in methodInfos)
        //    {

        //        var parameters = methodInfo.GetParameters();
        //        MethodInfo createMethodInfo = null;

        //        if (parameters.Length < 1 || parameters.Length > 2)
        //        {
        //            throw new Exception($"{method.ToString()} must take 1 (Criteria or Dependency) or 2 (Criteria, Dependency) parameters");
        //        }
        //        else if (parameters.Length == 1)
        //        {
        //            // If there is only one parameter we
        //            // and HasDependencies isn't set we use the
        //            // Container to see if the one parameter is a dependency
        //            // or a criteria

        //            if (!HasDependencies.HasValue)
        //            {
        //                HasDependencies = regs.IsRegistered(parameters[0].ParameterType);
        //            }

        //            // Criteria only
        //            if (!HasDependencies.Value)
        //            {
        //                createMethodInfo = typeof(HandleRegistrationExtensions).GetMethod(nameof(HandleCriteria), BindingFlags.Static | BindingFlags.NonPublic)
        //                    .MakeGenericMethod(new Type[] { typeof(T), parameters[0].ParameterType });
        //            }
        //            else
        //            {
        //                createMethodInfo = typeof(HandleRegistrationExtensions).GetMethod(nameof(HandleDependency), BindingFlags.Static | BindingFlags.NonPublic)
        //                    .MakeGenericMethod(new Type[] { typeof(T), parameters[0].ParameterType });
        //            }

        //        }
        //        else if (parameters.Length == 2)
        //        {
        //            // Criteria and Dependencies

        //            createMethodInfo = typeof(HandleRegistrationExtensions).GetMethod(nameof(HandleCriteriaDependency), BindingFlags.Static | BindingFlags.NonPublic)
        //                .MakeGenericMethod(new Type[] { typeof(T), parameters[0].ParameterType, parameters[1].ParameterType });


        //        }

        //        createMethodInfo.Invoke(null, new object[] { regs, methodInfo, method });
        //    }
        //}

        public static void Create<T>(this IHandleRegistrations<T> regs, string methodName)
        {
            regs.Add(Operation.Create, methodName);
        }

        public static void Create<T>(this IHandleRegistrations<T> regs, Action<T> a)
        {
            regs.Add(Operation.Create, a);
        }

        public static void CreateDependency<T, D>(this IHandleRegistrations<T> regs, Action<T, D> a)
        {
            regs.AddDependency(Operation.Create, a);
        }

        public static void CreateCriteria<T, C>(this IHandleRegistrations<T> regs, Action<T, C> a)
        {
            regs.AddCriteria(Operation.Create, a);
        }

        public static void CreateCriteria<T, C, D>(this IHandleRegistrations<T> regs, Action<T, C, D> a)
        {
            regs.Add(Operation.Create, a);
        }


        public static void CreateChild<T>(this IHandleRegistrations<T> regs, string methodName)
        {
            regs.Add(Operation.CreateChild, methodName);
        }

        public static void CreateChild<T>(this IHandleRegistrations<T> regs, Action<T> a)
        {
            regs.Add(Operation.CreateChild, a);
        }

        public static void CreateChildDependency<T, D>(this IHandleRegistrations<T> regs, Action<T, D> a)
        {
            regs.AddDependency(Operation.CreateChild, a);
        }

        public static void CreateChildCriteria<T, C>(this IHandleRegistrations<T> regs, Action<T, C> a)
        {
            regs.AddCriteria(Operation.CreateChild, a);
        }

        public static void CreateChildCriteria<T, C, D>(this IHandleRegistrations<T> regs, Action<T, C, D> a)
        {
            regs.Add(Operation.CreateChild, a);
        }

        public static void Fetch<T>(this IHandleRegistrations<T> regs, string methodName)
        {
            regs.Add(Operation.Fetch, methodName);
        }

        public static void Fetch<T>(this IHandleRegistrations<T> regs, Action<T> a)
        {
            regs.Add(Operation.Fetch, a);
        }

        public static void FetchDependency<T, D>(this IHandleRegistrations<T> regs, Action<T, D> a)
        {
            regs.AddDependency(Operation.Fetch, a);
        }

        public static void FetchCriteria<T, C>(this IHandleRegistrations<T> regs, Action<T, C> a)
        {
            regs.AddCriteria(Operation.Fetch, a);
        }

        public static void FetchCriteria<T, C, D>(this IHandleRegistrations<T> regs, Action<T, C, D> a)
        {
            regs.Add(Operation.Fetch, a);
        }


        public static void FetchChild<T>(this IHandleRegistrations<T> regs, string methodName)
        {
            regs.Add(Operation.FetchChild, methodName);
        }

        public static void FetchChild<T>(this IHandleRegistrations<T> regs, Action<T> a)
        {
            regs.Add(Operation.FetchChild, a);
        }

        public static void FetchChildDependency<T, D>(this IHandleRegistrations<T> regs, Action<T, D> a)
        {
            regs.AddDependency(Operation.FetchChild, a);
        }

        public static void FetchChildCriteria<T, C>(this IHandleRegistrations<T> regs, Action<T, C> a)
        {
            regs.AddCriteria(Operation.FetchChild, a);
        }

        public static void FetchChildCriteria<T, C, D>(this IHandleRegistrations<T> regs, Action<T, C, D> a)
        {
            regs.Add(Operation.FetchChild, a);
        }


        public static void Update<T>(this IHandleRegistrations<T> regs, string methodName)
        {
            regs.Add(Operation.Update, methodName);
        }

        public static void Update<T>(this IHandleRegistrations<T> regs, Action<T> a)
        {
            regs.Add(Operation.Update, a);
        }

        public static void UpdateDependency<T, D>(this IHandleRegistrations<T> regs, Action<T, D> a)
        {
            regs.AddDependency(Operation.Update, a);
        }

        public static void UpdateCriteria<T, C>(this IHandleRegistrations<T> regs, Action<T, C> a)
        {
            regs.AddCriteria(Operation.Update, a);
        }

        public static void UpdateCriteria<T, C, D>(this IHandleRegistrations<T> regs, Action<T, C, D> a)
        {
            regs.Add(Operation.Update, a);
        }


        public static void UpdateChild<T>(this IHandleRegistrations<T> regs, string methodName)
        {
            regs.Add(Operation.UpdateChild, methodName);
        }

        public static void UpdateChild<T>(this IHandleRegistrations<T> regs, Action<T> a)
        {
            regs.Add(Operation.UpdateChild, a);
        }

        public static void UpdateChildDependency<T, D>(this IHandleRegistrations<T> regs, Action<T, D> a)
        {
            regs.AddDependency(Operation.UpdateChild, a);
        }

        public static void UpdateChildCriteria<T, C>(this IHandleRegistrations<T> regs, Action<T, C> a)
        {
            regs.AddCriteria(Operation.UpdateChild, a);
        }

        public static void UpdateChildCriteria<T, C, D>(this IHandleRegistrations<T> regs, Action<T, C, D> a)
        {
            regs.Add(Operation.UpdateChild, a);
        }


        public static void Insert<T>(this IHandleRegistrations<T> regs, string methodName)
        {
            regs.Add(Operation.Insert, methodName);
        }

        public static void Insert<T>(this IHandleRegistrations<T> regs, Action<T> a)
        {
            regs.Add(Operation.Insert, a);
        }

        public static void InsertDependency<T, D>(this IHandleRegistrations<T> regs, Action<T, D> a)
        {
            regs.AddDependency(Operation.Insert, a);
        }

        public static void InsertCriteria<T, C>(this IHandleRegistrations<T> regs, Action<T, C> a)
        {
            regs.AddCriteria(Operation.Insert, a);
        }

        public static void InsertCriteria<T, C, D>(this IHandleRegistrations<T> regs, Action<T, C, D> a)
        {
            regs.Add(Operation.Insert, a);
        }


        public static void InsertChild<T>(this IHandleRegistrations<T> regs, string methodName)
        {
            regs.Add(Operation.InsertChild, methodName);
        }

        public static void InsertChild<T>(this IHandleRegistrations<T> regs, Action<T> a)
        {
            regs.Add(Operation.InsertChild, a);
        }

        public static void InsertChildDependency<T, D>(this IHandleRegistrations<T> regs, Action<T, D> a)
        {
            regs.AddDependency(Operation.InsertChild, a);
        }

        public static void InsertChildCriteria<T, C>(this IHandleRegistrations<T> regs, Action<T, C> a)
        {
            regs.AddCriteria(Operation.InsertChild, a);
        }

        public static void InsertChildCriteria<T, C, D>(this IHandleRegistrations<T> regs, Action<T, C, D> a)
        {
            regs.Add(Operation.InsertChild, a);
        }
    }
}
