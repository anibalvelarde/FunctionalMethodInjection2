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

    public static class HandleRegistrationsStaticExtensions
    {
        public static void Create<T>(this IHandleRegistrations<T> regs, Func<T> func)
        {

        }

        public static void CreateCriteria<T, C>(this IHandleRegistrations<T> regs, Func<C, T> func)
        {

        }

        public static void CreateDependency<T, D>(this IHandleRegistrations<T> regs, Func<D, T> func)
        {

        }

        public static void Create<T, C, D>(this IHandleRegistrations<T> regs, Func<C, D, T> func)
        {

        }
    }

    [Serializable]
    [HandleMethod(nameof(Handle))]

    internal class RootStatic : DPBusinessBase<RootStatic>, IRoot //, IHandleRegistrations
    {

        public RootStatic()
        {
        }

        internal static void Handle(IHandleRegistrations<RootStatic> regs)
        {

            // Trying to come up with a way to define only the method name 
            // with no reflection or long type definition (ie <...,...>)
            // So far NOT winning...
            // This doens't work. 'Cannot infer type' Why not?!

            //regs.CreateDependency(Create);
            //regs.Create()


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

        private static RootStatic Create(IObjectPortal<IBusinessItemList> op)
        {

            var result = new RootStatic();

            using (result.BypassPropertyChecks)
            {
                result.BusinessItemList = op.CreateChild();
            }

            return result;
        }

        private static RootStatic Create(Guid criteria, IObjectPortal<IBusinessItemList> op)
        {
            var result = new RootStatic();


            using (result.BypassPropertyChecks)
            {
                result.BusinessItemList = op.CreateChild(criteria);
            }

            return result;
        }

 
    }
}
