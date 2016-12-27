using Example.Dal;
using ObjectPortal;
using System;

namespace Example.Lib
{
    public interface IBusinessItem : Csla.IBusinessBase, ObjectPortal.IDPBusinessObject
    {
        string Name { get; set; }
        Guid Criteria { get; set; }
        Guid ScopeID { get;  }
        Guid FetchChildID { get;  }
        Guid UpdatedID { get; }

    }
}