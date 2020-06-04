using System.ComponentModel.DataAnnotations;

namespace TechParamsCalc.DataBaseConnection.Level
{
    public class Tank
    {
        [Key]
        public int id { get; set; }      
       

        public string tankDef { get; set; }

        public double GetVolume()
        {            
            return id * 0.5;
        }
    }
}
