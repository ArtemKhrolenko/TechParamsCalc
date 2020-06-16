using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechParamsCalc.OPC;
using TitaniumAS.Opc.Client.Da;
using TitaniumAS.Opc.Client.Da.Browsing;

namespace TechParamsCalc.Factory
{
    internal class SingleTagCreator : ItemsCreator
    {
        
       
       
        #region Пишем в OPC

        public short IsWritableTagToPLC { get; set; }  //Слово, которое сигнализирует о том, что в OPC идет запись
        public short PropyleneMass { get; set; }    //Расчетная масса пропилена для расчта задания расхода на Т02
        public short DeltaPE06 { get; set; }    //Расчетный перепад давления в 1.E06 после теплообменника 1.E23
        public short DeltaPR01 { get; set; } //Расчетная дельта к заданию давления реакции(см.AdditionalCalculator класс)

        #endregion


        #region Читаем из OPC

        //Список имен переменных для чтения
        public string[] SingleTagNames { get; private set; }

        public short[] SingleTagFromPLC { get; private set; }   //Теги обмена данными по сети от контроллера. Слово, которое сигнализирует о том, что в OPC идет запись располагается в SingleTagFromPLC[0]
        public short AtmoPressureFromOPC { get; set; } = 10101; //Тег атмосферного давления от контроллера        

        public float averReactFlow { get; set; } //Расход реакционной смеси от А01 для расчета расхода пропилена на А02
        public float AcnWaterMassFlow { get; set; } //РАсхо массовый ACN с водой из D02
        public float S11_R01_TT01_SP { get; set; } //Задание температуры в реакторе 1.R01
        #endregion

        //группа для записи переменных, которые должны писаться одновременно двумя серверами
        private OpcDaGroup dataGroupForSynchroWriting;
        private object[] syncValuesForWriting;

        public SingleTagCreator(OpcClient opcClient, string[] singleTagNames) : base(opcClient)
        {
            SingleTagNames = singleTagNames;
            SingleTagFromPLC = new short[100];

        }

        protected internal override void CreateItemList()
        {
            throw new NotImplementedException();
        }

       
        protected internal override void CreateOPCReadGroup()
        {
            //Создаем группу для чтения из OPC-сервера

            //Чтение массива переменных Exchange из PLC (элемент 0 массива - статус записи Primary Sever, элемент 1 - для Secondary Server'a)
            var definition = new OpcDaItemDefinition
            {
                ItemId = opcClient.ParentNodeDescriptor + SingleTagNames[0],
                IsActive = true
            };
            var definition2 = new OpcDaItemDefinition
            {
                ItemId = opcClient.ParentNodeDescriptor + SingleTagNames[1],
                IsActive = true
            };

            //Расход реакционной смеси от А01 для расчета расхода пропилена на А02 
            var definition3 = new OpcDaItemDefinition
            {
                ItemId = opcClient.ParentNodeDescriptor + SingleTagNames[2] + "_AVER_HMI",
                IsActive = true
            };

            //РАсход массовый ACN с водой из D02
            var definition4 = new OpcDaItemDefinition
            {
                ItemId = opcClient.ParentNodeDescriptor + SingleTagNames[3] + "_AVER_HMI",
                IsActive = true
            };

            //Задание температуры в реакторе 1.R01 для расчета дельты к заданию давления реакции (см. AdditionalCalculator класс)
            var definition5 = new OpcDaItemDefinition
            {
                ItemId = opcClient.ParentNodeDescriptor + "S11_R01_TT01_SP",
                IsActive = true
            };

            dataGroupRead = opcClient.OpcServer.AddGroup("SingleTagGroupRead");                               //Группа переменных для чтения (записи) из OPC-сервера 
            dataGroupRead.IsActive = true;
            OpcDaItemResult[] results = dataGroupRead.AddItems(new OpcDaItemDefinition[] { definition, definition2, definition3, definition4, definition5 });     //Добавление переменных в группу             

        }

