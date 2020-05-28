using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechParamsCalc.DataBaseConnection.ServerConnections
{
    internal enum HostRole
    {
        PRIMARY_SERVER,
        SECONDARY_SERVER,
        CLIENT,
        UNKNOWN,
        ERROR
        
    }
}
