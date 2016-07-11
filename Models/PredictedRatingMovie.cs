using Newtonsoft.Json;

namespace MovieRecommendations.Models
{
    public class PredictedRatingMovie
    {
        public int MovieId {get; set;}
        public double Rating {get; set;}
        public string Title {get; set;}

        [JsonProperty("poster_path")]
        public string PosterPath {get; set;}

        [JsonProperty("overview")]
        public string Overview {get; set;}

    }
}