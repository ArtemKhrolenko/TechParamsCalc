using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechParamsCalc.Factory;
using TechParamsCalc.Parameters;
using TechDotNetLib.Lab.Substances.ContentCalculation;
namespace TechParamsCalc.Controllers
{
    //Класс для дополнительных расчетов
    internal class AdditionalCalculator
    {
        TemperatureCreator temperatureCreator;
        PressureCreator pressureCreator;
        DensityCreator densityCreator;
        CapacityCreator capacityCreator;
        ContentCreator contentCreator;
        SingleTagCreator singleTagCreator;



        internal AdditionalCalculator(ItemsCreator temperatureCreator, ItemsCreator pressureCreator, ItemsCreator densityCreator, ItemsCreator capacityCreator, ItemsCreator contentCreator, ItemsCreator singleTagCreator)
        {
            this.temperatureCreator = temperatureCreator as TemperatureCreator;
            this.pressureCreator = pressureCreator as PressureCreator;
            this.densityCreator = densityCreator as DensityCreator;
            this.capacityCreator = capacityCreator as CapacityCreator;
            this.contentCreator = contentCreator as ContentCreator;
            this.singleTagCreator = singleTagCreator as SingleTagCreator;

            //Инициализируем раччет по расходу пропилена
            isInitPropyleneSuccess = InitalizePropyleneCalculations();
        }

        #region Расчет массы пропилена в 1й реакционной смеси (22.05.2020)
        private bool isInitPropyleneSuccess;


        private Density acnWaterDensity;
        private Density tempDensity;
        private Temperature acnWaterTemperature;
        private Temperature acnWaterPropyleneTemperature;

        internal void CalculateMassOfPropylene()
        {

            double propyleneMass;

            if (!isInitPropyleneSuccess)
            {
                return;
            }

            //Проверка на наличие минимального расхода ACN и воды, а также, чтобы расход из P05 был выше, чем из D02
            if ((singleTagCreator.AcnWaterMassFlow < 408.85 * densityCreator.DensityList.FirstOrDefault(d => d.TagName == "S11_A01_FC02_DENS").ValHmi * 0.0001) ||
                singleTagCreator.averReactFlow < singleTagCreator.AcnWaterMassFlow / (densityCreator.DensityList.FirstOrDefault(d => d.TagName == "S11_A01_FC02_DENS").ValHmi * 0.0001) + 1)
            {
                propyleneMass = 0.0;
            }
            else
            {
                var waterMass = singleTagCreator.AcnWaterMassFlow * acnWaterDensity.PercArray[0] * 0.01;
                var acnMass = singleTagCreator.AcnWaterMassFlow * acnWaterDensity.PercArray[1] * 0.01;

                //Инициализурем расчитываемую плотность стартовыми значениями (без пропилена - только вода и ACN
                tempDensity.PercArray[0] = acnWaterDensity.PercArray[0];
                tempDensity.PercArray[1] = acnWaterDensity.PercArray[1];
                tempDensity.PercArray[2] = 0.0;

                var dens = tempDensity.CalculateDensity();

                var massAverReactFlowSave = singleTagCreator.averReactFlow * dens * 0.0001;
                var massAverReactFlow = 0.0;
                var diff = 0.0;
                //Подставляем для расчета плотности температуру после P05
                tempDensity.Temperature = acnWaterPropyleneTemperature;
                var i = 0;
                do
                {   //Рассчитываем массовый расход реакционной смеси и % компонентов в ней                

                    if (massAverReactFlowSave != 0)
                    {
                        tempDensity.PercArray[0] = Math.Max(0.0, Math.Min(100.0, waterMass * 100.0 / massAverReactFlowSave));
                        tempDensity.PercArray[1] = Math.Max(0.0, Math.Min(100.0, acnMass * 100.0 / massAverReactFlowSave));
                        tempDensity.PercArray[2] = Math.Max(0.0, 100.0 - tempDensity.PercArray[1] - tempDensity.PercArray[0]);
                    }


                    //Считаем плотность с новым содержанием компонентов
                    dens = tempDensity.CalculateDensity();
                    if (dens != -1)
                    {
                        massAverReactFlow = singleTagCreator.averReactFlow * dens * 0.0001;
                        propyleneMass = massAverReactFlow * tempDensity.PercArray[2] * 0.01;
                        diff = Math.Abs(massAverReactFlowSave - massAverReactFlow);
                        massAverReactFlowSave = massAverReactFlow;
                        i++;
                    }
                    else return;
                }
                while (diff > 1.0 || i < 10);

            }

            singleTagCreator.PropyleneMass = (short)(propyleneMass * 10.0);
        }

        private bool InitalizePropyleneCalculations()
        {
            var isInitSuccess = false;

            acnWaterDensity = densityCreator.DensityList.FirstOrDefault(d => d.TagName == "S11_A01_FC02_DENS");
            acnWaterTemperature = temperatureCreator.TemperatureList.FirstOrDefault(t => t.TagName == "S11_E28_TT01");
            acnWaterPropyleneTemperature = temperatureCreator.TemperatureList.FirstOrDefault(t => t.TagName == "S11_P05_TT01");

            if (acnWaterDensity == null || acnWaterTemperature == null || acnWaterPropyleneTemperature == null)
            {
                return false;
            }

            tempDensity = new Density(new string[] { "Water", "ACN", "P" }, new double[] { 5, 95, 0 }, acnWaterTemperature);
            isInitSuccess = true;

            return isInitSuccess;
        }
        #endregion

