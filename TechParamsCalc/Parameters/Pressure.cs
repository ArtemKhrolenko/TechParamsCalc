using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechParamsCalc.Parameters
{
    internal class Pressure : Characteristic
    {
        private static int id = 0;
        public float Val_R { get; set; }

        public Pressure(string tagName) : base(tagName)
        {            
            Id = ++id;
        }

        public Pressure(string tagName, float val_r) : this(tagName)
        {
            Val_R = val_r;
        }
    }
}
