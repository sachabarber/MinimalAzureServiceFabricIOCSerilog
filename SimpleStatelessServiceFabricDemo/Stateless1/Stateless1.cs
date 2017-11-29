using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Serilog;
using Stateless1.Extensions;
using Stateless1.Serilog.ServiceFabric;

namespace Stateless1
{
    public class Stateless1 : StatelessService, IDisposable
    {
        private readonly ServiceConfiguration _configuration;
        private ILogger _log;

        public Stateless1(
            StatelessServiceContext context,
            ServiceLoggerFactory serviceLoggerFactory,
            ServiceConfiguration configuration
        ) : base(context)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }


            _configuration = configuration;
            _log = serviceLoggerFactory.EnrichLoggerForStatelessServiceContext(this);

            EnhanceConfigurationForServiceFabric(context, configuration);

            _log.Information("Stateless1 constructed ok");
        }

        private void EnhanceConfigurationForServiceFabric(StatelessServiceContext context, ServiceConfiguration configuration)
        {
            try
            {
                var configPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
                var appSettings = configPackage.Settings.Sections["appSettings"];
                var useServiceFabricEnhancements = bool.Parse(appSettings.Parameters["simpleStatelessServiceFabricDemo:UseServiceFabricEnhancements"].Value);
                var environmentName = appSettings.Parameters["simpleStatelessServiceFabricDemo:EnvironmentName"].Value;

                if (useServiceFabricEnhancements)
                {
                    UpdateEncryptedValue(configPackage, "simpleStatelessServiceFabricDemo:SomeSafeKey", (someSafeKey) => configuration.SomeSafeKey = someSafeKey);
                }
                else
                {
                    _log.Information("Stateless1 configuration set-up using default Environment='{EnvironmentName}", environmentName);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to contruct Stateless1");
                throw;
            }
        }


        private void UpdateEncryptedValue(ConfigurationPackage configurationPackage, string connectString, Action<String> updateServiceConfigurationAction)
        {
            var param = configurationPackage.Settings.Sections["encryptedSettings"].Parameters[connectString];
            updateServiceConfigurationAction(param.IsEncrypted
                ? param.DecryptValue().ConvertToUnsecureString()
                : param.Value);
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        protected override Task OnCloseAsync(CancellationToken cancellationToken)
        {
            _log.Information($"Stateless1 {nameof(OnCloseAsync)} called");
            return base.OnCloseAsync(cancellationToken);
        }

        protected override void OnAbort()
        {
            _log.Information($"Stateless1 {nameof(OnAbort)} called");
            base.OnAbort();
        }


        public void Dispose()
        {
            _log.Information($"Stateless1 {nameof(Dispose)} called");
        }

        protected override Task OnOpenAsync(CancellationToken cancellationToken)
        {
            _log.Information($"Stateless1 {nameof(OnOpenAsync)} called");
            return base.OnOpenAsync(cancellationToken);
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            long iterations = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var iter = ++iterations;
                ServiceEventSource.Current.ServiceMessage(this.Context, $"Working-{iter}");
                _log.Warning($"Working-{iter}");
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
