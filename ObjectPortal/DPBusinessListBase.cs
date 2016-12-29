using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPortal
{
    [Serializable]
    public class DtoBusinessListBase<T, C> : Csla.BusinessListBase<T, C>, IDPBusinessObject, IBusinessObjectScope
        where C : Csla.Core.IEditableBusinessObject
        where T:DtoBusinessListBase<T, C>
    {

        ILifetimeScope IBusinessObjectScope.scope { get; set; } // In the actual implementation we would not use a service locator. Limited by CSLA


    }
}