        #region Расчет дельты к заданному давлению в 1.E06 по ТТ и PT после теплообменника 1.E32
        Pressure S11_P05_PT01;
        Temperature S11_P05_TT01;
        float _pt, _tt;
        internal void CalculateDeltaPE06()
        {

            if (!InitalizeDeltaPE06Calculations())
            {
                singleTagCreator.DeltaPE06 = 5;
                return;

            }

            List<double> pressureList = new List<double> { 15.0, 16.0, 17.0, 18.0, 19.0, 20.0 }; //7
            List<CoefSet> coefListPressure = new List<CoefSet>();  //Для давлений

            coefListPressure.Add(new CoefSet { a0 = 0.92022056, a1 = -0.085198918, a2 = 0.0020543869, a3 = -0.0000084876782, a4 = 0.0, a5 = 0.0 }); //0
            coefListPressure.Add(new CoefSet { a0 = 1.0996157, a1 = -0.091265026, a2 = 0.002068102, a3 = -0.0000084679355, a4 = 0.0, a5 = 0.0 }); //1
            coefListPressure.Add(new CoefSet { a0 = 1.2858471, a1 = -0.097478466, a2 = 0.0020827365, a3 = -0.000008449405, a4 = 0.0, a5 = 0.0 }); //2
            coefListPressure.Add(new CoefSet { a0 = 1.4729132, a1 = -0.10378968, a2 = 0.002097827, a3 = -0.0000084303312, a4 = 0.0, a5 = 0.0 }); //3
            coefListPressure.Add(new CoefSet { a0 = 1.6621591, a1 = -0.11032376, a2 = 0.0021155911, a3 = -0.0000084226318, a4 = 0.0, a5 = 0.0 }); //4
            coefListPressure.Add(new CoefSet { a0 = 1.8629333, a1 = -0.11702339, a2 = 0.0021340842, a3 = -0.0000084147094, a4 = 0.0, a5 = 0.0 }); //5

            //Определяем номер формулы (по давлению - линейная интерполяция)
            var numOfRange = ContentCalc.GetNumOfFormula(pressureList, _pt, out double deviation);
            double delta;

            //Вичисляем содержание

            //Если попали в точку базового давления-
            if (1 - deviation < 0.1 || deviation == 0)
                //Считаем по конкретной формуле один раз
                delta = ContentCalc.getPolynomValue(_tt, coefListPressure[numOfRange]);

            //Если переданное давление ниже минимального в массиве -
            else if (numOfRange == 0)
            {
                //Считаем по формуле №0               
                delta = ContentCalc.getPolynomValue(_tt, coefListPressure[0]);
                //01.04.2020 - Считаем по коэффициенту наклона прямой вниз влево
                //var y1 = ContentCalc.getPolynomValue(S11_P05_TT01.Val_R, coefListPressure[0]);
                //var y2 = ContentCalc.getPolynomValue(S11_P05_TT01.Val_R, coefListPressure[1]);
                //delta = y1 - (y2 - Math.Abs(y1)) * (pressureList[0] - S11_P05_PT01.Val_R) / (pressureList[1] - pressureList[0]);
            }

            //Если переданное давление - больше максимального в массиве - 
            else if (numOfRange == pressureList.Count)
            {
                //Считаем по формуле №pressureList.Count - 1
                delta = ContentCalc.getPolynomValue(_tt, coefListPressure[pressureList.Count - 1]);

                //01.04.2020 - Считаем по коэффициенту наклона прямой вниз влево
                //var y1 = ContentCalc.getPolynomValue(S11_P05_TT01.Val_R, coefListPressure[pressureList.Count - 2]);
                //var y2 = ContentCalc.getPolynomValue(S11_P05_TT01.Val_R, coefListPressure[pressureList.Count - 1]);
                //delta = y2 + (y2 - Math.Abs(y1)) * (S11_P05_TT01.Val_R - pressureList[pressureList.Count - 1]) / (pressureList[pressureList.Count - 1] - pressureList[pressureList.Count - 2]);
            }


            else
            {
                //Считем по двум формулам
                double tmpcount_1 = ContentCalc.getPolynomValue(_tt, coefListPressure[numOfRange - 1]);
                double tmpcount_2 = ContentCalc.getPolynomValue(_tt, coefListPressure[numOfRange]);
                delta = tmpcount_1 + (tmpcount_2 - tmpcount_1) * deviation;
            }

            singleTagCreator.DeltaPE06 = Math.Max((short)0, (short)(delta * 100.0));


        }
        private bool InitalizeDeltaPE06Calculations()
        {
            S11_P05_PT01 = pressureCreator.PressureList.FirstOrDefault(p => p.TagName == "S11_P05_PT01");
            S11_P05_TT01 = temperatureCreator.TemperatureList.FirstOrDefault(t => t.TagName == "S11_P05_TT01");
            _pt = Math.Max(15.0f, Math.Min(S11_P05_PT01.Val_R, 20.0f));
            _tt = Math.Max(33.0f, Math.Min(S11_P05_TT01.Val_R, 90.0f));


            if (S11_P05_PT01 == null || S11_P05_TT01 == null)
                return false;
            else
                return true;
        }
        #endregion


        //Все дополнительные расчеты
        internal void CalculateParameters()
        {
            CalculateMassOfPropylene();
            CalculateDeltaPE06();
        }
    }
}
