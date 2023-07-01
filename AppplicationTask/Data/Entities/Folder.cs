using AppplicationTask.Authentication.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Text.Json.Serialization;

namespace AppplicationTask.Data.Entities
{
    public class Folder
    {
        [Key]
        [Required]
        public string? Id { get; set; } = Guid.NewGuid().ToString();
        [MaxLength(255)]              
        [Required]
        public string? Name { get; set; }
        [JsonIgnore]
        public ICollection<Folder>? Subfolders { get; set; } = new List<Folder>();
        [JsonIgnore]
        public ICollection<File>? Subfiles { get; set; } = new List<File>();
        [Required]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        [JsonIgnore]
        [ForeignKey("ParentFolderId")]
        public Folder? ParentFolder { get; set; }
        public string? ParentFolderId { get; set; }
        [Required]
        [JsonIgnore]
        public User? Owner { get; set; }
    }
}
