using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MovieRecommendations.Data
{
    public class UserRating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id {get; set;}
        public int MovieId {get; set;}
        public int Rating {get; set;}
        public string UserName {get; set;}
    }
}