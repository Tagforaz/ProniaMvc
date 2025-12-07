using Pronia_MVC.Models;
using System.ComponentModel.DataAnnotations;

namespace Pronia_MVC.ViewModels
{
    public class RegisterVM
    {
        [MinLength (6)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "UserName can only contain letters and spaces")]
        public string UserName { get; set; }
        [MaxLength(128)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [MinLength(3)]
        [MaxLength(28)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can only contain letters and spaces")]
        public string Name { get; set; }
        [MinLength(3)]
        [MaxLength(28)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Surname can only contain letters and spaces")]
        public string Surname { get; set; }
        public Gender Gender { get; set; }
        [DataType(DataType.Date)]
        public DateOnly Birthday { get; set; }
        public IFormFile? Photo { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword {  get; set; }
    }
}
