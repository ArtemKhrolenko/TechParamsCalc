using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using TechParamsCalc.OPC;
using TitaniumAS.Opc.Client.Da;
using TechParamsCalc.Parameters;
using TechParamsCalc.DataBaseConnection;
using TechParamsCalc.DataBaseConnection.Capacity;



namespace TechParamsCalc.Factory
{
    internal class CapacityCreator : ItemsCreator
    {
        private string[] itemDescCapacityForRead = new string[] { "SEL", "VAL_HMI", "VAL_CALC", "DELTA_C", "COMP_N", "PERC" };
        private string[] itemDescCapacityForWrite = new string[] { "VAL_CALC" };
        public List<Capacity> CapacityList { get; private set; }
        public event EventHandler capacityListGeneratedEvent;                      //Событие - "список переменных сформирован"
        //private short atmoPressure;
        private SingleTagCreator singleTagCreator;
        OpcDaItemValue[] capacityValues;

        public CapacityCreator(OpcClient opcClient, ItemsCreator itemCreator /*short atmoPressure*/) : base(opcClient)
        {
            subStringTagName = "_CAP";
            CapacityList = new List<Capacity>();
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
            //Отправляем в InitDataGroup список всех переменных Caps, т.к. еще не известно, какие из них несуществующие. OpcDaItemResult[] возвращает массив результатов по каждому тегу
            OpcDaItemResult[] readingResults;
            List<string> listOfValidItems;

            this.InitDataGroup(itemDescCapacityForRead, "Capacities_Read_Group", nodeElementCollection, out dataGroupRead, out readingResults, out listOfValidItems);
            listOfValidItems.ForEach(i => CapacityList.Add(new Capacity(i)));

            if (capacityListGeneratedEvent != null)
                capacityListGeneratedEvent.Invoke(this, new EventArgs());
        }


        //Создаем группу для записи в OPC-сервер  
        protected internal override void CreateOPCWriteGroup()
        {            
            //Принимаем, что список Capacities сформировался на этапе формирования группы чтения и не содержит неправильных тегов  
            this.InitDataGroup(itemDescCapacityForWrite, "Capacities_Write_Group", CapacityList.Where(c => c.IsWriteble == true), out dataGroupWrite);

            //Инициализация массива объектов для будущей записи в OPC. Отбираются теги, значение "IsWriteble" == false
            this.valuesForWriting = new object[(from e in CapacityList.Where(c => c.IsWriteble == true) select e).Count() * itemDescCapacityForWrite.Length]; //Массив значений для записи в OPC_сервер
            
        }

        //Обновление Capacity тегов из OPC
        protected internal override void UpdateItemListFromOpc()
        {
            capacityValues = dataGroupRead.Read(dataGroupRead.Items, OpcDaDataSource.Device);

            int valueCollectionIterator = 0;
            var capacity = default(Capacity);
            foreach (var item in CapacityList)
            {
                try
                {
                    //Initialization of fields of capacity instance
                    capacity = CapacityList.FirstOrDefault(c => c.TagName == item.TagName);
                    if (capacity != null)
                    {
                        capacity.Sel = (bool)capacityValues[0 + valueCollectionIterator].Value;
                        capacity.ValHmi = (short)capacityValues[1 + valueCollectionIterator].Value;
                        //_capacity.val_calc = (short)capacityValues[2 + _valueCollectionIterator].Value;
                        capacity.DeltaC = (short)capacityValues[3 + valueCollectionIterator].Value;
                        capacity.CompN = (short)capacityValues[4 + valueCollectionIterator].Value;

                        //Инициализация массива description при первом апдейте списка capacity
                        if (capacity.PercDescription == null)
                            capacity.PercDescription = new string[capacity.CompN];

                        //Инициализация массива значений содержаний компонентов при первом апдейте списка capacity
                        if (capacity.PercArray == null)
                            capacity.PercArray = new double[capacity.CompN];

                        //Заполнение списка содержданий
                        for (int i = 0; i < capacity.CompN; i++)
                            capacity.PercArray[i] = (double)((short[])(capacityValues[5 + valueCollectionIterator].Value))[i] * 0.01;

                        //Инициализация атмосферного давления, полученного из OPC

                        //01.04.2020 Передаем объект SingleTag чтобы прочитать из него атмосферное давление 
                        //capacity.AtmoPressure = atmoPressure;
                        capacity.AtmoPressure = singleTagCreator.AtmoPressureFromOPC;
                    }
                }
                catch (Exception)
                {
                    throw new Exception($"Capacity Creator error handling. Tag = {item.TagName}");
                }
                finally
                {
                    valueCollectionIterator += itemDescCapacityForRead.Length;
                }
                
            }
        }


        //Обновление Capacity тегов из Базы Данных
        protected internal override void UpdateItemListFromDB(List<Temperature> temperatureList, List<Pressure> pressureList, DBPGContext dbContext)
        {
            // считываем описание с базы данных
            var capData = from c in dbContext.capacityDescs
                          select c;
            //}
            // -------------------------------------------------------------------------------------------------------
            // обьединяем два листа по условию равности tagName и формируем объект
            var result = from itemCapData in (IEnumerable<CapacityContent>)capData
                         join itemCapacityList in CapacityList on itemCapData.tagname equals itemCapacityList.TagName
                         select new
                         {
                             tagname = itemCapData.tagname,
                             percDescription = new string[] {
                                     itemCapData.perc0 ?? string.Empty,
                                     itemCapData.perc1 ?? string.Empty,
                                     itemCapData.perc2 ?? string.Empty,
                                     itemCapData.perc3 ?? string.Empty,
                                     itemCapData.perc4 ?? string.Empty,
                                 },

                             temperature = temperatureList.FirstOrDefault(x => x.TagName == itemCapData.temperature),
                             pressure = pressureList.FirstOrDefault(x => x.TagName == itemCapData.pressure),
                             description = itemCapData.description,
                             isWriteble = itemCapData.isWritable
                         };

            // ----------------------------------------------------------------------------
            var capacity = default(Characteristic);
            foreach (var item in result)
            {
                capacity = CapacityList.FirstOrDefault(c => c.TagName == item.tagname);

                try
                {
                    if (capacity != null)
                    {
                        Array.Copy(item.percDescription, ((Capacity)capacity).PercDescription, ((Capacity)capacity).CompN);
                        capacity.Description = item.description;
                        ((Capacity)capacity).Temperature = item.temperature as Temperature;
                        ((Capacity)capacity).Pressure = item.pressure as Pressure == null ? new Pressure("PressureSample") : item.pressure as Pressure;
                        ((Capacity)capacity).IsWriteble = item.isWriteble ?? false;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Alert!", MessageBoxButton.OK, MessageBoxImage.Error);
                    capacity.IsInValid = true;
                }
            }
        }
        

        //Запись тегов в OPC
        protected internal override void WriteItemToOPC()
        {
            int i = 0;
            foreach (var item in CapacityList)
            {
                if (item.IsWriteble)
                    valuesForWriting[i++] = item.ValCalc;
                //i++;
            }

            if (opcClient.OpcServer.IsConnected)
                opcClient.WriteMultiplyItems(dataGroupWrite, valuesForWriting);
        }

        //Запись тегов в БД
        protected internal override void WriteItemToDB(DBPGContext context)
        {
            foreach (var item in CapacityList)
            {
                try
                {
                    context.capacityDescs.FirstOrDefault(c => c.tagname == item.TagName).value = item.ValCalc;
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
