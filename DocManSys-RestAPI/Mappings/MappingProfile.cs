using AutoMapper;
using DocManSys_DAL.Entities;
using DocManSys_RestAPI.Models;

namespace DocManSys_RestAPI.Mappings {
    public class MappingProfile : Profile {
        public MappingProfile() {
            CreateMap<Document, DocumentEntity>();
            CreateMap<DocumentEntity, Document>();
        }
    }
}
