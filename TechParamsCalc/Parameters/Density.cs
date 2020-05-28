using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechDotNetLib;
using TechDotNetLib.Lab.Substances;


namespace TechParamsCalc.Parameters
{
    internal class Density : Characteristic
    {
        public bool Sel { get; set; }
        public short ValHmi { get; set; }
        public short ValCalc { get; set; }
        public short DeltaD { get; set; }
        public short CompN { get; set; }
        public double[] PercArray { get; set; }
        public string[] PercDescription { get; set; }
        public Temperature Temperature { get; set; }
        public Pressure Pressure { get; set; }
        public short AtmoPressure { get; set; }

     
  


        private Mix mix;
        public static int id = 0;

        public Density(string tagName) : base(tagName)
        {            
            Id = ++id;
        }

        public Density(string[] percDescription, double[] percArray, Temperature temperature) : base()
        {
            
            this.PercDescription = percDescription;
            this.PercArray = percArray;
            this.Temperature = temperature;
           
        }

        //Метод расчета плотности с подключением библиотеки TechDotNetLib
        public double CalculateDensity()
        {
            double _densityValue;
            try
            {
                mix = new Mix(PercDescription, PercArray);
                if (Temperature != null)
                    _densityValue = mix.GetDensity(Temperature.Val_R, Pressure?.Val_R + AtmoPressure * 0.0001f); // pressure?.Val_R + (delta_d * 0.01F)
                else
                    _densityValue = -1;
            }
            catch (Exception)
            {
                _densityValue = -2;                

            }
            ValCalc = (short)(_densityValue + DeltaD * 0.1);
            return _densityValue;
        }
    }
}
