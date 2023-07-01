using AppplicationTask.Data.Entities;
using AppplicationTask.Models;
using AutoMapper;

namespace AppplicationTask.MappingProfiles
{
    public class FileMappingProfile : Profile
    {
        public FileMappingProfile()
        {
            this.CreateMap<Data.Entities.File, FileViewModel>().ReverseMap();
        }
    }
}
