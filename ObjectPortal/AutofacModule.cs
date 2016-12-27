using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace ObjectPortal
{
    public class AutofacModule : Autofac.Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterGeneric(typeof(ObjectPortal<>)).As(typeof(IObjectPortal<>));


            builder.RegisterGeneric(typeof(HandleRegistrations<>)).As(typeof(IHandleRegistrations<>)).SingleInstance();


        }

    }
}
