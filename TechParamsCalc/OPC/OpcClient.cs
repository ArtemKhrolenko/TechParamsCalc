using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TechParamsCalc.Parameters;
using TitaniumAS.Opc.Client.Da;
using TitaniumAS.Opc.Client.Da.Browsing;
using TitaniumAS.Opc.Client.Common;

namespace TechParamsCalc.OPC
{
    internal class OpcClient
    {
        //internal string _serverDescriptor;                //e.g. "Schneider-Aut.OFS.2"
        public string ParentNodeDescriptor { get; private set; }           //Parent OPC Node (e.g. "KNH_PO!")        
        public OpcDaServer OpcServer { get; private set; }          //Server OPC
        public int OpcGroupsCount { get; set; }

        public OpcClient(OpcDaServer opcServer, string parentNodeDescriptor)
        {
            this.ParentNodeDescriptor = parentNodeDescriptor;
            OpcServer = opcServer;
        }

        //Reads Data from OPC. Fills the Nodelist from PLC
        internal IEnumerable<OpcDaBrowseElement> ReadDataToNodeList(string _subStringPattern) //e.g. "_CAP - for Capacity tag"
        {
            //Читаем список переменных из OCP-сервера. Фильтруем переменные-ветви и отбираем те, в именах которых содержится _subStringFromTagName (например "_CAP")
            var opcDaElementFilter = new OpcDaElementFilter() { ElementType = OpcDaBrowseFilter.Branches };
            var browser = new OpcDaBrowserAuto(OpcServer);

            var items = from s in browser.GetElements(ParentNodeDescriptor, opcDaElementFilter)
                            //where s.Name.Contains(_subStringFromTagName)
                        where Regex.IsMatch(s.Name, _subStringPattern, RegexOptions.IgnoreCase)
                        select s;
            return items;
        }

        //Method writes data to "VAL_CALC" field of Capacity (Density) object and than - to OPC server        
        internal void WriteMultiplyItems(OpcDaGroup dataGroup, object[] values)
        {
            OpcDaItem[] items = dataGroup.Items.ToArray();
            try
            {
                HRESULT[] resultsWrite = dataGroup.Write(items, values);
            }
            catch (Exception)
            {
                
            }
            
        }

    }
}
