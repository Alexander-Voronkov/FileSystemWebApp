using System.ComponentModel.DataAnnotations;

namespace AppplicationTask.Authentication.Entities
{
    public class User
    {
        [Key]
        [Required]
        public string? Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
        [MinLength(8)]
        public string? Password { get; set; }
        public ICollection<Data.Entities.Folder>? Folders { get; set; } = new List<Data.Entities.Folder>();
    }
}
