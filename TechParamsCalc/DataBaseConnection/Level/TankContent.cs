using System.ComponentModel.DataAnnotations;

namespace TechParamsCalc.DataBaseConnection.Level
{
    public class TankContent
    {
        [Key]
        public int id { get; set; }

        public string tankVarDef { get; set; }
        public int tankId { get; set; }
        public int distanceA { get; set; }
        public int distanceB { get; set; }
        public int probeLength { get; set; }
        public int distToDistanceA { get; set; }



    }
}
