using Autofac;
using Csla.Core;
using Csla.Serialization.Mobile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPortal
{

    internal interface IMobileObjectWrapper
    {
        ILifetimeScope scope { get; }

        MobileDependencyList MobileDependencies { get; }
    }


    internal interface IMobileObjectWrapper<T> : IMobileObjectWrapper, IMobileObject
    {
        T BusinessObject { get; }
    }

    internal interface IMobileObjectWrapper<T, C> : IMobileObjectWrapper<T>
    { }


    [Serializable]
    internal class MobileObjectWrapper<T> : Csla.Core.MobileObject, IMobileObjectWrapper<T>
        where T : IMobileObject, ITrackStatus
    {

        public MobileObjectWrapper()
        {
            // Neccessary for deserialization
            // Scope will be set by ObjectPortal
        }

        public MobileObjectWrapper(ILifetimeScope scope)
        {
            this._scope = scope;
        }

        private T _businessObject;

        public T BusinessObject
        {
            get { return _businessObject; }
            set { _businessObject = value; }
        }

        private MobileDependencyList _mobileDependencies;

        public MobileDependencyList MobileDependencies
        {
            get { return _mobileDependencies; }
            set { _mobileDependencies = value; }
        }


        [NonSerialized]
        private ILifetimeScope _scope;

        public ILifetimeScope scope { get { return _scope; } }

        public void DataPortal_Create()
        {
            var portal = scope.Resolve<IObjectPortal<T>>();

            BusinessObject = portal.Create();

            MobileDependencies = scope.Resolve<MobileDependencyList>();

        }

        public void DataPortal_Fetch()
        {
            var portal = scope.Resolve<IObjectPortal<T>>();

            BusinessObject = portal.Fetch();

            MobileDependencies = scope.Resolve<MobileDependencyList>();

        }


        protected override void OnGetChildren(SerializationInfo info, MobileFormatter formatter)
        {
            base.OnGetChildren(info, formatter);


            var boInfo = formatter.SerializeObject(this.BusinessObject);
            info.AddChild(nameof(BusinessObject), boInfo.ReferenceId);

            var mdInfo = formatter.SerializeObject(this.MobileDependencies);
            info.AddChild(nameof(MobileDependencies), mdInfo.ReferenceId);

        }


        protected override void OnSetChildren(SerializationInfo info, MobileFormatter formatter)
        {
            base.OnSetChildren(info, formatter);

            var boData = info.Children[nameof(BusinessObject)];
            BusinessObject = (T)formatter.GetObject(boData.ReferenceId);

            var mdInfo = info.Children[nameof(MobileDependencies)];
            MobileDependencies = (MobileDependencyList)formatter.GetObject(mdInfo.ReferenceId);

        }

    }

    [Serializable]
    internal class MobileObjectWrapper<T, C> : MobileObjectWrapper<T>, IMobileObjectWrapper<T, C>
        where T : IMobileObject, ITrackStatus
    {

        public MobileObjectWrapper() : base()
        {
            // Neccessary for deserialization
            // Scope will be set by ObjectPortal
        }

        public MobileObjectWrapper(ILifetimeScope scope) : base(scope)
        {

        }

        private void DataPortal_Create(C criteria)
        {
            var portal = scope.Resolve<IObjectPortal<T>>();

            BusinessObject = portal.Create(criteria);

            MobileDependencies = scope.Resolve<MobileDependencyList>();
        }

        private void DataPortal_Fetch(C criteria)
        {
            var portal = scope.Resolve<IObjectPortal<T>>();

            BusinessObject = portal.Fetch(criteria);

            MobileDependencies = scope.Resolve<MobileDependencyList>();
        }

    }

}
