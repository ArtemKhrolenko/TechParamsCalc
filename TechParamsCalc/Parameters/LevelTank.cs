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
        public int DistanceA { get; set; }
        public int DistanceB { get; set; }
        public int ProbeLength { get; set; }

        //DistToDistanceA не может быть меньше DistanceA        
        public int DistToDistanceA { get; set; }       

        //Расчет массы
        public double Mass
        {
            get
            {
                if (Density != null && Density.ValHmi > 0)                
                    return Volume * Density.ValHmi * 0.0001;
                else return 0;
            }
        }      
        
        public int LevelMm { get; private set; }       

        public double CalculateTankVolume()
        {           

            //Уровень в милиметрах
            LevelMm = Math.Max(0, (int)((DistanceB - DistanceA) * Level.Val_R * 10 / 1000));

            //Объем            
            Volume = Tank.GetVolume(LevelMm);
            return Volume;
        }
    }
}
