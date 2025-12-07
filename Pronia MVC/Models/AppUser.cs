using Microsoft.AspNetCore.Identity;


namespace Pronia_MVC.Models
{
    public class AppUser:IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Image {  get; set; }
        public DateOnly Birthday { get; set; }
        public Gender Gender { get; set; }


    }
}
