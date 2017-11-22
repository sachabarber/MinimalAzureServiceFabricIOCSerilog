using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stateless1
{
    public class ServiceConfiguration
    {
        public ServiceConfiguration()
        {
            SomeDummyValue = ConfigurationManager.AppSettings["SomeDummyValue"];
            SomeSafeKey = ConfigurationManager.AppSettings["SomeSafeKey"];
        }


        public string SomeDummyValue { get; set; }
        public string SomeSafeKey { get; set; }
    }
}
