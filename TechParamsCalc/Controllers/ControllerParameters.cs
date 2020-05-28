using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechParamsCalc.DataBaseConnection.ServerConnections;

namespace TechParamsCalc.Controllers
{
    class ControllerParameters
    {
        public bool isEnableWriting { get; set; }
        public string OpcServerName { get; set; }
        public string OpcServerSubstring { get; set; }
        public HostRole ControllerRole { get; set; }
        public string[] SingleTagNamesForRW { get; set; }

    }
}
