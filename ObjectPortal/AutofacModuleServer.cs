using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Csla.Server;
using System.Reflection;

namespace ObjectPortal
{
    public class AutofacModuleServer : Autofac.Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterGeneric(typeof(ObjectPortal<>)).As(typeof(IObjectPortal<>));

            builder.RegisterGeneric(typeof(Handle<>)).As(typeof(IHandleRegistration<>));
            builder.RegisterGeneric(typeof(Handle<,,>)).As(typeof(IHandleRegistration<,,>));
            builder.RegisterGeneric(typeof(HandleCriteria<,>)).As(typeof(IHandleRegistrationCriteria<,>));
            builder.RegisterGeneric(typeof(HandleDependency<,>)).As(typeof(IHandleRegistrationDependency<,>));

            builder.RegisterGeneric(typeof(HandleRegistrations<>)).As(typeof(IHandleRegistrations<>)).SingleInstance();

            builder.RegisterGeneric(typeof(MobileDependency<>))
                .As(typeof(IMobileDependency<>));

            builder.RegisterGeneric(typeof(MobileObjectWrapper<>))
                .As(typeof(IMobileObjectWrapper<>));

            builder.RegisterGeneric(typeof(MobileObjectWrapper<,>))
                .As(typeof(IMobileObjectWrapper<,>));

            builder.RegisterType<ObjectPortalActivator>().As<IDataPortalActivator>();

            builder.RegisterType<MobileDependencyList>()
                .AsSelf()
                .InstancePerLifetimeScope();

            builder.RegisterGeneric(typeof(Csla.DataPortal<>));



            // TODO : Discuss
            // Does it make life easier to be able to switch out implementations
            // with the module??
            // This is the third spot I've moved this logic! :-D
            builder.Register<HandleRegistrationFromMethodName>(cc =>
            {
                return new HandleRegistrationFromMethodName((Operation operation, string methodName, bool? hasDependencies, System.Type type, ILifetimeScope scope) =>
                {
                    List<IHandleRegistration> result = new List<IHandleRegistration>();

                    var methodInfos = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name == methodName);

                    foreach (var methodInfo in methodInfos)
                    {

                        var parameters = methodInfo.GetParameters();

                        if (parameters.Length == 1)
                        {
                            // If there is only one parameter we
                            // and HasDependencies isn't set we use the
                            // Container to see if the one parameter is a dependency
                            // or a criteria

                            if (!hasDependencies.HasValue)
                            {
                                hasDependencies = cc.IsRegistered(parameters[0].ParameterType);
                            }


                            // Criteria only
                            // TODO : Discuss. Nice but probably not safe
                            // Decide if it's criteria or dependency depending on whether it's registered or not
                            if (!hasDependencies.Value)
                            {

                                Delegate func = (Delegate)scope.Resolve(typeof(Func<,,>).MakeGenericType(new Type[] { typeof(Operation), typeof(MethodInfo)
                                , typeof(IHandleRegistrationCriteria<,>).MakeGenericType(new Type[] { type, parameters[0].ParameterType }) }));

                                var reg = (IHandleRegistration)func.DynamicInvoke(new object[] { operation, methodInfo });

                                result.Add(reg);

                            }
                            else
                            {

                                Delegate func = (Delegate)scope.Resolve(typeof(Func<,,>).MakeGenericType(new Type[] { typeof(Operation), typeof(MethodInfo)
                                , typeof(IHandleRegistrationDependency<,>).MakeGenericType(new Type[] { type, parameters[0].ParameterType }) }));

                                var reg = (IHandleRegistration)func.DynamicInvoke(new object[] { operation, methodInfo });

                                result.Add(reg);
                            }

                        }
                        else if (parameters.Length == 2)
                        {
                            // Criteria and Dependencies

                            Delegate func = (Delegate)scope.Resolve(typeof(Func<,,>).MakeGenericType(new Type[] { typeof(Operation), typeof(MethodInfo)
                            , typeof(IHandleRegistration<,,>).MakeGenericType(new Type[] { type, parameters[0].ParameterType, parameters[1].ParameterType }) }));

                            var reg = (IHandleRegistration)func.DynamicInvoke(new object[] { operation, methodInfo });

                            result.Add(reg);

                        }
                        else
                        {
                            throw new Exception($"{operation.ToString()} must take 1 (Criteria or Dependency) or 2 (Criteria, Dependency) parameters");
                        }

                    }

                    return result;


                });


            });

        }

    }
}
