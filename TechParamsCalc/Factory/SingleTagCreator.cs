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
        public short PeroxideMixRatio { get; set; } //Расчетное соотношение перекиси к реакционной смеси 1 для подержания точки азиотропы
        public short AcnStrength { get; set; } //Расчетная крепость ACN в колоне 1.Т01 по расходу 100% перекиси, вычисленной по заданному расходу перекиси на реакторы

        #endregion


        #region Читаем из OPC

        //Список имен переменных для чтения
        public string[] SingleTagNames { get; private set; }

        public short[] SingleTagFromPLC { get; private set; }   //Теги обмена данными по сети от контроллера. Слово, которое сигнализирует о том, что в OPC идет запись располагается в SingleTagFromPLC[0]
        public short AtmoPressureFromOPC { get; set; } = 10101; //Тег атмосферного давления от контроллера        

        public float averReactFlow { get; set; }    //Расход реакционной смеси от А01 для расчета расхода пропилена на А02
        public float AcnWaterMassFlow { get; set; } //РАсхо массовый ACN с водой из D02
        public float S11_R01_TT01_SP { get; set; } //Задание температуры в реакторе 1.R01
        public float S11_P05_FC07_HMI { get; set; } //Массовый расход рнакционно смеси 1 после Р05
        public float S12_P02_AP01_HMI { get; set; } //% содержания перекиси водорода со склада        
        public float S11_A01_FC02_HMI { get; set; }//Расход массовый ACN из сборника 1.D02 
        public float S11_D02_AP01_HMI { get; set; } //Содержание ACN в сборнике 1.D02
        public float S12_P02_FT01_SP { get; set; } //Заданный массовый расход перекиси к реакторам
        public float S11_T01_PT05_AZEO_HMI { get; set; } //Крепость ACN в колонне 1.Т01 в точке азеотропы
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
            var simpleTagsItemDefinitions = new OpcDaItemDefinition[]
            {
                //[0] Чтение массива переменных Exchange из PLC (элемент 0 массива - статус записи Primary Sever, элемент 1 - для Secondary Server'a)
                new OpcDaItemDefinition
                {
                    ItemId = opcClient.ParentNodeDescriptor + SingleTagNames[0],
                    IsActive = true
                },
                //[1] Атмосферное давление
                new OpcDaItemDefinition
                {
                    ItemId = opcClient.ParentNodeDescriptor + SingleTagNames[1],
                    IsActive = true
                },

                //[2] Расход реакционной смеси от А01 для расчета расхода пропилена на А02 
                new OpcDaItemDefinition
                {
                    ItemId = opcClient.ParentNodeDescriptor + "S11_P05_FC08_AVER_HMI",
                    IsActive = true
                },

                //[3] РАсход массовый ACN с водой из D02
                new OpcDaItemDefinition
                {
                    ItemId = opcClient.ParentNodeDescriptor + "S11_A01_FC02_AVER_HMI",
                    IsActive = true
                },

                //[4] Задание температуры в реакторе 1.R01 для расчета дельты к заданию давления реакции (см. AdditionalCalculator класс)
                new OpcDaItemDefinition
                {
                    ItemId = opcClient.ParentNodeDescriptor + "S11_R01_TT01_SP",
                    IsActive = true
                },

                //[5] Массовый расход реакционной смеси S11_P05_FC07_HMI
                new OpcDaItemDefinition
                {
                    ItemId = opcClient.ParentNodeDescriptor + "S11_P05_FC07_HMI",
                    IsActive = true
                },

                //[6] Плотность перекиси со склада (лабораторные показатели)
                new OpcDaItemDefinition
                {
                    ItemId = opcClient.ParentNodeDescriptor + "S12_P02_AP01.HMI",
                    IsActive = true
                },

                //[7] Расход массовый ACN из сборника 1.D02 
                new OpcDaItemDefinition
                {
                    ItemId = opcClient.ParentNodeDescriptor + "S11_A01_FC02.HMI",
                    IsActive = true
                },

                //[8] Содержание ACN в сборнике 1.D02
                new OpcDaItemDefinition
                {
                    ItemId = opcClient.ParentNodeDescriptor + "S11_A01_FC02_DENS.PERC[1]",
                    IsActive = true
                },

                //[9] Заданный массовый расход перекиси к реакторам
                new OpcDaItemDefinition
                {
                    ItemId = opcClient.ParentNodeDescriptor + "S12_P02_FT01_SP",
                    IsActive = true
                },

                //[10] Крепость ACN в колонне 1.Т01 в точке азеотропы
                new OpcDaItemDefinition
                {
                    ItemId = opcClient.ParentNodeDescriptor + "S11_T01_PT05_AZEO_HMI",
                    IsActive = true
                }              


            };

            dataGroupRead = opcClient.OpcServer.AddGroup("SingleTagGroupRead");                               //Группа переменных для чтения (записи) из OPC-сервера 
            dataGroupRead.IsActive = true;
            OpcDaItemResult[] results = dataGroupRead.AddItems(simpleTagsItemDefinitions);                   //Добавление переменных в группу             

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

            #region Переприсвоение тегов из массива items, прочитанных из OPC

            //[0] Массив short'ов [0-100]

            if (singleValues[0].Error.Succeeded)
                SingleTagFromPLC = (short[])(singleValues[0].Value);

            //[1] Тег атмосферного давления от контроллера        

            if (singleValues[1].Error.Succeeded)
                AtmoPressureFromOPC = (short)singleValues[1].Value;

            //[2] Усредненный объемный расход реакционной смеси    

            if (singleValues[2].Error.Succeeded)
                averReactFlow = (short)singleValues[2].Value * 0.1f;

            //[3] Усредненный массовый расход ACN и воды из A02

            if (singleValues[3].Error.Succeeded)
                AcnWaterMassFlow = (short)(singleValues[3].Value) * 0.1f;

            //[4] Задание температуры в реакторе 1.R01 для расчета дельты к заданию давления реакции (см. AdditionalCalculator класс)

            if (singleValues[4].Error.Succeeded)
                S11_R01_TT01_SP = (short)(singleValues[4].Value) * 0.1f;

            //[5] Массовый расход реакционной смеси S11_P05_FC07_HMI
            if (singleValues[5].Error.Succeeded)
                S11_P05_FC07_HMI = (short)(singleValues[5].Value) * 0.1f;

            //[6] Плотность перекиси со склада (лабораторные показатели)
            if (singleValues[6].Error.Succeeded)
                S12_P02_AP01_HMI = (short)(singleValues[6].Value) * 0.01f;

            //[7] Расход массовый ACN из сборника 1.D02 
            if (singleValues[7].Error.Succeeded)
                S11_A01_FC02_HMI = (short)(singleValues[7].Value) * 0.1f;

            //[8] Содержание ACN в сборнике 1.D02
            if (singleValues[8].Error.Succeeded)
                S11_D02_AP01_HMI = (short)(singleValues[8].Value) * 0.01f;

            //[9] Заданный массовый расход перекиси к реакторам
            if (singleValues[9].Error.Succeeded)
                S12_P02_FT01_SP = (short)(singleValues[9].Value) * 0.1f;

            //Крепость ACN в колонне 1.Т01 в точке азеотропы
            if (singleValues[10].Error.Succeeded)
                S11_T01_PT05_AZEO_HMI = (short)(singleValues[10].Value) * 0.01f;
            #endregion
        }


        protected internal void CreateOPCWriteGroup(bool isPrimaryServer)
        {
            OpcDaItemResult[] results;

            //Создаем группу для записи из OPC-сервера

            //---Группа 1 (синхронная запись)---------------------//

            dataGroupForSynchroWriting = opcClient.OpcServer.AddGroup("SingleTagSynchroGroupWrite");         //Группа переменных для одновременной записи в  OPC 
            dataGroupForSynchroWriting.IsActive = true;

            var syncWriteItems = new OpcDaItemDefinition[]
            {
                new OpcDaItemDefinition
                {
                    ItemId = opcClient.ParentNodeDescriptor + SingleTagNames[0] + (isPrimaryServer ? "[0]" : "[1]"),
                    IsActive = true
                }
        };

            results = dataGroupForSynchroWriting.AddItems(syncWriteItems);
            syncValuesForWriting = new object[results.Count()]; //Массив значений для записи в OPC_сервер


            //---Группа 2 (Асинхронная запись)---------------------//

            dataGroupWrite = opcClient.OpcServer.AddGroup("SingleTagAsynchroGroupWrite");          //Группа переменных для разразненной записи в  OPC  
            dataGroupWrite.IsActive = true;

            var aSyncWriteItems = new OpcDaItemDefinition[]
            {
                //Переменая "Масса пропилена" для задания расхода пропилена на T02
                new OpcDaItemDefinition
                {
                    ItemId = opcClient.ParentNodeDescriptor + SingleTagNames[0] + "[20]",
                    IsActive = true
                },

                //Расчетный перепад давления в 1.E06 после теплообменника 1.E23
                new OpcDaItemDefinition
                {
                    ItemId = opcClient.ParentNodeDescriptor + SingleTagNames[0] + "[21]",
                    IsActive = true
                },

                //Расчетная дельта к заданию давления реакции (см. AdditionalCalculator класс)
                new OpcDaItemDefinition
                {
                    ItemId = opcClient.ParentNodeDescriptor + SingleTagNames[0] + "[22]",
                    IsActive = true
                },

                //Расчетная крепость ACN в колонне 1.Т01 для поддержания точки азиотропы
                new OpcDaItemDefinition
                {
                    ItemId = opcClient.ParentNodeDescriptor + SingleTagNames[0] + "[23]",
                    IsActive = true
                },

                //Расчетная крепость ACN в колоне 1.Т01 по расходу 100% перекиси, вычисленной по заданному расходу перекиси на реакторы
                new OpcDaItemDefinition
                {
                    ItemId = opcClient.ParentNodeDescriptor + SingleTagNames[0] + "[24]",
                    IsActive = true
                }                
        };

            results = dataGroupWrite.AddItems(aSyncWriteItems);
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
            valuesForWriting[3] = PeroxideMixRatio;
            valuesForWriting[4] = AcnStrength;
            //........Добавить при необходимости

            if (opcClient.OpcServer.IsConnected)
                opcClient.WriteMultiplyItems(dataGroupWrite, valuesForWriting);
        }

    }
}
