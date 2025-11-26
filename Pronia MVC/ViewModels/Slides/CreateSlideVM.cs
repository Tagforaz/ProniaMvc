using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pronia_MVC.ViewModels
{
    public class CreateSlideVM
    {
        [MaxLength(100, ErrorMessage = "Deyer 50-den chox ola bilmez")]
        [MinLength(2)]
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public IFormFile Photo { get; set; }
    }
}
