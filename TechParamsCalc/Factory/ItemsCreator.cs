using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechParamsCalc.OPC;
using TitaniumAS.Opc.Client.Da;
using TitaniumAS.Opc.Client.Da.Browsing;
using TechParamsCalc.Parameters;
using TechParamsCalc.DataBaseConnection;
using System.Collections.ObjectModel;

namespace TechParamsCalc.Factory
{
    internal abstract class ItemsCreator
    {
        protected OpcClient opcClient;
        internal List<OpcDaBrowseElement> nodeElementCollection;          //Коллекция названий тегов, читаемых из OPC-сервера          
        //protected string[] itemDesc;                                           //Массив названий переменных в структуре, которая читается из OPC
        protected string subStringTagName;                                       //Признак, по которому вычитываются данные из OCP ("_CAP", "_DENS", etc.)      
        internal OpcDaGroup dataGroupRead;                                       //Группа переменных для чтения из OPC сервера
        internal OpcDaGroup dataGroupWrite;                                      //Группа переменных для записи в OPC-сервер
        internal object[] valuesForWriting;                                      //Массив объектов для записи в OPC 


        protected ItemsCreator(OpcClient opcClient)
        {
            this.opcClient = opcClient;
            nodeElementCollection = new List<OpcDaBrowseElement>();
        }

        //Создание группы для OPC - без формирования списка пеерменных Characteristic (без фильтрации на несуществующие теги. Передается список Characteristic)
        protected virtual void InitDataGroup(string[] itemDescription, string groupName, IEnumerable<Characteristic> characteristicList, out OpcDaGroup dataGroup)
        {

            List<OpcDaItemDefinition> itemDefinitions = new List<OpcDaItemDefinition>(); 

            foreach (var item in characteristicList)
            {
                for (int i = 0; i < itemDescription.Length; i++)
                {
                    itemDefinitions.Add(new OpcDaItemDefinition
                    {
                        ItemId = opcClient.ParentNodeDescriptor + item.TagName + "." + itemDescription[i],
                        IsActive = true

                    });
                }
            }

            dataGroup = opcClient.OpcServer.AddGroup(groupName);                
            dataGroup.IsActive = true;
            OpcDaItemResult[] results = dataGroup.AddItems(itemDefinitions);    
            opcClient.OpcGroupsCount++;
        }

        //Создание группы для OPC - с формированием списка пеерменных Characteristic (с фильтрацией на неправильные теги. Передается список opBrowseElements)
        protected virtual void InitDataGroup(string[] itemDescription, string groupName, IEnumerable<OpcDaBrowseElement> opBrowseElements, out OpcDaGroup dataGroup, out OpcDaItemResult[] results, out List<string> listOfValidItems)
        {

            List<OpcDaItemDefinition> itemDefinitions = new List<OpcDaItemDefinition>();  

            if (opBrowseElements != null)
            {
                foreach (var item in opBrowseElements)
                {
                    for (int i = 0; i < itemDescription.Length; i++)
                    {
                        itemDefinitions.Add(new OpcDaItemDefinition
                        {
                            ItemId = opcClient.ParentNodeDescriptor + item.Name + "." + itemDescription[i],
                            IsActive = true

                        });
                    }
                }
            }                    
            

            dataGroup = opcClient.OpcServer.AddGroup(groupName);                 
            dataGroup.IsActive = true;
            results = dataGroup.AddItems(itemDefinitions);                     
            opcClient.OpcGroupsCount++;

            //Создаем список переменных, на основании добавленных в OPC тегов
            string itemyName;
            listOfValidItems = new List<string>();
            //Формируем список пустых объектов Density
            foreach (var result in results)
            {
                if (result.Error.Succeeded)
                {
                    itemyName = result.Item.ItemId.Substring(result.Item.ItemId.LastIndexOf('!') + 1);
                    itemyName = itemyName.Substring(0, itemyName.LastIndexOf('.'));
                    if (!listOfValidItems.Any(s => s == itemyName))
                        listOfValidItems.Add(itemyName);
                }
            }
        }


        //Создание группы для OPC -для одного тега. Нахера??? :)
        protected virtual void InitSingleItemDataGroup(string groupName, string valueName, out OpcDaGroup dataGroup)
        {
            var definition = new OpcDaItemDefinition
            {
                ItemId = opcClient.ParentNodeDescriptor + valueName,
                IsActive = true
            };

            dataGroup = opcClient.OpcServer.AddGroup(groupName);                                         //Группа переменных для чтения (записи) из OPC-сервера 
            dataGroup.IsActive = true;
            OpcDaItemResult[] results = dataGroup.AddItems(new OpcDaItemDefinition[] { definition });    //Добавление переменных в группу            
        }


        //Создает пустой список, инициализирует его пустыми объектами
        protected internal abstract void CreateItemList();

        //Обновляет список переменных, занося в объекты листа данные из OPC-сервера
        protected internal abstract void UpdateItemListFromOpc();

        //Обновляет список переменных, занося в объекты листа данные из Базы Данных
        protected internal virtual void UpdateItemListFromDB(List<Temperature> temperatureList, List<Pressure> pressureList, DBPGContext dbContext) { }

        //Создает группы чтения для OPC-сервера
        protected internal virtual void CreateOPCReadGroup() { }

        //Создает группы записи для OPC-сервера
        protected internal virtual void CreateOPCWriteGroup() { }

        //Записывает данные в OPC
        protected internal virtual void WriteItemToOPC() { }

        //Записывает данные в БД
        protected internal virtual void WriteItemToDB(DBPGContext context) { }

    }


}
