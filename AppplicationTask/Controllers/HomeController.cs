using AppplicationTask.Data.Contexts;
using AppplicationTask.Data.Entities;
using AppplicationTask.Models;
using AppplicationTask.Utils;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.Identity.Client;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace AppplicationTask.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FileSystemContext _context;
        private readonly IMapper _mapper;
        private readonly IDbSerializer _serializer;

        public HomeController(ILogger<HomeController> logger, FileSystemContext context, IMapper mapper, IDbSerializer serializer)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _serializer = serializer;
        }




        [HttpGet]
        public async Task<IActionResult> Root(string path)
        {
            var fileOrFolder = await ProcessQuery(path);
            if (fileOrFolder == null)
            {
                var rootFolder = await _context.Folders
                    .Include(q=>q.Subfiles)
                    .Include(q=>q.Subfolders)
                    .Include(q=>q.ParentFolder)
                    .FirstOrDefaultAsync(q => q.Owner!.Email == HttpContext.User.Identity!.Name && q.Name == "Root");
                if (rootFolder == null)
                    return NotFound();
                else
                {
                    ViewBag.Url = "";
                    return View(_mapper.Map<Folder, FolderViewModel>(rootFolder));
                }
            }
            else
            {
                if (fileOrFolder is Data.Entities.File file)
                {
                    ViewBag.Url = await MakeUrl(file.ParentFolder!.Id);
                    return File(file.Data!, file.ContentType!, file.Name);
                }
                else
                {
                    var tempFile = (fileOrFolder as Folder)!;
                    ViewBag.Url = await MakeUrl(tempFile.Id);
                    return View(_mapper.Map<Folder, FolderViewModel>(tempFile));
                }
            }
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View(new FileViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Import(FileViewModel fileVM)
        {
            if(fileVM.formFile.ContentType != "application/octet-stream")
            {
                ModelState.AddModelError("", "Wrong file format!");
                return View(new FileViewModel());
            }

            using (var str = fileVM.formFile.OpenReadStream())
            {
                byte[] buff = new byte[str.Length];
                str.Read(buff, 0, buff.Length);
                var res = _serializer.Deserialize(buff);
                if(res == null)
                {
                    ModelState.AddModelError("", "Error!");
                    return View(new FileViewModel());
                }

                var elems = res as List<object>;

                if(elems == null)
                {
                    ModelState.AddModelError("", "Error while trying to create folder list!");
                    return View(new FileViewModel());
                }

                var owner = await _context.Users
                    .Include(q => q.Folders)
                    .FirstOrDefaultAsync(q => q.Email == HttpContext.User.Identity!.Name);

                if (owner == null)
                {
                    ModelState.AddModelError("", "Error!");
                    return View(new FileViewModel());
                }

                _context.Folders.RemoveRange(await _context.Folders.Where(q=>q.Owner == owner).ToListAsync());

                await _context.SaveChangesAsync();

                for (int i = 0; i < elems.Count; i++)
                {
                    var newGuid = Guid.NewGuid().ToString();

                    string tempGuid;

                    if (elems[i] is Folder folder)
                    {
                        tempGuid = folder.Id;
                        folder.Id = newGuid;
                        for (int j = 0; j < elems.Count; j++)
                        {
                            if (elems[j] is Folder f)
                                if (f.ParentFolderId == tempGuid)
                                    f.ParentFolderId = newGuid;
                            else if (elems[j] is Data.Entities.File ff)
                                if (ff.ParentFolderId == tempGuid)
                                    ff.ParentFolderId = newGuid;
                        }
                    }
                }

                foreach (var elem in elems)
                {
                    if(elem is Folder folder)
                    {
                        owner.Folders.Add(folder);
                    }
                    else if(elem is Data.Entities.File file)
                    {
                        _context.Files.Add(file);
                    }    
                }

                await _context.SaveChangesAsync();
                
                return RedirectToAction("Root", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Export()
        {
            var owner = await _context.Users
                .Include(q => q.Folders)
                .FirstAsync(q => q.Email == HttpContext.User.Identity!.Name);

            if (owner.Folders.Count == 1)
                return Redirect("Home/Root");

            var file = await _serializer.Serialize();
            return File(file, "application/octet-stream", DateTime.Now.ToShortDateString());
        }

        private async Task<string> MakeUrl(string folderid)
        {
            List<string> url = new List<string>();
            var folder = await _context.Folders
                .Include(q => q.ParentFolder)
                .Include(q=>q.Owner)
                .FirstOrDefaultAsync(q => q.Id == folderid);
            while(folder!.Name != "Root" && folder!.Owner!.Email == HttpContext.User.Identity!.Name)
            {
                url.Insert(0, folder!.Name!);
                folder = await _context.Folders
                .Include(q => q.ParentFolder)
                .Include(q => q.Owner)
                .FirstOrDefaultAsync(q => q.Id == folder!.ParentFolder!.Id);
            }

            return url.Aggregate("", (curr, next) => curr + next + "/");
        }

        private async Task<object?> ProcessQuery(string? query)
        {
            if(query == null)
                return null;

            var segments = query.Trim('/').Split('/');

            Folder? folder = await _context.Folders.Include(q=>q.Owner)
                                                   .Include(q=>q.Subfolders)
                                                   .Include(q=>q.Subfiles)
                                                   .Include(q=>q.ParentFolder)
                                                   .FirstOrDefaultAsync(q=>q.Name == "Root" 
                                                            && q.Owner!.Email == HttpContext.User.Identity!.Name);
            if (folder == null)
                return null;

            for (int i = 0; i < segments.Length - 1; i++)
            {
                folder = folder!.Subfolders?.FirstOrDefault(q => q.Name == segments[i]);
                
                if (folder == null)
                    return null;

                folder = await _context.Folders
                    .Include(q => q.Subfolders)
                    .Include(q => q.Subfiles)
                    .FirstOrDefaultAsync(q => q.Id == folder.Id);
            }
                
            Data.Entities.File? tempFile;
            if ((tempFile = await _context.Files
                    .Include(q=>q.ParentFolder)
                    .FirstOrDefaultAsync(q => q.Name == segments[segments.Length-1] && q.ParentFolder!.Id == folder!.Id)) != null)
                return tempFile;
            else if ((folder = await _context.Folders
                    .Include(q => q.Subfolders)
                    .Include(q => q.Subfiles)
                    .Include(q=>q.Owner)
                    .Include(q=>q.ParentFolder)
                    .FirstOrDefaultAsync(q => q.Name == segments[segments.Length-1] && q.ParentFolder!.Id == folder!.Id)) != null)
                return folder;

            return null;
        }

        [HttpGet]
        public async Task<IActionResult> AddNewFolder(string parentFolderId)
        {
            var parentFolder = await _context.Folders.FirstOrDefaultAsync(q=>q.Id == parentFolderId);
            if(parentFolder == null)
                return NotFound();
            return View(new FolderViewModel()
            {
                ParentFolderId = parentFolderId,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewFolder(FolderViewModel folderVM)
        {
            if (!ModelState.IsValid)
                return View(new FolderViewModel { ParentFolderId = folderVM.ParentFolderId });

            
            var segments = folderVM.Name!.Trim('/').Split('/');

            var parent = await _context.Folders
                .Include(q=>q.Subfolders)
                .FirstOrDefaultAsync(q=>q.Id == folderVM.ParentFolderId);

            var owner = await _context.Users.FirstAsync(q => q.Email == HttpContext.User.Identity!.Name);

           
            foreach (var segment in segments)
            {
                if(Path.GetExtension(segment) != string.Empty)
                {
                    ModelState.AddModelError("", "No file extensions allowed!");
                    return View(new FolderViewModel { ParentFolderId = folderVM.ParentFolderId });
                }

                if (parent.Subfolders.FirstOrDefault(q => q.Name == segment) != null)
                {
                    ModelState.AddModelError("", $"Folder circumstance error on {segment} in {parent.Name}!");
                    return View(new FolderViewModel { ParentFolderId = folderVM.ParentFolderId });
                }

                parent!.Subfolders!.Add(new Folder()
                {
                    Name = segment,
                    Owner = owner,
                    ParentFolder = parent,
                });

                await _context.SaveChangesAsync();

                parent = await _context.Folders
                    .Include(q=>q.Subfolders) 
                    .Include(q=>q.ParentFolder)
                    .FirstOrDefaultAsync(q=>q.Name == segment && q.ParentFolder!.Id == parent.Id);
            }
            return Redirect("Root/" + await MakeUrl(folderVM.ParentFolderId!));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFile(string? fileId)
        {
            if (fileId == null)
                return NotFound();
            var fileToDelete = await _context.Files
                .Include(q => q.ParentFolder)
                .ThenInclude(q => q!.Owner)
                .FirstOrDefaultAsync(q => q.ParentFolder!.Owner!.Email == HttpContext.User.Identity!.Name
                                        && q.Id == fileId);
         
            if (fileToDelete == null)
                return NotFound();

            var callbackIndex = fileToDelete.ParentFolder!.Id;

            _context.Remove<Data.Entities.File>(fileToDelete);

            await _context.SaveChangesAsync();

            return Redirect("Root/"+await MakeUrl(callbackIndex!));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFolder(string? folderId)
        {
            if (folderId == null)
                return Error();

            var folderToDelete = await _context.Folders
                .Include(q => q.ParentFolder)
                .ThenInclude(q => q!.Owner)
                .Include(q => q.Subfolders)
                .Include(q => q.Subfiles)
                .FirstOrDefaultAsync(q => q.ParentFolder!.Owner!.Email == HttpContext.User.Identity!.Name
                                            && q.Id == folderId);

            if (folderToDelete == null || folderToDelete.Name == "Root")
                return NotFound();

            var callbackid = folderToDelete.ParentFolder!.Id;

            await RecursiveDeleteFolder(folderToDelete);

            return Redirect("Root/" + await MakeUrl(callbackid!));
        }

        private async Task RecursiveDeleteFolder(Folder folder)
        {
            folder = await _context.Folders
                .Include(q => q.Subfolders)
                .Include(q=>q.Subfiles)
                .FirstAsync(q=>q.Id == folder.Id);
            foreach (var subfolder in folder.Subfolders.ToList())
            {
                await RecursiveDeleteFolder(subfolder);
            }

            foreach (var subfile in folder.Subfiles.ToList())
            {
                _context.Files.Remove(subfile);
            }

            _context.Folders.Remove(folder);
            await _context.SaveChangesAsync();
        }

        [HttpGet]
        public IActionResult AddNewFile(string? folderid)
        {
            if (folderid == null)
                return NotFound();
            return View(new FileViewModel()
            {
                ParentFolderId = folderid,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewFile(FileViewModel fileVM)
        {
            if (!ModelState.IsValid)
                return View(new FileViewModel { ParentFolderId = fileVM.ParentFolderId });

            var folder = await _context.Folders
                .Include(q=>q.Subfiles)
                .FirstOrDefaultAsync(q=>q.Id == fileVM.ParentFolderId);
            
            if(folder == null)
            {
                ModelState.AddModelError("", "Error!");
                return View(new FileViewModel() { ParentFolderId = fileVM.ParentFolderId });
            }

            if (folder.Subfiles.FirstOrDefault(q => q.Name == fileVM.formFile.FileName) != null)
            {
                ModelState.AddModelError("", $"File circumstance error on {fileVM.formFile.FileName} in {folder.Name}!");
                return View(new FileViewModel { ParentFolderId = fileVM.ParentFolderId });
            }

            using (var str = fileVM.formFile!.OpenReadStream())
            {
                var buff = new byte[str.Length];
                str.Read(buff, 0, (int)str.Length);
                await _context.Files.AddAsync(new Data.Entities.File()
                {
                    Name = fileVM.formFile.FileName,
                    ContentType = fileVM.formFile.ContentType,
                    Size = fileVM.formFile.Length,
                    ParentFolder = folder,
                    Data = buff
                });
                await _context.SaveChangesAsync();
            }

            return Redirect("Root/" + await MakeUrl(fileVM.ParentFolderId!));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}