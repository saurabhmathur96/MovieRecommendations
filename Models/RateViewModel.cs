using System.ComponentModel.DataAnnotations;

namespace MovieRecommendations.Models
{
    public class RateViewModel
    {
        [Required]
        [Display(Name  = "MovieId")]
        public int MovieId {get; set;}

        [Required]
        [Display(Name = "Rating")]
        public int Rating {get; set;}
    }
}