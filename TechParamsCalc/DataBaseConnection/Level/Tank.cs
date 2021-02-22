using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;


namespace TechParamsCalc.DataBaseConnection.Level
{
    /// <summary>
    /// Класс для сборников
    /// Описание типов сборников:
    /// 1 - вертикальный с двумя эллиптическими днищами;
    /// 2 - горизонтальный с двумя эллиптическими боковинами
    /// 3 - 
    /// </summary>
    public class Tank
    {
        [Key]
        public int id { get; set; }
        public string tankDef { get; set; }
        public int type { get; set; }
        public int dimA { get; set; }
        public int dimB { get; set; }
        public int dimC { get; set; }
        public int dimD { get; set; }
        public int dimE { get; set; }
        public int dimF { get; set; }


        private double totalLength;
        private int ltoDistanceA, distanceA, distanceB;


        //Коллекция методов для определения объема сборника каждого из типов
        private Dictionary<int, Func<int, double>> TankVolumeDictionary;

        public Tank()
        {
            TankVolumeDictionary = new Dictionary<int, Func<int, double>>();
            

            //Формирование статических свойств сборника
            //
            //...

            
            //Формирование списка методов расчета объема для каждого типа сборника
            TankVolumeDictionary.Add(1, (v) => GetTypeOneVolume(v)); //Расчет объема для типа сборника 1
            TankVolumeDictionary.Add(2, (v) => GetTypeTwoVolume(v)); //Расчет объема для типа сборника 2
            TankVolumeDictionary.Add(3, (v) => GetTypeThreeVolume(v)); //Расчет объема для типа сборника 3
            TankVolumeDictionary.Add(4, (v) => GetTypeFourVolume(v)); //Расчет объема для типа сборника 4
            TankVolumeDictionary.Add(5, (v) => GetTypeFiveVolume(v)); //Расчет объема для типа сборника 5



        }

        public void InitalizeTank(int _ltoDistanceA, int _distanceA, int _distanceB)
        {
            ltoDistanceA = _ltoDistanceA;
            distanceA = _distanceA;
            distanceB = _distanceB;            
        }


        public double GetVolume(int _levelmm)
        {
            return TankVolumeDictionary[type](_levelmm);
        }

        #region Методы для расчета объема каждого из типов сборников

        /// <summary>
        /// Получение объема для типа сборника 1 - вертикальный с двумя эллиптическими днищами
        /// </summary>
        /// <param name="LtoDistanceA"></param>
        /// <param name="DistanceB"></param>
        /// <returns></returns>
        private double GetTypeOneVolume(int levelmm)
        {                           
            //Радиус сбоника
            var radius = dimB * 0.001 / 2;

            //Общая высота сборника, включая два элиптических днища
            totalLength = dimA + dimC * 2;

            //Расстояние до дна сборника для оценки неучтенного объема
            //var levelFromSensorToBottomOfTheTank = Math.Max(0, totalLength - (ltoDistanceA + (distanceB - distanceA)));
            var levelFromSensorToBottomOfTheTank = Math.Max(0, totalLength - distanceB + ltoDistanceA);

            //Неучтенный объем жидкости под датчиком уровня
            var volumeLeft = Math.PI * radius * radius * levelFromSensorToBottomOfTheTank * 0.001 * 0.8; //0.8 - поправка на эллиптическое днище

            //Объем жидкости по датчику уровня
            var volumeLevel = Math.PI * radius * radius * Math.Max(0, levelmm) * 0.001;

            //Общий объем сборника по датчику уровня, включая неучтенный остаток
            var totalVolume = volumeLevel + Math.Max(0, volumeLeft);

            return totalVolume;
        }


