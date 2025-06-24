using System.ComponentModel.DataAnnotations;

namespace RecipePlatform.MVC.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required, StringLength(20)]
        public string? UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email address")]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required]
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }
    }
}
