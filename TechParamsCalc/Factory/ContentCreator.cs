using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TechParamsCalc.OPC;
using TitaniumAS.Opc.Client.Da;
using TechParamsCalc.Parameters;
using TechParamsCalc.DataBaseConnection;
using TechParamsCalc.DataBaseConnection.Content;

namespace TechParamsCalc.Factory
{
    internal class ContentCreator : ItemsCreator
    {
        private string[] itemDescContentForRead = new string[] { "SEL", "VAL_HMI", "VAL_CALC", "DELTA_P", "DELTA_T", "CONF" };
        private string[] itemDescContentForWrite = new string[] { "VAL_CALC" };

        public List<Content> ContentList { get; private set; }
        public event EventHandler contentListGeneratedEvent;                      //Событие - "список переменных сформирован"
        //private short atmoPressure;
        private SingleTagCreator singleTagCreator;

        private OpcDaItemValue[] contentValues;

        public ContentCreator(OpcClient opcClient, ItemsCreator itemCreator/*short atmoPressure*/) : base(opcClient)
        {
            //@"^.*_CONT.*$"            
            subStringTagName = @"^[S]\d{2,3}[_]\w*[_]CONT$";
            ContentList = new List<Content>();
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
            //Отправляем в InitDataGroup список всех переменных Conts, т.к. еще не известно, какие из них несуществующие. OpcDaItemResult[] возвращает массив результатов по каждому тегу
            OpcDaItemResult[] readingResults;
            List<string> listOfValidItems;

            this.InitDataGroup(itemDescContentForRead, "Contents_Read_Group", nodeElementCollection, out dataGroupRead, out readingResults, out listOfValidItems);
            listOfValidItems.ForEach(i => ContentList.Add(new Content(i)));

            string contentName;
            //Формируем список пустых объектов Density
            foreach (var result in readingResults)
            {
                if (result.Error.Succeeded)
                {
                    contentName = result.Item.ItemId.Substring(result.Item.ItemId.LastIndexOf('!') + 1);
                    contentName = contentName.Substring(0, contentName.LastIndexOf('.'));
                    if (!ContentList.Any(d => d.TagName == contentName))
                        ContentList.Add(new Content(contentName));
                }
            }

            if (contentListGeneratedEvent != null)
                contentListGeneratedEvent.Invoke(this, new EventArgs());
        }


        //Создаем группу для записи в OPC-сервер   
        protected internal override void CreateOPCWriteGroup()
        {
            //Принимаем, что список Contents сформировался на этапе формирования группы чтения и не содержит неправильных тегов            
            this.InitDataGroup(itemDescContentForWrite, "Contents_Write_Group", ContentList.Where(c => c.IsWriteble == true), out dataGroupWrite);

            //Инициализация массива объектов для будущей записи в OPC. Отбираются теги, значение "IsWriteble" == false
            this.valuesForWriting = new object[(from e in ContentList.Where(c => c.IsWriteble == true) select e).Count() * itemDescContentForWrite.Length]; //Массив значений для записи в OPC_сервер
        }


        //Обновление Capacity тегов из OPC
        protected internal override void UpdateItemListFromOpc()
        {
            contentValues = dataGroupRead.Read(dataGroupRead.Items, OpcDaDataSource.Device);

            int valueCollectionIterator = 0;
            var content = default(Content);
            foreach (var item in nodeElementCollection)
            {
                try
                {
                    //Initialization of fields of capacity instance
                    content = ContentList.FirstOrDefault(c => c.TagName == item.Name);
                    if (content != null)
                    {
                        content.Sel = (short)contentValues[0 + valueCollectionIterator].Value;
                        content.ValHmi = (short)contentValues[1 + valueCollectionIterator].Value;
                        //_capacity.val_calc = (short)capacityValues[2 + _valueCollectionIterator].Value;
                        content.DeltaP = (short[])contentValues[3 + valueCollectionIterator].Value;
                        content.DeltaT = (short[])contentValues[4 + valueCollectionIterator].Value;
                        content.Conf = (short)contentValues[5 + valueCollectionIterator].Value;

                        //Инициализация массива description при первом апдейте списка capacity
                        if (content.PercDescription == null)
                            content.PercDescription = new string[5];

                        //Инициализация атмосферного давления, полученного из OPC
                        //01.04.2020 Передаем объект SingleTag чтобы прочитать из него атмосферное давление 
                        //content.AtmoPressure = atmoPressure;
                        content.AtmoPressure = singleTagCreator.AtmoPressureFromOPC;
                    }
                }
                catch (Exception)
                {
                    throw new Exception($"Content Creator error handling. Tag = {item.Name}");
                }
                finally
                {
                    valueCollectionIterator += itemDescContentForRead.Length;
                }
                
            }
        }
        
        //Обновление Content тегов из Базы Данных
        protected internal override void UpdateItemListFromDB(List<Temperature> temperatureList, List<Pressure> pressureList, DBPGContext dbContext)
        {
            // считываем описание с базы данных
            var contentData = from c in dbContext.contentDescs
                              select c;
            //}
            // -------------------------------------------------------------------------------------------------------
            // обьединяем два листа по условию равности tagName и формируем объект
            var result = from itemContData in (IEnumerable<ContentContent>)contentData
                         join itemContentList in ContentList on itemContData.tagname equals itemContentList.TagName
                         select new
                         {
                             tagname = itemContData.tagname,
                             percDescription = new string[] {
                                     itemContData.comp0 ?? string.Empty,
                                     itemContData.comp1 ?? string.Empty,
                                     itemContData.comp2 ?? string.Empty,
                                     itemContData.comp3 ?? string.Empty,
                                     itemContData.comp4 ?? string.Empty,
                                 },

                             temperature = temperatureList.FirstOrDefault(x => x.TagName == itemContData.temperature),
                             pressure = pressureList.FirstOrDefault(x => x.TagName == itemContData.pressure),
                             description = itemContData.description,
                             isWriteble = itemContData.isWritable
                         };
            

            // ----------------------------------------------------------------------------
            var content = default(Characteristic);
            foreach (var item in result)
            {
                content = ContentList.FirstOrDefault(c => c.TagName == item.tagname);

                try
                {
                    if (content != null)
                    {
                        Array.Copy(item.percDescription, ((Content)content).PercDescription, 5); //5 компонентов
                        content.Description = item.description;
                        ((Content)content).Temperature = item.temperature as Temperature;
                        ((Content)content).Pressure = item.pressure as Pressure == null ? new Pressure("PressureSample") : item.pressure as Pressure;                        
                        ((Content)content).IsWriteble = item.isWriteble ?? false;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Alert!", MessageBoxButton.OK, MessageBoxImage.Error);
                    content.IsInValid = true;
                }
            }
        }
       

        //Запись тегов в OPC
        protected internal override void WriteItemToOPC()
        {
            int i = 0;
            foreach (var item in ContentList)
            {
                if (item.IsWriteble)
                    valuesForWriting[i++] = item.ValCalc;
            }

            if (opcClient.OpcServer.IsConnected)
                opcClient.WriteMultiplyItems(dataGroupWrite, valuesForWriting);
        }




    }
}