        /// <summary>
        /// Получение объема для типа сборника 2 - горизонтальный с двумя эллиптическими боковинами
        /// </summary>
        /// <param name="LtoDistanceA"></param>
        /// <param name="DistanceB"></param>
        /// <param name="levelmm"></param>
        /// <returns> </returns>
        private double GetTypeTwoVolume(int levelmm)
        {
            //Радиус сбоника
            var radius = dimB * 0.001 / 2;

            //Расстояние до дна сборника для оценки неучтенного объема
            //var levelFromSensorToBottomOfTheTank = Math.Max(0, dimB - (ltoDistanceA + (distanceB - distanceA)));
            var levelFromSensorToBottomOfTheTank = Math.Max(0, dimB - distanceB + ltoDistanceA);


            double getSomeVolume(double level, double length)
            {

                var alphaRadians = 2 * Math.Acos(Math.Max(1 - level / radius, -1.0));

                var alpha = alphaRadians * 180.0 / Math.PI;

                //Площадь сектора, образующегося углом alpha
                var s = 0.5 * radius * radius * (Math.PI * alpha / 180.0 - Math.Sin(alphaRadians));

                var volume = s * length;

                return volume;
            }

            //Расчет объема цилиндрической части, включая неучтенный под датчиком уровня
            var volumeMainPart = getSomeVolume(levelFromSensorToBottomOfTheTank * 0.001 + levelmm * 0.001, dimA * 0.001);
            
            //Расчет объема эллиптических частей сб-ка. Прнимаем их объем по объему эквивалентных цилиндров с коэффициентом поправки
            var volumeOfEllipticParts = getSomeVolume(levelFromSensorToBottomOfTheTank * 0.001 + levelmm * 0.001, dimC * 2 * 0.001 * 0.681); //0.681 - поправка для эллипических частей сб-ка

            var volumeTotal = volumeMainPart + volumeOfEllipticParts;

            return volumeTotal;
        }


        /// <summary>
        /// Получение объема для типа сборника 3 - вертикальный с перегородкой
        /// </summary>
        /// <param name="LtoDistanceA"></param>
        /// <param name="DistanceA"></param>
        /// <param name="DistanceB"></param>
        /// <param name="levelmm"></param>
        /// <returns></returns>
        private double GetTypeThreeVolume(int levelmm)
        {
            //Радиус сбоника
            var radius = dimB * 0.001 / 2;

            //Общая высота сборника, включая эллиптическое днище
            var totalLength = dimA + dimC;

            //Расстояние до дна сборника для оценки неучтенного объема
            //var levelFromSensorToBottomOfTheTank = Math.Max(0, totalLength - (ltoDistanceA + (distanceB - distanceA)));
            var levelFromSensorToBottomOfTheTank = Math.Max(0, totalLength - distanceB + ltoDistanceA);

            //Метод для расчета объема усеченного по длине цилиндра
            double getSomeVolume(double level)
            { 

                var alphaRadians = 2 * Math.Acos(Math.Max((dimD * 0.001 - radius) / radius, -1.0));

                var alpha = 360.0 - alphaRadians * 180.0 / Math.PI;              

               
                //Площадь сектора, образующегося углом alpha
                var s = 0.5 * radius * radius * (Math.PI * alpha / 180.0 - Math.Sin(2 * Math.PI - alphaRadians));
              

                var volume = s * level;

                return volume;
            }            

            //Неучтенный объем жидкости под датчиком уровня            
            var volumeLeft = getSomeVolume(levelFromSensorToBottomOfTheTank * 0.001) * 0.85; //0.85 - поправка на эллиптическое днище

            //Объем жидкости по датчику уровня            
            var volumeLevel = getSomeVolume(levelmm * 0.001);

            var volumeTotal = volumeLeft + volumeLevel;
            
            return volumeTotal;
        }

