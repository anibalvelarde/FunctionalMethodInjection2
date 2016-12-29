using Autofac;
using Csla.Server;
using Csla.Server.Hosts;
using Csla.Server.Hosts.WcfChannel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPortal
{

    /// <summary>
    /// Keeps the Autofac scope open per call
    /// </summary>

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class AutofacWcfPortal : IWcfPortal
    {

        private ILifetimeScope scope;
        //private IDataPortalActivator _dataPortalActivator;

        public AutofacWcfPortal()
        {
            //this._scope = scope;
            //// Since IDataPortalActivatorServer is internal it can't be on a public constructor
            //// AutofacWcfPortal can't be internal because it needs to be in the Web.Config of the IIS WCF Application
            //this._dataPortalActivator = scope.Resolve<Func<ILifetimeScope, IDataPortalActivator>>()(scope);

            //this.scope = scope;
                
        }

        private WcfPortal portal = new WcfPortal();

        /// <summary>
        /// Create a new business object.
        /// </summary>
        /// <param name="request">The request parameter object.</param>
        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public async Task<WcfResponse> Create(CreateRequest request)
        {

            // TODO DIscuss
            // At somepoint need to go from object to critieria type
            // Pretty much with this we allow Autofac to do it

            var portal = (IServerObjectPortal) scope.Resolve(typeof(CslaServerObjectPortal<,>)
                .MakeGenericType(new Type[] { request.ObjectType, request.Criteria.GetType() }));

            object result;

            try
            {
                // TODO : Discuss
                // Can't figure out any way to not have this be reflection
                // At some point you need to go from object to actual type for
                // the criteria object

                result = await portal.Create(request.Criteria, request.Context, true);


            }
            catch (Exception ex)
            {
                result = ex;
            }

            return new WcfResponse(result);
        }

        /// <summary>
        /// Get an existing business object.
        /// </summary>
        /// <param name="request">The request parameter object.</param>
        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public async Task<WcfResponse> Fetch(FetchRequest request)
        {

            var portal = (IServerObjectPortal)scope.Resolve(typeof(CslaServerObjectPortal<,>)
                .MakeGenericType(new Type[] { request.ObjectType, request.Criteria.GetType() }));

            object result;

            try
            {
                result = await portal.Fetch(request.Criteria, request.Context, true);
            }
            catch (Exception ex)
            {
                result = ex;
            }

            return new WcfResponse(result);

        }

        /// <summary>
        /// Update a business object.
        /// </summary>
        /// <param name="request">The request parameter object.</param>
        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public async Task<WcfResponse> Update(UpdateRequest request)
        {
            Csla.Server.DataPortal portal = new Csla.Server.DataPortal();

            object result;

            try
            {
                result = await portal.Update(request.Object, request.Context, true);
            }
            catch (Exception ex)
            {
                result = ex;
            }

            return new WcfResponse(result);

        }

        /// <summary>
        /// Delete a business object.
        /// </summary>
        /// <param name="request">The request parameter object.</param>
        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public async Task<WcfResponse> Delete(DeleteRequest request)
        {
            Csla.Server.DataPortal portal = new Csla.Server.DataPortal();

            object result;

            try
            {
                result = await portal.Delete(request.ObjectType, request.Criteria, request.Context, true);
            }
            catch (Exception ex)
            {
                result = ex;
            }

            return new WcfResponse(result);

        }
    }
}
