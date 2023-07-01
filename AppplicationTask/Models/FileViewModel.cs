using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AppplicationTask.Models
{
    public class FileViewModel
    {
        [Required(ErrorMessage = "Select a file.")]
        [DisplayName("Select a file: ")]
        public IFormFile? formFile { get; set; }
        public string? ParentFolderId { get; set; }
    }
}
