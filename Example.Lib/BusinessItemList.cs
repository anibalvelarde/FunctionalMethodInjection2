using Csla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using ObjectPortal;
using Example.Dal;

namespace Example.Lib
{


    [Serializable]
    [HandleMethod(nameof(Handle))]
    internal class BusinessItemList : DtoBusinessListBase<BusinessItemList, IBusinessItem>, IBusinessItemList
    {

        public BusinessItemList()
        {

        }

        private static void Handle(IHandleRegistrations<BusinessItemList> regs)
        {
            regs.HandleCreateChildWithDependency((BusinessItemList bo, IObjectPortal<IBusinessItem> d) => bo.CreateChild(d));
            regs.HandleCreateChild((BusinessItemList bo, Guid criteria, IObjectPortal<IBusinessItem> d) 
                => bo.CreateChild(criteria, d));

            regs.HandleFetchChildWithDependency((BusinessItemList bo, System.Tuple<IObjectPortal<IBusinessItem>, IBusinessItemDal> d) 
                => bo.FetchChild(d.Item1, d.Item2));

            regs.HandleFetchChild((BusinessItemList bo, Criteria criteria, System.Tuple<IObjectPortal<IBusinessItem>, IBusinessItemDal> d) 
                => bo.FetchChild(criteria, d.Item1, d.Item2));

            regs.HandleUpdateChildWithDependency((BusinessItemList bo, IObjectPortal<IBusinessItem> d) => bo.UpdateChild(d));
            // TODO : Discuss - Same method. Assume this will be handled by the replacement for FieldManager.UpdateChildren()
            regs.HandleInsertChildWithDependency((BusinessItemList bo, IObjectPortal<IBusinessItem> d) => bo.UpdateChild(d));

        }

        public void CreateChild(IObjectPortal<IBusinessItem> op)
        {
            this.Add(op.CreateChild());
        }

        public void CreateChild(Guid criteria, IObjectPortal<IBusinessItem> op)
        {
            this.Add(op.CreateChild(criteria));
        }

        public void FetchChild(IObjectPortal<IBusinessItem> op, IBusinessItemDal dal)
        {
            var dtos = dal.Fetch();

            foreach (var d in dtos)
            {
                Add(op.FetchChild(d));
            }

        }

        public void FetchChild(CriteriaBase criteria, IObjectPortal<IBusinessItem> op, IBusinessItemDal dal)
        {

            var dtos = dal.Fetch(criteria.Guid);

            foreach (var d in dtos)
            {
                // We allow the Fetch calls (and delegates) to have multiple parameters
                // But the IHandleXYZ interface can only have one criteria as a parameter
                // with a tuple to handle multiple parameters
                // ObjectPortal will bridge the two by turning the multiple paramters to a tuple

                Add(op.FetchChild(Tuple.Create<CriteriaBase, BusinessItemDto>(criteria, d)));
            }

        }


        public void UpdateChild(IObjectPortal<IBusinessItem> op)
        {
            foreach (var i in this)
            {
                if (i.IsDirty)
                {
                    op.UpdateChild(i, Guid.NewGuid());
                }
            }
        }
    }
}
