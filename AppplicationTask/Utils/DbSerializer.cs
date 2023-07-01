using AppplicationTask.Data.Contexts;
using AppplicationTask.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;

namespace AppplicationTask.Utils
{
    public class DbSerializer : IDbSerializer
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly FileSystemContext _fileSystemContext;
        public DbSerializer(IHttpContextAccessor httpContext, FileSystemContext fileSystemContext)
        {
            _httpContext = httpContext;
            _fileSystemContext = fileSystemContext;
        }

        public async Task<byte[]> Serialize()
        {
            var owner = await _fileSystemContext.Users
                .Include(q=>q.Folders)
                .FirstAsync(q => q.Email == _httpContext.HttpContext.User.Identity!.Name);

            var files = await _fileSystemContext.Files
                .Include(q => q.ParentFolder)
                .ThenInclude(q => q.Owner)
                .Where(q => q.ParentFolder.Owner == owner)
                .ToListAsync();

            var folders = owner.Folders.ToList();
            
            
            return Encoding.UTF8.GetBytes(
                
                JsonSerializer.Serialize(new string[] 
                { 
                    JsonSerializer.Serialize(folders, typeof(List<Folder>)),
                    JsonSerializer.Serialize(files, typeof(List<Data.Entities.File>)) 
                }));
        }

        public object? Deserialize(byte[] obj)
        {
            var arr = JsonSerializer.Deserialize(Encoding.UTF8.GetString(obj), typeof(string[])) as string[];

            if (arr == null)
                throw new Exception("Error while deserializing!");

            var folders = JsonSerializer.Deserialize(arr[0], typeof(List<Folder>)) as List<Folder>;

            if(folders == null)
                throw new Exception("Error while deserializing!");

            var files = JsonSerializer.Deserialize(arr[1], typeof(List<Data.Entities.File>)) as List<Data.Entities.File>;

            if (files == null)
                throw new Exception("Error while deserializing!");

            return folders.Cast<object>().Concat(files).ToList();
        }
    }
}
