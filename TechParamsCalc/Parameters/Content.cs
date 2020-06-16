using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TechDotNetLib;
using TechDotNetLib.Lab.Substances;

namespace TechParamsCalc.Parameters
{
    internal class Content : Characteristic
    {        
        public short Sel { get; set; }
        public short ValHmi { get; set; }        
        public short[] ValCalc { get; set; }
        public short[] DeltaT { get; set; }
        public short[] DeltaP { get; set; }
        public string[] PercDescription { get; set; }       
        
        public Temperature Temperature { get; set; }
        public Pressure Pressure { get; set; }
        public short AtmoPressure { get; set; }

        private Mix mix;
        public static int id = 0;

        public Content(string tagName) : base(tagName)
        {                     
            Id = ++id;
        }

        //Метод расчета крепости с подключением библиотеки TechDotNetLib
        public double[] CalculateContent()
        {
            double[] _contentValue = new double[5];            

            try
            {
                mix = new Mix(PercDescription);
                try
                {

                    //Содержанеия по колонне 1.Т01 считаются по паре ACN-PO, формулы выданы Алексеем Зинченко 12.06.2020. Воду принимаем равную 0!
                    //if (TagName == "S11_T01_QC06_CONT" || TagName == "S11_T01_QC05_CONT")
                    //    _contentValue = mix.GetContent((float)(Temperature.Val_R + DeltaT[0] * 0.1), (float)(Pressure?.Val_R + DeltaP[0] * 0.01 + AtmoPressure * 0.0001f), 3); //Содержание воды в смеси 0%

                    //else if (TagName == "S11_T01_QC04_CONT" || TagName == "S11_T01_QC03_CONT")
                    //    _contentValue = mix.GetContent((float)(Temperature.Val_R + DeltaT[0] * 0.1), (float)(Pressure?.Val_R + DeltaP[0] * 0.01 + AtmoPressure * 0.0001f), 2); //Содержание воды в смеси 16%

                    //else if (TagName == "S11_T01_QC02_CONT" || TagName == "S11_T01_QC01_CONT")
                    //    _contentValue = mix.GetContent((float)(Temperature.Val_R + DeltaT[0] * 0.1), (float)(Pressure?.Val_R + DeltaP[0] * 0.01 + AtmoPressure * 0.0001f), 1); ////Содержание воды в смеси 18%

                    //else
                        _contentValue = mix.GetContent((float)(Temperature.Val_R + DeltaT[0] * 0.1), (float)(Pressure?.Val_R + DeltaP[0] * 0.01 + AtmoPressure * 0.0001f), 0); //Pressure + deltaP 1bar (abs/relative)
                }
                catch (Exception)
                {
                    _contentValue = new double[] { -1.0, -1.0, -1.0, -1.0, -1.0 };
                }

            }
            catch (Exception)
            {
                MessageBox.Show("Content calculation error!", "Alert!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            ValCalc = Array.ConvertAll(_contentValue, el => (short)el);
            return _contentValue;

        }
    }
}
