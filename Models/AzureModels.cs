using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;


namespace MovieRecommendations.Models
{

    public class StringTable
    {
        public string[] ColumnNames { get; set; }
        public string[,] Values { get; set; }
    }

    public class Output
    {
        public Output() 
        {
            value_ = new StringTable();
        }

        [JsonProperty("value")]
        public StringTable value_ {get; set;}
    }
    public class Results
    {
        public Results()
        {
            output1 = new Output();
        }
        public Output output1 {get; set;}
    }
}