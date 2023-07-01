using AppplicationTask.Authentication.Entities;
using AppplicationTask.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace AppplicationTask.Data.Contexts
{
    public class FileSystemContext : DbContext
    {
        public FileSystemContext(DbContextOptions<FileSystemContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

        public DbSet<Folder> Folders => Set<Folder>();
        [JsonIgnore]
        public DbSet<User> Users => Set<User>();
        public DbSet<Data.Entities.File> Files => Set<Data.Entities.File>();
    }
}
