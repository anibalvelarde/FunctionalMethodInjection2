using Example.Dal;
using ObjectPortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Example.Lib
{
    public interface IBusinessItemList : Csla.IBusinessListBase<IBusinessItem>, ObjectPortal.IDPBusinessObject
    {

        //void AddChild();
        //IBusinessItem AddCreateChild();
        //IBusinessItem AddCreateChild(Guid criteria);
    }
}
