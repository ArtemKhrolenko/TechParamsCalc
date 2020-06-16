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


        private double radius, totalLength;
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
            var levelFromSensorToBottomOfTheTank = Math.Max(0, totalLength - (ltoDistanceA + (distanceB - distanceA)));

            //Неучтенный объем жидкости под датчиком уровня
            var volumeLeft = Math.PI * radius * radius * levelFromSensorToBottomOfTheTank * 0.001 * 0.85; //0.85 - поправка на эллиптическое днище

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
            var levelFromSensorToBottomOfTheTank = Math.Max(0, dimB - (ltoDistanceA + (distanceB - distanceA)));

            double getSomeVolume(double level, double length)
            {

                var alphaRadians = 2 * Math.Acos(1 - level / radius );

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
            var levelFromSensorToBottomOfTheTank = Math.Max(0, totalLength - (ltoDistanceA + (distanceB - distanceA)));

            //Метод для расчета объема усеченного по длине цилиндра
            double getSomeVolume(double level)
            { 

                var alphaRadians = 2 * Math.Acos((dimD * 0.001 - radius) / radius);

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
        #endregion      
    }
}
