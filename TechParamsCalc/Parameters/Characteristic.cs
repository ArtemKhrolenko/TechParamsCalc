using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechParamsCalc.Parameters
{
    internal abstract class Characteristic
    {
        public int Id { get; set; }
        public string TagName { get; set; }
        public string Description { get; set; }
        public bool IsInValid { get; set; }
        public bool IsWriteble { get; set; }

        internal Characteristic()
        {
        }

        internal Characteristic(string tagName)
        {
            TagName = tagName;
        }
        
    }
}
