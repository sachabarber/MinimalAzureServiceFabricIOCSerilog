using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;
using Serilog;

namespace Stateless1.Serilog.ServiceFabric
{
    public class ServiceLoggerFactory
    {
        private ILogger _logger;
        private int _hasBeenEnriched;

        public ServiceLoggerFactory(ILogger logger)
        {
            _logger = logger;
        }

        public ILogger EnrichLoggerForStatelessServiceContext(StatelessService service)
        {
            if (Interlocked.Exchange(ref _hasBeenEnriched, 1) == 0)
            {
                _logger = _logger?.ForContext(new[] {new ServiceEnricher<StatelessServiceContext>(service.Context)});
            }
            return _logger;
        }

        public ILogger GetEnrichedLogger()
        {
            return _logger;
        }
    }
}
