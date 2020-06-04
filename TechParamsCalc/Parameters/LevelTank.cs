using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechParamsCalc.DataBaseConnection.Level;

namespace TechParamsCalc.Parameters
{
    internal class LevelTank : Characteristic
    {
        public Tank Tank { get; set; }
        public double Volume {get;set;}
        public Level Level { get; set; }
        public Density Density { get; set; }

        public double CalculateTankVolume()
        {
            Volume = Tank.GetVolume();
            return Volume;
        }
    }
}
