using AutoMapper;

namespace DocManSys_RestAPI.Mappings {
    public class MappingProfile : Profile {
        public MappingProfile() {
            CreateMap<DocManSys_DAL.Entities.Document, Models.Document>()
                .ForMember(dest => dest.Id, opt 
                    => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt 
                    => opt.MapFrom(src => $"*{src.Title ?? string.Empty}*"))
                .ForMember(dest => dest.Author, opt
                    => opt.MapFrom(src => $"*{src.Author ?? string.Empty}*"))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt
                    => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt
                    => opt.MapFrom(src => (src.Title ?? string.Empty).Replace("*", "")))
                .ForMember(dest => dest.Author, opt
                    => opt.MapFrom(src => (src.Author ?? string.Empty).Replace("*", "")));
        }
    }
}
