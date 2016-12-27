using Autofac;
using ObjectPortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPortal
{


    [Serializable]
    public class DPBusinessBase<T> : Csla.BusinessBase<T>, IDPBusinessObject
        where T : DPBusinessBase<T>
    {


        ILifetimeScope IDPBusinessObject.scope { get; set; } // In the actual implementation we would not use a service locator. Limited by CSLA


    }
}
