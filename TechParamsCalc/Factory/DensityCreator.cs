using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TechParamsCalc.DataBaseConnection;
using TechParamsCalc.DataBaseConnection.Density;
using TechParamsCalc.OPC;
using TechParamsCalc.Parameters;
using TitaniumAS.Opc.Client.Da;

namespace TechParamsCalc.Factory
{
    internal class DensityCreator : ItemsCreator
    {
        private string[] itemDescDensityForRead = new string[] { "SEL", "VAL_HMI", "VAL_CALC", "DELTA_D", "COMP_N", "PERC" };
        private string[] itemDescDensityForWrite = new string[] { "VAL_CALC" };
        public List<Density> DensityList { get; private set; }
        public event EventHandler densityListGeneratedEvent;                      //Событие - "список переменных сформирован"
        //private short atmoPressure;
        private SingleTagCreator singleTagCreator;
        private OpcDaItemValue[] densityValues;

        public DensityCreator(OpcClient opcClient, ItemsCreator itemCreator /*short atmoPressure*/) : base(opcClient)
        {
            subStringTagName = "_DENS";
            DensityList = new List<Density>();
            //01.04.2020
            //this.atmoPressure = atmoPressure;
            //this.atmoPressure = (itemCreator as SingleTagCreator).AtmoPressureFromOPC;
            singleTagCreator = itemCreator as SingleTagCreator;
        }

        //Метод для создания пустого списка переменных
        protected internal override void CreateItemList()
        {
            //Считываем из OPC-Reader строки с названиями переменных
            nodeElementCollection = opcClient.ReadDataToNodeList(subStringTagName).ToList();           

        }

        //Создаем группу для чтения из OPC-сервера
        protected internal override void CreateOPCReadGroup()
        {           
            
            //Отправляем в InitDataGroup список всех переменных Dens, т.к. еще не известно, какие из них несуществующие. OpcDaItemResult[] возвращает массив результатов по каждому тегу
            OpcDaItemResult[] readingResults;            
            List<string> listOfValidItems;

            this.InitDataGroup(itemDescDensityForRead, "Densities_Read_Group", nodeElementCollection, out dataGroupRead, out readingResults, out listOfValidItems);
            listOfValidItems.ForEach(i => DensityList.Add(new Density(i)));

            if (densityListGeneratedEvent != null)
                densityListGeneratedEvent.Invoke(this, new EventArgs());

        }

        //Создаем группу для записи в OPC-сервер  
        protected internal override void CreateOPCWriteGroup()
        {   
            //Принимаем, что список Densities сормировался на этапе формирования группы чтения и не содержит неправильных тегов
            this.InitDataGroup(itemDescDensityForWrite, "Densities_Write_Group", DensityList.Where(c => c.IsWriteble == true), out dataGroupWrite);


            //Инициализация массива объектов для будущей записи в OPC. Отбираются теги, значение "IsWriteble" == false
            this.valuesForWriting = new object[(from e in DensityList.Where(c => c.IsWriteble == true) select e).Count() * itemDescDensityForWrite.Length]; //Массив значений для записи в OPC_сервер
        }


