using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechDotNetLib;
using TechDotNetLib.Lab.Substances;


namespace TechParamsCalc.Parameters
{
    internal class Capacity : Characteristic
    {        
        public bool Sel { get; set; }
        public short ValHmi{ get; set; }
        public short ValCalc { get; set; }
        public short DeltaC { get; set; }
        public short CompN { get; set; }
        public double[] PercArray { get; set; }
        public string[] PercDescription { get; set; }
        public Temperature Temperature { get; set; }
        public Pressure Pressure { get; set; }
        public short AtmoPressure { get; set; }


        private Mix mix;
        public static int id = 0;

        public Capacity(string tagName) : base(tagName)
        {            
            Id = ++id;
        }

        //Метод расчета теплоемкости с подключением библиотеки TechDotNetLib
        public double CalculateCapacity()
        {
            double _capacityValue;
            try
            {
                mix = new Mix(PercDescription, PercArray);
                if (Temperature != null)
                    _capacityValue = mix.GetCapacity(Temperature.Val_R, Pressure?.Val_R + AtmoPressure * 0.0001f);
                else
                    _capacityValue = -1;
            }
            catch (Exception)
            {
                _capacityValue = -1;

            }
            ValCalc = (short)(_capacityValue + DeltaC * 0.1);

            return _capacityValue;
        }
    }
}
