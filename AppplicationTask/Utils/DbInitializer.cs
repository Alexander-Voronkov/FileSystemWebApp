using AppplicationTask.Data.Contexts;

namespace AppplicationTask.Utils
{
    public static class DbInitializer
    {
        public static async Task Init(FileSystemContext context)
        {
            if(context != null && !context.Users.Any() && !context.Folders.Any())
            {
                var user = new Authentication.Entities.User
                {
                    Email = "admin@gmail.com",
                    Password = new HashService().HashPassword("12345678")
                };
                var root = new Data.Entities.Folder
                {
                    Name = "Root"
                };
                var f1 = new Data.Entities.Folder
                {
                    Name = "Creating Digital Images",
                    ParentFolder = root
                };
                var f2 = new Data.Entities.Folder
                {
                    Name = "Resources",
                    ParentFolder = f1
                };
                var f3 = new Data.Entities.Folder
                {
                    Name = "Evidence",
                    ParentFolder = f1
                };
                var f4 = new Data.Entities.Folder
                {
                    Name = "Graphic Products",
                    ParentFolder = f1
                };
                var f5 = new Data.Entities.Folder
                {
                    Name = "Primary Resources",
                    ParentFolder = f2
                };
                var f6 = new Data.Entities.Folder
                {
                    Name = "Secondary Resources",
                    ParentFolder = f2
                };
                var f7 = new Data.Entities.Folder
                {
                    Name = "Process",
                    ParentFolder = f4
                };
                var f8 = new Data.Entities.Folder
                {
                    Name = "Final Product",
                    ParentFolder = f4
                };
                user.Folders.Add(root);
                user.Folders.Add(f1);
                user.Folders.Add(f2);
                user.Folders.Add(f3);
                user.Folders.Add(f4);
                user.Folders.Add(f5);
                user.Folders.Add(f6);
                user.Folders.Add(f7);
                user.Folders.Add(f8);
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
            }
        }
    }
}
