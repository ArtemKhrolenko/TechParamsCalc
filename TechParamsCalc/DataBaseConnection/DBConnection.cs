using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Core.EntityClient;
namespace TechParamsCalc.DataBaseConnection
{
    internal static class DBConnection
    {
        public static string GetConnectionString(string host, string port, string dataBase, string userId, string password)
        {
            //name = "Default" connectionString = "host=192.168.1.131;port=5432;database=TechCalc;user id=Al;password=kip12" providerName = "Npgsql";            

            return $"host={host};port={port};database={dataBase};user id={userId};password={password}";
            
        }

        
    }
}
