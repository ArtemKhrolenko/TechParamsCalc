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

                    //Содержание ACN-Water по колонне 1.T04 (доазеотропная концентрация. Configuration code = 1);
                    if (TagName == "S11_T04_QC01_CONT" || TagName == "S11_T04_QC02_CONT" || TagName == "S11_T04_QC03_CONT" || TagName == "S11_T04_QC04_CONT" || TagName == "S11_T04_QC05_CONT")
                        _contentValue = mix.GetContent((float)(Temperature.Val_R + DeltaT[0] * 0.1), (float)(Pressure?.Val_R + DeltaP[0] * 0.01 + AtmoPressure * 0.0001f), 1);

                    //Содержание ACN-Water по колонне 1.T05 (доазеотропная концентрация. Configuration code = 2);
                    else if (TagName == "S11_T05_QC01_CONT" || TagName == "S11_T05_QC02_CONT" || TagName == "S11_T05_QC03_CONT" || TagName == "S11_T05_QC04_CONT")
                        _contentValue = mix.GetContent((float)(Temperature.Val_R + DeltaT[0] * 0.1), (float)(Pressure?.Val_R + DeltaP[0] * 0.01 + AtmoPressure * 0.0001f), 2);

                    //Для этих тегов не обрезаем расчитанное значение content (0-100%). Configuration code = 3
                    else if (TagName == "S11_T01_QC05_CONT" || TagName == "S11_T01_QC04_CONT")
                        _contentValue = mix.GetContent((float)(Temperature.Val_R + DeltaT[0] * 0.1), (float)(Pressure?.Val_R + DeltaP[0] * 0.01 + AtmoPressure * 0.0001f), 3);

                    else
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
