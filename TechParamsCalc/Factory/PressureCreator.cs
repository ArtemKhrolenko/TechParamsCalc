using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechParamsCalc.OPC;
using TitaniumAS.Opc.Client.Da;
using TechParamsCalc.Parameters;

namespace TechParamsCalc.Factory
{
    class PressureCreator : ItemsCreator
    {
        private string[] itemDescPressureForRead = new string[] { "R" };
        private string[] itemDescPressureForWrite = new string[0];
        private int countOfErrorsInReading;

        private OpcDaItemValue[] pressureValues;
        public List<Pressure> PressureList { get; private set; }

        public PressureCreator(OpcClient opcClient) : base(opcClient)
        {
            subStringTagName = "_PT";
            PressureList = new List<Pressure>();
        }

        protected internal override void CreateItemList()
        {
            //Считываем из OPC-Reader строки с названиями переменных
            nodeElementCollection = opcClient.ReadDataToNodeList(subStringTagName).ToList();

        }

        //Создаем группу для чтения из OPC-сервера
        protected internal override void CreateOPCReadGroup()
        {
            OpcDaItemResult[] readingResults;
            List<string> listOfValidItems;

            this.InitDataGroup(itemDescPressureForRead, "Pressures_Read_Group", nodeElementCollection, out dataGroupRead, out readingResults, out listOfValidItems);
            listOfValidItems.ForEach(i => PressureList.Add(new Pressure(i)));

        }

        //Обновляем список Pressure данными из OPC
        protected internal override void UpdateItemListFromOpc()
        {
            countOfErrorsInReading = 0;
            pressureValues = dataGroupRead.Read(dataGroupRead.Items, OpcDaDataSource.Device);

            int _valueCollectionIterator = 0;
            var _pressure = default(Pressure);
            foreach (var item in PressureList)
            {
                try
                {
                    //Initialization of fields of temperature instance
                    _pressure = PressureList.FirstOrDefault(c => c.TagName == item.TagName);
                    if (_pressure != null && pressureValues[0 + _valueCollectionIterator].Error.Succeeded)
                    {
                        _pressure.Val_R = (float)pressureValues[0 + _valueCollectionIterator].Value;
                    }
                    else
                        countOfErrorsInReading++;
                }
                catch (Exception)
                {
                    throw new Exception("Error in Pressures UpdateItemListFromOpc");
                }
                finally
                {
                    _valueCollectionIterator += itemDescPressureForRead.Length;
                }
                
            }
            if (countOfErrorsInReading > 0)
                throw new Exception($"Количество ошибок чтения давлений - {countOfErrorsInReading}");
        }

        



    }
}
