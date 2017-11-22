using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Integration.ServiceFabric;
using Serilog;
using Stateless1.Serilog.ServiceFabric;

namespace Stateless1
{
    public class ContainerOperations
    {
        private static readonly Lazy<IContainer> _containerSingleton = new Lazy<IContainer>(CreateContainer);

        public static bool UseAzureWireup { get; set; }


        public static IContainer Container => _containerSingleton.Value;

        private static IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new GlobalAutofacModule());
            if (UseAzureWireup)
            {
                builder.RegisterServiceFabricSupport();
                // This needs to match the name of gthe type in PackageRoot\ServiceManifest.xml AND ApplicationPackageRoot\ApplicationManifest
                builder.RegisterStatelessService<Stateless1>("Stateless1Type");
            }
            return builder.Build();

        }
    }


    public class GlobalAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register<ILogger>(container => Program.LoggerSingleton.Value);

            builder.RegisterType<ServiceLoggerFactory>()
                .SingleInstance();

          

            builder.RegisterType<ServiceConfiguration>()
                .AsSelf()
                .SingleInstance();


            builder.Register(c => Log.Logger)
                .As<ILogger>()
                .SingleInstance();
        }
    }
}
