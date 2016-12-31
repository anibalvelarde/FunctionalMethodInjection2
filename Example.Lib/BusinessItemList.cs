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
            //regs.CreateChildWithDependency((BusinessItemList bo, System.Tuple<IObjectPortal<IBusinessItem>, IMobileDependency<IObjectPortal<IBusinessItem>>> d) 
            //    => bo.CreateChild(d.Item1, d.Item2));

            regs.CreateChild(nameof(CreateChildNoCriteria));

            // TODO : Discuss
            // Hmmm...if this wasn't a static method would that be better??
            // Could just send in the method signatures
            // Would be even better if the types could be derived
            //regs.HandleTrySomething((Action<Guid>)CreateChildGuid);

            //regs.CreateChild((BusinessItemList bo, Guid criteria, IObjectPortal<IBusinessItem> d) 
            //    => bo.CreateChildGuid(criteria, d));

            // TODO Discuss - Cleaner then above
            regs.CreateChild(nameof(CreateChildGuid));

            //regs.FetchChildDependency((BusinessItemList bo, System.Tuple<IObjectPortal<IBusinessItem>, IBusinessItemDal, IMobileDependency<IObjectPortal<IBusinessItem>>> d)
            //   => bo.FetchChild(d.Item1, d.Item2, d.Item3));
            regs.FetchChild(nameof(FetchChild));

            //regs.FetchChildCriteria((BusinessItemList bo, Criteria criteria, System.Tuple<IObjectPortal<IBusinessItem>, IBusinessItemDal, IMobileDependency<IObjectPortal<IBusinessItem>>> d)
             //   => bo.FetchChild(criteria, d.Item1, d.Item2, d.Item3));

           // regs.HandleUpdateChildWithDependency((BusinessItemList bo, IObjectPortal<IBusinessItem> d) => bo.UpdateChild(d));
            // TODO : Discuss - Same method. Assume this will be handled by the replacement for FieldManager.UpdateChildren() since it is a list
            //regs.HandleInsertChildWithDependency((BusinessItemList bo, IObjectPortal<IBusinessItem> d) => bo.UpdateChild(d));

        }

        public IBusinessItem CreateAddChild()
        {
            var newChild = _newChild.Dependency.CreateChild();

            this.Add(newChild);

            return newChild;
        }

        // If we had properties on Lists I think we could transfer this as a CSLA Property
        IMobileDependency<IObjectPortal<IBusinessItem>> _newChild;

        public void CreateChildNoCriteria(System.Tuple<IObjectPortal<IBusinessItem>, IMobileDependency<IObjectPortal<IBusinessItem>>> newChild)
        {
            this.Add(newChild.Item1.CreateChild());
            this._newChild = newChild.Item2;
        }

        public void CreateChildGuid(Guid criteria, IObjectPortal<IBusinessItem> op)
        {
            this.Add(op.CreateChild(criteria));
        }

        public void FetchChild(System.Tuple<IObjectPortal<IBusinessItem>, IBusinessItemDal, IMobileDependency<IObjectPortal<IBusinessItem>>> d)
        {
            var dtos = d.Item2.Fetch();

            foreach (var dto in dtos)
            {
                Add(d.Item1.FetchChild(dto));
            }

            this._newChild = d.Item3;
        }

        public void FetchChild(CriteriaBase criteria, System.Tuple<IObjectPortal<IBusinessItem>, IBusinessItemDal, IMobileDependency<IObjectPortal<IBusinessItem>>> d)
        {

            var dtos = d.Item2.Fetch(criteria.Guid);

            foreach (var dto in dtos)
            {
                // We allow the Fetch calls (and delegates) to have multiple parameters
                // But the IHandleXYZ interface can only have one criteria as a parameter
                // with a tuple to handle multiple parameters
                // ObjectPortal will bridge the two by turning the multiple paramters to a tuple

                Add(d.Item1.FetchChild(Tuple.Create<CriteriaBase, BusinessItemDto>(criteria, dto)));
            }

            this._newChild = d.Item3;
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
            _newChild = (IMobileDependency<IObjectPortal<IBusinessItem>>)formatter.GetObject(mdInfo.ReferenceId);

        }

    }
}
