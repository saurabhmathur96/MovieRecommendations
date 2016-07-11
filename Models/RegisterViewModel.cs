using System.ComponentModel.DataAnnotations;

namespace MovieRecommendations.Models
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name  = "UserName")]
        public string UserName {get; set;}

        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password {get; set;}
    }
}