using System.ComponentModel.DataAnnotations;

namespace TechParamsCalc.DataBaseConnection.Level
{
    public class Tank
    {
        [Key]
        public int tankId { get; set; }
       

        public string tankDef { get; set; }
    }
}
