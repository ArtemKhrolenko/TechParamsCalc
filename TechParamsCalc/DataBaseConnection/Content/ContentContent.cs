using System.ComponentModel.DataAnnotations;

namespace TechParamsCalc.DataBaseConnection.Content
{
    //Class with "Content" content description (reading from PostgreSQL DB)
    public class ContentContent
    {
        public int id { get; set; }
        [Key]
        public string tagname { get; set; }
        public string comp0 { get; set; }           //name of Component 0
        public string comp1 { get; set; }           //name of Component 1
        public string comp2 { get; set; }           //name of Component 2
        public string comp3 { get; set; }           //name of Component 3
        public string comp4 { get; set; }           //name of Component 4
        public string description { get; set; }     //Description of content tag
        public string temperature { get; set; }    // temperature
        public string pressure { get; set; }       // pressure
        public bool? isWritable { get; set; } //Is tag writeble to OPC
    }
}
