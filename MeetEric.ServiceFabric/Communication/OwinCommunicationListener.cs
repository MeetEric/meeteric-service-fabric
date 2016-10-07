namespace MeetEric.Communication
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Fabric;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using MeetEric.Common;
    using MeetEric.Diagnostics;
    using Exceptions;
    using Microsoft.Owin.Hosting;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Owin;
    using Security;

    public class OwinCommunicationListener : ICommunicationListener
    {
        private readonly Action<IAppBuilder, string> _startup;
        private readonly ServiceContext _serviceContext;
        private readonly int _port;

        private IDisposable webApp;
        private string publishAddress;
        private string listeningAddress;

        public OwinCommunicationListener(ServiceContext serviceContext, int port, Action<IAppBuilder, string> startup)
        {
            Throw.IfArgumentNull(nameof(serviceContext), serviceContext);
            Throw.IfDefault(nameof(port), port);
            Throw.IfArgumentNull(nameof(startup), startup);

            _startup = startup;
            _serviceContext = serviceContext;
            _port = port;

            this.Properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Machine", Environment.MachineName }
            };

            Log = MeetEricFactory.GetService<ILoggingService>().CreateLoggingContext();
        }

        public bool ListenOnSecondary { get; set; }

        private ILoggingContext Log { get; }

        private Dictionary<string, string> Properties { get; }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            string routePrefix = null;

            if (this._serviceContext is StatefulServiceContext)
            {
                var idFactory = MeetEricFactory.GetService<IIdentityFactory>();
                routePrefix = idFactory.Create().Moniker;
                listeningAddress = $"http://+:{_port}/{routePrefix}/";
            }
            else if (this._serviceContext is StatelessServiceContext)
            {
                listeningAddress = $"http://+:{_port}/";
            }
            else
            {
                throw new InvalidOperationException();
            }

            this.publishAddress = this.listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);

            Properties["Address"] = publishAddress;

            try
            {
                Log.LogEvent("StartingListener", Properties);
                this.webApp = WebApp.Start(this.listeningAddress, appBuilder => _startup(appBuilder, routePrefix));
                return Task.FromResult(this.publishAddress);
            }
            catch (Exception ex)
            {
                Log.LogException(ex, Properties);
                this.StopWebServer();
                throw;
            }
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            Log.LogEvent("ClosingServer", Properties);
            this.StopWebServer();
            return Task.FromResult(true);
        }

        public void Abort()
        {
            Log.LogEvent("AbortingServer", Properties);
            this.StopWebServer();
        }

        private void StopWebServer()
        {
            if (this.webApp != null)
            {
                try
                {
                    this.webApp.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // no-op
                }
            }
        }
    }
}