        /// <summary>
        /// Получение объема для типа сборника 4 - сборник-параллелепипед
        /// </summary>
        /// <param name="LtoDistanceA"></param>
        /// <param name="DistanceB"></param>
        /// <returns></returns>
        private double GetTypeFourVolume(int levelmm)
        {
            
            //Общая высота сборника, включая два элиптических днища
            totalLength = dimA;

            //Расстояние до дна сборника для оценки неучтенного объема
            var levelFromSensorToBottomOfTheTank = Math.Max(0, totalLength - (ltoDistanceA + (distanceB - distanceA)));

            //Неучтенный объем жидкости под датчиком уровня
            var volumeLeft = dimB * 0.001 * dimC * 0.001 * levelFromSensorToBottomOfTheTank * 0.001;

            //Объем жидкости по датчику уровня
            var volumeLevel = dimB * 0.001 * dimC * 0.001 * Math.Max(0, levelmm) * 0.001;

            //Общий объем сборника по датчику уровня, включая неучтенный остаток
            var totalVolume = volumeLevel + Math.Max(0, volumeLeft);

            return totalVolume;
        }

        /// <summary>
        /// Получение объема для типа сборника 5 - куб колонны со встроенным ребойлером
        /// </summary>
        /// <param name="levelmm"></param>
        /// <returns></returns>
        private double GetTypeFiveVolume(int levelmm)
        {

            //Радиус сбоника
            var radius = dimB * 0.001 / 2.0;

            //Общая высота сборника, включая эллиптическое днище
            var totalLength = dimA + dimC;

            //Расстояние до дна сборника для оценки неучтенного объема
            //var levelFromSensorToBottomOfTheTank = Math.Max(0, totalLength - (ltoDistanceA + (distanceB - distanceA)));
            var levelFromSensorToBottomOfTheTank = Math.Max(0, totalLength - distanceB + ltoDistanceA);

            //Метод для расчета объема цилиндра
            double getSomeVolume(double level)
            {
                //Площадь круга
                var s = radius * radius * Math.PI;

                var volume = s * level;

                return volume;
            }

            double volumeTotal = 0;

            //Неучтенный объем жидкости под датчиком уровня            
            volumeTotal += getSomeVolume(levelFromSensorToBottomOfTheTank * 0.001) * 0.85; //0.85 - поправка на эллиптическое днище

            //Расстояние межжду нижней врезкой и нижним рорвандом встроенного ребойлера, мм
            var distFromDistBToLowRorvand = Math.Max(0, distanceB - dimD - dimE);            

            if (levelmm <= distFromDistBToLowRorvand)
            {
                volumeTotal += getSomeVolume(levelmm * 0.001);
                return volumeTotal;
            }

            //Объем, включающий неучтенный под уровнем и участок под рорвандом
            volumeTotal += getSomeVolume(distFromDistBToLowRorvand * 0.001);


            //Расстояние межжду нижней врезкой и верхним рорвандом встроенного ребойлера, мм
            var distFromDistBToHighRorwand = Math.Max(0, distFromDistBToLowRorvand + dimD);

            //Полезный объем встроенного ребойлера колонны (с учетом объема трубок встроенного ребойлера)
            var volumeOfReboilerWithoutTubes = Math.Max(0.0, getSomeVolume(dimD * 0.001) - dimF*0.001);       //Объем цилиндра ребойлера минус общий объем трубок ребойлера            

            
            if (levelmm > distFromDistBToLowRorvand && levelmm <= distFromDistBToHighRorwand)
            {
                volumeTotal += volumeOfReboilerWithoutTubes * (levelmm - distFromDistBToLowRorvand) / dimD;
                return volumeTotal;
            }

            //Объем, включающий неучтенный под уровнем, участок под рорвандом и сам рорванд
            volumeTotal += volumeOfReboilerWithoutTubes;

            //Расчет общего объема, включающего участок над верхним рорвандом
            var distFromHighRorwand = levelmm - distFromDistBToLowRorvand - dimD;

            var volumeOfTopOfTank = getSomeVolume(distFromHighRorwand * 0.001);

            volumeTotal += volumeOfTopOfTank;            

            return volumeTotal;
        }


        #endregion
    }
}
