using System.ComponentModel.DataAnnotations;

namespace TechParamsCalc.DataBaseConnection.Level
{
    public class TankContent
    {
        [Key]
        public int id { get; set; }

        public string tankVarDef { get; set; }
        public int tankId { get; set; }
    }
}
