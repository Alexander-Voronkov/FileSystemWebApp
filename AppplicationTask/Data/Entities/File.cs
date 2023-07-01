using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AppplicationTask.Data.Entities
{
    public class File
    {
        [Key]
        [Required]
        public string? Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        [MaxLength(255)] // максимальна довжина назви файла 255 символів
        public string? Name { get; set; }
        [MaxLength(100000000)] // максимальний розмір файла 100мб
        public byte[]? Data { get; set; }
        [Required]
        [ForeignKey("ParentFolderId")]
        [JsonIgnore]
        public Folder? ParentFolder { get; set; }
        public string? ParentFolderId { get; set; }

        [Required]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        [Required]
        public string? ContentType { get; set; }
        [Required]
        public long Size { get; set; }
    }
}
