using Csla;
using System;
using Autofac;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectPortal;
using Example.Dal;

namespace Example.Lib
{


    [Serializable]
    [HandleMethod(nameof(Handle))]

    internal class Root : DPBusinessBase<Root>, IRoot //, IHandleRegistrations
    {

        public Root()
        {
        }

        private static void Handle(IHandleRegistrations<Root> regs)
        {
            // TODO To Discuss
            // I think it would be best for this method to be static
            // but that would require sending the instance into each lambda

            // If it stays instance should this method be an abstract on the base class
            // with the attribute defined there or no attribute?

            // Analysis
            // This is breaking the principal of DI awareness
            // The class should not be making any decision of where the dependency
            // is coming from, simiply state that it needs a dependency

            // 1. Get rid of explicit IOC reference
            // 2. What is the dicoverability of this
            //      - Fairly clean. The type C makes generic methods that work.
            // 3. Is it type specific or can change each instance?

            // + With interfaces the ObjectPortal methods cannot be private
            // + Fluent api for .RunLocal()
            // + Both Create and CreateChild can call a child method

            // Same - still use Tuples for the dependencies and criteria

            // regs.HandleCreate(nameof(Create)).RunLocal();
            // regs.HandleCreate<Guid>(nameof(Create));

            //regs.HandleCreate<Guid>((c, s) => Create(c, s.Resolve<IObjectPortal<IBusinessItemList>>()));
            //regs.HandleCreate(s => Create(s.Resolve<IObjectPortal<IBusinessItemList>>()));


            regs.HandleCreate<Root, Guid, IObjectPortal<IBusinessItemList>>((bo, c, d) => bo.Create(c, d));
            regs.HandleCreateWithDependency<Root, IObjectPortal<IBusinessItemList>>((bo, d) => bo.Create(d));


            regs.HandleFetchWithDependency((Root bo, IObjectPortal<IBusinessItemList> d) => bo.Fetch(d));
            regs.HandleFetch((Root bo, Guid criteria, IObjectPortal<IBusinessItemList> d) => bo.Fetch(criteria, d));

            regs.HandleUpdateWithDependency((Root bo, IObjectPortal<IBusinessItemList> d) => bo.Update(d));
            regs.HandleInsertWithDependency((Root bo, IObjectPortal<IBusinessItemList> d) => bo.Insert(d));

        }

        public static readonly PropertyInfo<IBusinessItemList> BusinessItemListProperty = RegisterProperty<IBusinessItemList>(c => c.BusinessItemList);
        public IBusinessItemList BusinessItemList
        {
            get { return GetProperty(BusinessItemListProperty); }
            set { SetProperty(BusinessItemListProperty, value); }
        }

        // Unfortunatly I need to have these so that they don't default to the "RunLocal"

        private void DataPortal_Create() { }
        private void DataPortal_Create(Guid criteria) { }

        private void Create(IObjectPortal<IBusinessItemList> op)
        {
            using (BypassPropertyChecks)
            {
                BusinessItemList = op.CreateChild();
            }
        }

        // [HandleCreate]
        private void Create(Guid criteria, IObjectPortal<IBusinessItemList> op)
        {
            using (BypassPropertyChecks)
            {
                BusinessItemList = op.CreateChild(criteria);
            }
        }

        private void Fetch(IObjectPortal<IBusinessItemList> op)
        {
            using (BypassPropertyChecks)
            {
                BusinessItemList = op.FetchChild();
            }
        }

        private void Fetch(Guid criteria, IObjectPortal<IBusinessItemList> op)
        {
            using (BypassPropertyChecks)
            {
                BusinessItemList = op.FetchChild(new Criteria() { Guid = criteria });
            }
        }

        public void Insert(IObjectPortal<IBusinessItemList> op)
        {
            op.UpdateChild(BusinessItemList);
        }

        public void Update(IObjectPortal<IBusinessItemList> op)
        {
            op.UpdateChild(BusinessItemList);
        }
    }
}