        //Обновление Density тегов из OPC
        protected internal override void UpdateItemListFromOpc()
        {
            densityValues = dataGroupRead.Read(dataGroupRead.Items, OpcDaDataSource.Device);

            int valueCollectionIterator = 0;
            var density = default(Density);
            foreach (var item in DensityList)
            {
                try
                {
                    //Initialization of fields of density instance
                    density = DensityList.FirstOrDefault(c => c.TagName == item.TagName);
                    if (density != null)
                    {
                        density.Sel = (bool)densityValues[0 + valueCollectionIterator].Value;
                        density.ValHmi = (short)densityValues[1 + valueCollectionIterator].Value;
                        //_density.val_calc = (short)densityValues[2 + _valueCollectionIterator].Value;
                        density.DeltaD = (short)densityValues[3 + valueCollectionIterator].Value;
                        density.CompN = (short)densityValues[4 + valueCollectionIterator].Value;

                        //Инициализация массива description при первом апдейте списка density
                        if (density.PercDescription == null)
                            density.PercDescription = new string[density.CompN];

                        //Инициализация массива значений содержаний компонентов при первом апдейте списка density
                        if (density.PercArray == null)
                            density.PercArray = new double[density.CompN];

                        //Заполнение списка содержданий
                        for (int i = 0; i < density.CompN; i++)
                            density.PercArray[i] = ((short[])(densityValues[5 + valueCollectionIterator].Value))[i] * 0.01;

                        //01.04.2020 Передаем объект SingleTag чтобы прочитать из него атмосферное давление 
                        //density.AtmoPressure = atmoPressure;
                        density.AtmoPressure = singleTagCreator.AtmoPressureFromOPC;

                        valueCollectionIterator += itemDescDensityForRead.Length;
                    }
                }
                catch (Exception)
                {
                    throw new Exception($"Density Creator error handling. Tag = {item.TagName}");
                }
                finally
                {
                    //valueCollectionIterator += itemDescDensityForRead.Length;
                }

            }
        }

        //Обновление Capacity тегов из Базы Данных
        protected internal override void UpdateItemListFromDB(List<Temperature> temperatureList, List<Pressure> pressureList, DBPGContext dbContext)
        {
            // считываем описание с базы данных
            var denData = from c in dbContext.densityDescs
                          select c;
            //}
            // -------------------------------------------------------------------------------------------------------
            // обьединяем два листа по условию равности tagName и формируем объект
            var result = from itemDenData in (IEnumerable<DensityContent>)denData
                         join itemDensityList in DensityList on itemDenData.tagname equals itemDensityList.TagName
                         select new
                         {
                             tagname = itemDenData.tagname,
                             percDescription = new string[] {
                                     itemDenData.perc0 ?? string.Empty,
                                     itemDenData.perc1 ?? string.Empty,
                                     itemDenData.perc2 ?? string.Empty,
                                     itemDenData.perc3 ?? string.Empty,
                                     itemDenData.perc4 ?? string.Empty,
                                 },

                             temperature = temperatureList.FirstOrDefault(x => x.TagName == itemDenData.temperature),
                             pressure = pressureList.FirstOrDefault(x => x.TagName == itemDenData.pressure),
                             description = itemDenData.description,
                             isWriteble = itemDenData.isWritable
                         };

            // ----------------------------------------------------------------------------
            var density = default(Characteristic);
            foreach (var item in result)
            {
                density = DensityList.FirstOrDefault(c => c.TagName == item.tagname);

                try
                {
                    if (density != null)
                    {
                        Array.Copy(item.percDescription, ((Density)density).PercDescription, ((Density)density).CompN);
                        density.Description = item.description;
                        ((Density)density).Temperature = item.temperature as Temperature;
                        ((Density)density).Pressure = item.pressure as Pressure == null ? new Pressure("PressureSample") : item.pressure as Pressure;
                        ((Density)density).IsWriteble = item.isWriteble ?? false;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Alert!", MessageBoxButton.OK, MessageBoxImage.Error);
                    density.IsInValid = true;
                }
            }
        }

        //Запись тегов в OPC
        protected internal override void WriteItemToOPC()
        {
            int i = 0;
            foreach (var item in DensityList)
            {
                if (item.IsWriteble)
                    valuesForWriting[i++] = item.ValCalc;
            }

            if (opcClient.OpcServer.IsConnected)
                opcClient.WriteMultiplyItems(dataGroupWrite, valuesForWriting);
        }

        //Запись тегов в БД
        protected internal override void WriteItemToDB(DBPGContext context)
        {
            foreach (var item in DensityList)
            {
                try
                {
                    context.densityDescs.FirstOrDefault(c => c.tagname == item.TagName).value = item.ValCalc;
                }
                catch (Exception)
                {
                    throw new Exception($"Tag = {item.TagName}");
                }

            }
            context.SaveChanges();
        }
    }
}
