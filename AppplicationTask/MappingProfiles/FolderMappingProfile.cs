using AppplicationTask.Data.Entities;
using AppplicationTask.Models;
using AutoMapper;

namespace AppplicationTask.MappingProfiles
{
    public class FolderMappingProfile : Profile
    {
        public FolderMappingProfile() 
        {
            this.CreateMap<Folder, FolderViewModel>().ReverseMap();
        }
    }
}
