using AutoMapper;
using DocManSys_DAL.Entities;
using DocManSys_RestAPI.Models;

namespace DocManSys_RestAPI.Mappings {
    public class MappingProfile : Profile {
        public MappingProfile() {
            CreateMap<Document, DocumentEntity>()
                .ForMember(dest => dest.Id, opt 
                    => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt 
                    => opt.MapFrom(src => $"*{src.Title ?? string.Empty}*"))
                .ForMember(dest => dest.Author, opt
                    => opt.MapFrom(src => $"*{src.Author ?? string.Empty}*"))
                .ForMember(dest => dest.Image, opt
                    => opt.MapFrom(src => $"*{src.Image ?? string.Empty}*"))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt
                    => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt
                    => opt.MapFrom(src => (src.Title ?? string.Empty).Replace("*", "")))
                .ForMember(dest => dest.Author, opt
                    => opt.MapFrom(src => (src.Author ?? string.Empty).Replace("*", "")))
                .ForMember(dest => dest.Image, opt
                    => opt.MapFrom(src => (src.Image ?? string.Empty).Replace("*", "")));
        }
    }
}
