using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Csla.Server;

namespace ObjectPortal
{
    public class AutofacModuleClient : AutofacModuleServer
    {

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            // Only difference
            builder.RegisterGeneric(typeof(ObjectPortal_DPWrapper<>)).As(typeof(IObjectPortal<>));

        }

    }
}
