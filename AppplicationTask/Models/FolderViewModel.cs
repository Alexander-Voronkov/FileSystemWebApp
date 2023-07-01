using AppplicationTask.Data.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AppplicationTask.Models
{
    public class FolderViewModel
    {
        public string? Id { get; set; }
        [Required(ErrorMessage = "Folder name is required!")]
        [MaxLength(255, ErrorMessage = "Folder name must contain less than 255 symbols")]
        [DisplayName("Enter folder name: ")]
        public string? Name { get; set; }
        public ICollection<Folder>? Subfolders { get; set; } = new List<Folder>();
        public ICollection<Data.Entities.File>? Subfiles { get; set; } = new List<Data.Entities.File>();
        public Folder? ParentFolder { get; set; }
        public string? ParentFolderId { get; set; }
    }
}
