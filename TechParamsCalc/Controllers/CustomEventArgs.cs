using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechParamsCalc.Controllers
{
    class CustomEventArgs : EventArgs
    {
        public bool IsSucceed { get; set; }
        public string ErrorMessage { get; set; }
    }
}
