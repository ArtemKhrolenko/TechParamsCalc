using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechParamsCalc.DataBaseConnection;
using TechParamsCalc.DataBaseConnection.Level;
using TechParamsCalc.OPC;
using TechParamsCalc.Parameters;
using TitaniumAS.Opc.Client.Da;

namespace TechParamsCalc.Factory
{
    internal class LevelTankCreator : ItemsCreator
    {
        public List<LevelTank> LevelTankList { get; private set; }
        private SingleTagCreator singleTagCreator;
        //private OpcDaItemValue[] levelTankValues;


        private string[] itemDescLevelTankForRead = new string[0];
        private string[] itemDescLevelTankForWrite = new string[] { "H_MAX", "H_HMI", "V_HMI", "M_HMI" };

        public LevelTankCreator(OpcClient opcClient, ItemsCreator itemCreator) : base(opcClient)
        {
            subStringTagName = "_TANK";
            LevelTankList = new List<LevelTank>();
            singleTagCreator = itemCreator as SingleTagCreator;
        }


        //Метод для создания пустого списка переменных
        protected internal override void CreateItemList()
        {
            //Считываем из OPC-Reader строки с названиями переменных
            nodeElementCollection = opcClient.ReadDataToNodeList(subStringTagName).ToList();
        }


        protected internal override void UpdateItemListFromOpc()
        {
            throw new NotImplementedException();
        }


        //Обновление LevelTank тегов из Базы Данных
        protected internal void UpdateItemListFromDB(List<Level> levelList, List<Density> densityList, DBPGContext dbContext)
        {

            //Создаем список анонимных объектов 
            var collection = (from tc in dbContext.tankContents
                              join t in dbContext.tanks on tc.tankId equals t.id
                              select new { Id = tc.id, TagName = tc.tankVarDef, Tank = t }).ToList();

            //Создаем список LevelTank, параллельно инициализируем его переменными Level
            LevelTankList = new List<LevelTank>();
            collection.ForEach(x =>
            {
                var level = levelList.FirstOrDefault(l => l.TagName == x.TagName.Substring(0, x.TagName.IndexOf(subStringTagName)));
                var density = level != null ? densityList.FirstOrDefault(d => d.TagName == level.TagName + "_DENS") : null;

                if (level != null && density != null && nodeElementCollection.Any(ne => ne.Name == x.TagName))
                {
                    LevelTankList.Add(new LevelTank { Id = x.Id, TagName = x.TagName, Tank = x.Tank, Level = level, Density = density, IsWriteble = true });
                }

            });
        }


        //Создаем группу для записи в OPC-сервер  
        protected internal override void CreateOPCWriteGroup()
        {
            var LevelTanks = LevelTankList.Where(l => l.IsWriteble == true);
            //Принимаем, что список LevelTank сформировался на этапе формирования группы чтения и не содержит неправильных тегов
            this.InitDataGroup(itemDescLevelTankForWrite, "LevelTank_Write_Group", LevelTanks, out dataGroupWrite);


            //Инициализация массива объектов для будущей записи в OPC. Отбираются теги, значение "IsWriteble" == false
            this.valuesForWriting = new object[LevelTanks.Count() * itemDescLevelTankForWrite.Length]; //Массив значений для записи в OPC_сервер
        }



        //Запись тегов в OPC
        protected internal override void WriteItemToOPC()
        {
            int i = 0;
            foreach (var item in LevelTankList)
            {
                if (item.IsWriteble)
                {
                    valuesForWriting[i++] = -10;
                    valuesForWriting[i++] = (short)item.Level.Val_R * 10;
                    valuesForWriting[i++] = (short)item.Volume * 10;
                    valuesForWriting[i++] = -20;

                }

            }

            if (opcClient.OpcServer.IsConnected)
                opcClient.WriteMultiplyItems(dataGroupWrite, valuesForWriting);
        }
    }
}
