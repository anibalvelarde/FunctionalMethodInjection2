using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Csla.Server;

namespace ObjectPortal
{
    public class AutofacModuleServer : Autofac.Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterGeneric(typeof(ObjectPortal<>)).As(typeof(IObjectPortal<>));
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

        }

    }
}
