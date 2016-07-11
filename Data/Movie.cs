using Newtonsoft.Json;

namespace MovieRecommendations.Data
{
    public class Movie 
    {
        public int MovieId {get; set;}
        public string Title {get; set;}

        [JsonProperty("poster_path")]
        public string PosterPath {get; set;}

        [JsonProperty("overview")]
        public string Overview {get; set;}

    }
}