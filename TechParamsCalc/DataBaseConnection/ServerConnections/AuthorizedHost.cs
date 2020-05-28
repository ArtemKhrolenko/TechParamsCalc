using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace TechParamsCalc.DataBaseConnection.ServerConnections
{
    public class AuthorizedHost
    {
        //Class with content of IP address of autorized hosts to write data into OPC server
        [Key]
        public int id { get; set; }        
        public string hostAddress { get; set; }        
    }
}
