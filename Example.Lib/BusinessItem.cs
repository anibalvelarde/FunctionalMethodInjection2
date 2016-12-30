using Autofac;
using Csla;
using Example.Dal;
using ObjectPortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Csla.Core;
using Csla.Rules;

namespace Example.Lib
{

    [Serializable]
    [HandleMethod(nameof(Handle))]
    internal class BusinessItem : DPBusinessBase<BusinessItem>, IBusinessItem
    {

        public BusinessItem()
        {
            Criteria = Guid.Empty;

        }

        private static void Handle(IHandleRegistrations<BusinessItem> reg)
        {
            reg.HandleCreate(nameof(CreateChild));

            reg.HandleCreateChild((BusinessItem bo, Guid c) => bo.CreateChild(c));


            reg.HandleFetchChild((BusinessItem bo, BusinessItemDto d) => bo.FetchChild(d));
            reg.HandleFetchChild((BusinessItem bo, System.Tuple<CriteriaBase, BusinessItemDto> d) => bo.FetchChild(d.Item1, d.Item2));
            reg.HandleUpdateChild((BusinessItem bo, Guid criteria, IBusinessItemDal d) => bo.UpdateChild(criteria, d));
            reg.HandleInsertChild((BusinessItem bo, Guid criteria, IBusinessItemDal d) => bo.InsertChild(criteria, d));
        }

        public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(c => c.Name);
        public string Name
        {
            get { return GetProperty(NameProperty); }
            set { SetProperty(NameProperty, value); }
        }

        public static readonly PropertyInfo<Guid> CriteriaProperty = RegisterProperty<Guid>(c => c.Criteria);
        public Guid Criteria
        {
            get { return GetProperty(CriteriaProperty); }
            set { SetProperty(CriteriaProperty, value); }
        }

        public static readonly PropertyInfo<Guid> UniqueIDProperty = RegisterProperty<Guid>(c => c.FetchChildID);
        public Guid FetchChildID
        {
            get { return GetProperty(UniqueIDProperty); }
            set { SetProperty(UniqueIDProperty, value); }
        }


        public static readonly PropertyInfo<Guid> UpdatedIDProperty = RegisterProperty<Guid>(c => c.UpdatedID);
        public Guid UpdatedID
        {
            get { return GetProperty(UpdatedIDProperty); }
            set { SetProperty(UpdatedIDProperty, value); }
        }

        public static readonly PropertyInfo<Guid> ScopeIDProperty = RegisterProperty<Guid>(c => c.ScopeID);
        public Guid ScopeID
        {
            get { return GetProperty(ScopeIDProperty); }
            set { SetProperty(ScopeIDProperty, value); }
        }

        public void FetchChild(BusinessItemDto dto) // I only need the dependency within this method
        {
            using (BypassPropertyChecks)
            {
                MarkAsChild();
                this.FetchChildID = dto.FetchUniqueID;
            }
        }


        // We allow the Fetch calls (and delegates) to have multiple parameters
        // But the IHandleXYZ interface can only have one criteria as a parameter
        // with a tuple to handle multiple parameters
        // ObjectPortal will bridge the two by turning the multiple paramters to a tuple
        public void FetchChild(CriteriaBase g, BusinessItemDto dto) // I only need the dependency within this method
        {
            using (BypassPropertyChecks)
            {
                this.FetchChildID = dto.FetchUniqueID;
                this.Criteria = g.Guid;
            }
        }

        //protected override void AddBusinessRules()
        //{
        //    base.AddBusinessRules();
        //    BusinessRules.AddRule(new BusinessRule(NameProperty));
        //    BusinessRules.AddRule(new DependencyBusinessRule(NameProperty));
        //}

        public void CreateChild(Guid criteria)
        {
            using (BypassPropertyChecks)
            {
                this.Criteria = criteria;
            }
        }

        public void InsertChild(Guid criteria, IBusinessItemDal dal)
        {

            var dto = new BusinessItemDto();
            dal.Update(dto);

            using (BypassPropertyChecks)
            {
                this.UpdatedID = dto.UpdateUniqueID;
            }

        }

        public void UpdateChild(Guid criteria, IBusinessItemDal dal)
        {
            var dto = new BusinessItemDto();
            dal.Update(dto);

            using (BypassPropertyChecks)
            {
                this.UpdatedID = dto.UpdateUniqueID;
            }
        }

        //internal class BusinessRule : BusinessRuleDIBase
        //{

        //    public BusinessRule(IPropertyInfo nameProperty) : base(nameProperty)
        //    {
        //        InputProperties.Add(nameProperty);
        //    }

        //    protected override void Execute_(RuleContext context)
        //    {
        //        base.Execute_(context);

        //        context.Complete();
        //    }

        //}

        //internal class DependencyBusinessRule : BusinessRuleDIBase
        //{

        //    public DependencyBusinessRule(IPropertyInfo nameProperty) : base(nameProperty)
        //    {
        //        InputProperties.Add(nameProperty);
        //        ExecuteMethodDI = (rc, s) => ExecuteDI(rc, s.Resolve<IBusinessItemDal>());
        //    }

        //    public void ExecuteDI(RuleContext context, IBusinessItemDal dependencies)
        //    {
        //        if (dependencies == null)
        //        {
        //            context.AddErrorResult("Did not recieve dependency!");
        //        }
        //    }

        //}


    }
}
