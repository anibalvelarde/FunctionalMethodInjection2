using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Csla.DataPortalClient;
using Csla.WcfPortal;


namespace ObjectPortal
{
    public class BasicHttpBindingWcfProxy : MobileProxy
    {

        public BasicHttpBindingWcfProxy() : base()
        {

        }

        protected override CriteriaRequest ConvertRequest(CriteriaRequest request)
        {
            return base.ConvertRequest(request);
        }
        protected override WcfResponse ConvertResponse(WcfResponse response)
        {
            return base.ConvertResponse(response);
        }

        protected override WcfPortalClient GetProxy()
        {
            var address = new EndpointAddress(this.DataPortalUrl);
            var client = new WcfPortalClient(this.Binding, address);
            //client.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;
            return client;
        }

        public new System.ServiceModel.Channels.Binding Binding
        {
            get
            {
                //ChannelFactory<IWcfPortal> factory;

                //factory = new ChannelFactory<IWcfPortal>("DataPortalEndpoint");

                //return factory;



                BasicHttpBinding basicBinding = new BasicHttpBinding();
                //basicBinding.Security.Mode = SecurityMode.Transport;
                //basicBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                basicBinding.ReaderQuotas.MaxArrayLength = int.MaxValue;
                basicBinding.ReaderQuotas.MaxStringContentLength = int.MaxValue;
                basicBinding.ReaderQuotas.MaxBytesPerRead = int.MaxValue;
                basicBinding.MaxBufferPoolSize = int.MaxValue;
                basicBinding.MaxReceivedMessageSize = int.MaxValue;
                basicBinding.ReaderQuotas.MaxArrayLength = int.MaxValue;
                return basicBinding;
            }
        }
    }
}