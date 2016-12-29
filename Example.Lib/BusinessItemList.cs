using Csla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using ObjectPortal;
using Example.Dal;
using Csla.Serialization.Mobile;

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
            regs.HandleCreateChildWithDependency((BusinessItemList bo, System.Tuple<IObjectPortal<IBusinessItem>, IMobileDependency<IObjectPortal<IBusinessItem>>> d) 
                => bo.CreateChild(d.Item1, d.Item2));

            regs.HandleCreateChild((BusinessItemList bo, Guid criteria, IObjectPortal<IBusinessItem> d) 
                => bo.CreateChild(criteria, d));

            regs.HandleFetchChildWithDependency((BusinessItemList bo, System.Tuple<IObjectPortal<IBusinessItem>, IBusinessItemDal, IMobileDependency<IObjectPortal<IBusinessItem>>> d) 
                => bo.FetchChild(d.Item1, d.Item2, d.Item3));

            regs.HandleFetchChild((BusinessItemList bo, Criteria criteria, System.Tuple<IObjectPortal<IBusinessItem>, IBusinessItemDal, IMobileDependency<IObjectPortal<IBusinessItem>>> d) 
                => bo.FetchChild(criteria, d.Item1, d.Item2, d.Item3));

            regs.HandleUpdateChildWithDependency((BusinessItemList bo, IObjectPortal<IBusinessItem> d) => bo.UpdateChild(d));
            // TODO : Discuss - Same method. Assume this will be handled by the replacement for FieldManager.UpdateChildren() since it is a list
            regs.HandleInsertChildWithDependency((BusinessItemList bo, IObjectPortal<IBusinessItem> d) => bo.UpdateChild(d));

        }

        public IBusinessItem CreateAddChild()
        {
            var newChild = _newChild.Dependency.CreateChild();

            this.Add(newChild);

            return newChild;
        }

        IMobileDependency<IObjectPortal<IBusinessItem>> _newChild;

        public void CreateChild(IObjectPortal<IBusinessItem> op, IMobileDependency<IObjectPortal<IBusinessItem>> newChild)
        {
            this.Add(op.CreateChild());
            this._newChild = newChild;
        }

        public void CreateChild(Guid criteria, IObjectPortal<IBusinessItem> op)
        {
            this.Add(op.CreateChild(criteria));
        }

        public void FetchChild(IObjectPortal<IBusinessItem> op, IBusinessItemDal dal, IMobileDependency<IObjectPortal<IBusinessItem>> newChild)
        {
            var dtos = dal.Fetch();

            foreach (var d in dtos)
            {
                Add(op.FetchChild(d));
            }

            this._newChild = newChild;
        }

        public void FetchChild(CriteriaBase criteria, IObjectPortal<IBusinessItem> op, IBusinessItemDal dal, IMobileDependency<IObjectPortal<IBusinessItem>> newChild)
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

            this._newChild = newChild;
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

        // TODO : Make these generic
        // Probably take some changes to MobileFormatter but that's ok in the future

        protected override void OnGetChildren(SerializationInfo info, MobileFormatter formatter)
        {
            base.OnGetChildren(info, formatter);

            var mdInfo = formatter.SerializeObject(_newChild);
            info.AddChild(nameof(_newChild), mdInfo.ReferenceId);

        }

        protected override void OnSetChildren(SerializationInfo info, MobileFormatter formatter)
        {
            base.OnSetChildren(info, formatter);

            var mdInfo = info.Children[nameof(_newChild)];
            _newChild = (IMobileDependency<IObjectPortal<IBusinessItem>>)  formatter.GetObject(mdInfo.ReferenceId);

        }

    }
}
