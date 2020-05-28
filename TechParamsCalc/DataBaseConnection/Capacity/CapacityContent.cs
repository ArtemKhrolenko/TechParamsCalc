using System.ComponentModel.DataAnnotations;

namespace TechParamsCalc.DataBaseConnection.Capacity
{
    //Class with capacity content description (reading from PostgreSQL DB)
    public class CapacityContent
    {
        public int id { get; set; }
        [Key]
        public string tagname { get; set; }
        public string perc0 { get; set; }      //name of Component 0
        public string perc1 { get; set; }         //name of Component 1
        public string perc2 { get; set; }      //name of Component 2
        public string perc3 { get; set; }         //name of Component 3
        public string perc4 { get; set; }         //name of Component 4
        public string description { get; set; }   //Description of Capacity tag

        public string temperature { get; set; } // temperature
        public string pressure { get; set; } // pressure
        public bool? isWritable { get; set; } //Is tag writeble to OPC
        public short value { get; set; } //Value
    }
}