        protected internal override void UpdateItemListFromOpc()
        {
            OpcDaItemValue[] singleValues;
            try
            {
                singleValues = dataGroupRead.Read(dataGroupRead.Items, OpcDaDataSource.Device);
            }
            catch (Exception e)
            {
                throw new Exception("Error in group creating for single tags!" + e.Message);
            }


            //Массив short'ов [0-100]
            try
            {
                if (singleValues[0].Value != null)
                    SingleTagFromPLC = (short[])(singleValues[0].Value);
            }
            catch (Exception)
            {
                SingleTagFromPLC[0] = -1;
            }

            //Тег атмосферного давления от контроллера
            try
            {
                if (singleValues[1].Value != null)
                    AtmoPressureFromOPC = (short)singleValues[1].Value;
            }
            catch (Exception)
            {
                AtmoPressureFromOPC = 10100;
            }

            // Усредненный объемный расход реакционной смеси 
            try
            {
                if (singleValues[2].Value != null)
                    averReactFlow = (short)singleValues[2].Value * 0.1f;
            }
            catch (Exception)
            {          
                
            }

            // Усредненный массовый расход ACN и воды из A02
            try
            {
                if (singleValues[3].Value != null)
                    AcnWaterMassFlow = (short)(singleValues[3].Value) * 0.1f;
            }
            catch (Exception)
            {

            }

            // Задание температуры в реакторе 1.R01 для расчета дельты к заданию давления реакции (см. AdditionalCalculator класс)
            try
            {
                if (singleValues[4].Value != null)
                    S11_R01_TT01_SP = (short)(singleValues[4].Value) * 0.1f;
            }
            catch (Exception)
            {

            }
        }


        protected internal void CreateOPCWriteGroup(bool isPrimaryServer)
        {
            OpcDaItemResult[] results;

            //Создаем группу для записи из OPC-сервера

            //---Группа 1 (синхронная запись)---------------------//
            var definition = new OpcDaItemDefinition
            {
                ItemId = opcClient.ParentNodeDescriptor + SingleTagNames[0] + (isPrimaryServer ? "[0]" : "[1]"),
                IsActive = true
            };

            dataGroupForSynchroWriting = opcClient.OpcServer.AddGroup("SingleTagSynchroGroupWrite");         //Группа переменных для одновременной записи в  OPC 
            dataGroupForSynchroWriting.IsActive = true;
            results = dataGroupForSynchroWriting.AddItems(new OpcDaItemDefinition[] { definition });

            syncValuesForWriting = new object[results.Count()]; //Массив значений для записи в OPC_сервер

            //---Группа 2 (Асинхронная запись)---------------------//

            //Переменая "Масса пропилена" для задания расхода пропилена на T02
            var definition2 = new OpcDaItemDefinition
            {
                ItemId = opcClient.ParentNodeDescriptor + SingleTagNames[0] + "[20]",
                IsActive = true
            };

            //Расчетный перепад давления в 1.E06 после теплообменника 1.E23
            var definition3 = new OpcDaItemDefinition
            {
                ItemId = opcClient.ParentNodeDescriptor + SingleTagNames[0] + "[21]",
                IsActive = true
            };

            //Расчетная дельта к заданию давления реакции (см. AdditionalCalculator класс)
            var definition4 = new OpcDaItemDefinition
            {
                ItemId = opcClient.ParentNodeDescriptor + SingleTagNames[0] + "[22]",
                IsActive = true
            };

            dataGroupWrite = opcClient.OpcServer.AddGroup("SingleTagAsynchroGroupWrite");          //Группа переменных для разразненной записи в  OPC  
            dataGroupWrite.IsActive = true;
            results = dataGroupWrite.AddItems(new OpcDaItemDefinition[] { definition2, definition3, definition4 });
            valuesForWriting = new object[results.Count()];
        }


        //Запись синхронной группы
        protected internal void WriteSyncItemToOPC()
        {
            //Первое значение синхронной группы - всегда бит активности сервера
            syncValuesForWriting[0] = IsWritableTagToPLC;

            if (opcClient.OpcServer.IsConnected)
                opcClient.WriteMultiplyItems(dataGroupForSynchroWriting, syncValuesForWriting);
        }

        //Запись асинхронной группы
        protected internal void WriteASyncItemToOPC()
        {
            valuesForWriting[0] = PropyleneMass;
            valuesForWriting[1] = DeltaPE06;
            valuesForWriting[2] = DeltaPR01;
            //........Добавить при необходимости

            if (opcClient.OpcServer.IsConnected)
                opcClient.WriteMultiplyItems(dataGroupWrite, valuesForWriting);
        }

    }
}
