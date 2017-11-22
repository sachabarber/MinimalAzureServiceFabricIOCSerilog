using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.ServiceFabric.Services.Runtime;
using Serilog;
using Serilog.Events;
using Stateless1.Extensions;

namespace Stateless1
{
    internal static class Program
    {

        private static ILogger _log = null;
        internal static readonly Lazy<ILogger> LoggerSingleton = new Lazy<ILogger>(ConfigureLogging);

        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {

            _log = LoggerSingleton.Value;

            try
            {

                TaskScheduler.UnobservedTaskException += TaskSchedulerUnobservedTaskException;

                ContainerOperations.UseAzureWireup = true;

                using (var _container = ContainerOperations.Container)
                {
                    LogAutofacRegistrations(_container);

                    _log.Information("Starting {AppName} v{AppVersion} application", AppName, AppVersion);


                    // The ServiceManifest.XML file defines one or more service type names.
                    // Registering a service maps a service type name to a .NET type.
                    // When Service Fabric creates an instance of this service type,
                    // an instance of the class is created in this host process.


                    ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(Stateless1).Name);

                    // Prevents this host process from terminating so services keep running.
                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(ex.ToString());
                _log.Fatal(ex, $"Error starting {typeof(Stateless1).Name} Service");
                throw;
            }


            //try
            //{
            //    // The ServiceManifest.XML file defines one or more service type names.
            //    // Registering a service maps a service type name to a .NET type.
            //    // When Service Fabric creates an instance of this service type,
            //    // an instance of the class is created in this host process.

            //    ServiceRuntime.RegisterServiceAsync("Stateless1Type",
            //        context => new Stateless1(context)).GetAwaiter().GetResult();

            //    ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(Stateless1).Name);

            //    // Prevents this host process from terminating so services keep running.
            //    Thread.Sleep(Timeout.Infinite);
            //}
            //catch (Exception e)
            //{
            //    ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
            //    throw;
            //}
        }


        private static void LogAutofacRegistrations(IContainer container)
        {
            foreach (var registration in container.ComponentRegistry.Registrations)
            {
                foreach (var service in registration.Services)
                {
                    _log.Debug("Autofac: {dependency} resolves to {implementation}, {lifetime}, {sharing}, {ownership}",
                        service, registration.Activator.LimitType, registration.Lifetime, registration.Sharing, registration.Ownership);
                }
            }
        }


        private static void TaskSchedulerUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            ProcessUnhandledException(e.Exception);
            e.SetObserved();
        }

        private static void ProcessUnhandledException(Exception ex)
        {
            _log.Error(ex, "Unhandled Error");
        }

        private static ILogger ConfigureLogging()
        {
            AppDomain.CurrentDomain.ProcessExit += (sender, args) => Log.CloseAndFlush();

            var configPackage = FabricRuntime.GetActivationContext().GetConfigurationPackageObject("Config");
            var environmentName = GetSetting(configPackage, "appSettings", "simpleStatelessServiceFabricDemo:EnvironmentName");
            var seqRestrictedToMinimumLevel = GetSetting(configPackage, "appSettings", "simpleStatelessServiceFabricDemo:serilog:write-to:Seq.restrictedToMinimumLevel");
            var seqServerUrl = GetSetting(configPackage, "appSettings", "simpleStatelessServiceFabricDemo:serilog:write-to:Seq.serverUrl");
            var seqLogEventLevel = LogEventLevel.Verbose;
            Enum.TryParse(seqRestrictedToMinimumLevel, true, out seqLogEventLevel);

            var loggerConfiguration = new LoggerConfiguration()
                .WriteTo.Seq(seqServerUrl, restrictedToMinimumLevel: seqLogEventLevel)
                .Enrich.WithProperty("AppName", Program.AppName)
                .Enrich.WithProperty("AppVersion", Program.AppVersion)
                .Enrich.WithProperty("EnvName", environmentName);


            if (Environment.UserInteractive)
            {
                loggerConfiguration.WriteTo.ColoredConsole();
            }

            return Log.Logger = loggerConfiguration.CreateLogger();
        }


        private static string GetSetting(ConfigurationPackage configurationPackage, string section, string parameterName)
        {
            var param = configurationPackage.Settings.Sections[section].Parameters[parameterName];
            return param.IsEncrypted ? param.DecryptValue().ConvertToUnsecureString() : param.Value;
        }

        public static readonly string AppName = typeof(Program).Assembly.GetName().Name;
        public static readonly string AppVersion = typeof(Program).Assembly.GetInformationalVersion();
    }
}
