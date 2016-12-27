using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Csla.DataPortalClient;

namespace ObjectPortal
{
    public class BasicHttpBindingWcfProxy : WcfProxy
    {

        public BasicHttpBindingWcfProxy() : base()
        {

        }

        protected override ChannelFactory<IWcfPortal> GetChannelFactory()
        {
            //ChannelFactory<IWcfPortal> factory;

            //factory = new ChannelFactory<IWcfPortal>("DataPortalEndpoint");

            //return factory;


            ChannelFactory<Csla.DataPortalClient.IWcfPortal> factory = base.GetChannelFactory();

            factory.Credentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;

            WSHttpBinding basicBinding = new WSHttpBinding();
            //basicBinding.Security.Mode = SecurityMode.Transport;
            //basicBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            basicBinding.ReaderQuotas.MaxArrayLength = int.MaxValue;
            basicBinding.ReaderQuotas.MaxStringContentLength = int.MaxValue;
            basicBinding.ReaderQuotas.MaxBytesPerRead = int.MaxValue;
            basicBinding.MaxBufferPoolSize = int.MaxValue;
            basicBinding.MaxReceivedMessageSize = int.MaxValue;
            basicBinding.ReaderQuotas.MaxArrayLength = int.MaxValue;

            factory.Endpoint.Binding = basicBinding;

            return factory;
        }
    }
}
