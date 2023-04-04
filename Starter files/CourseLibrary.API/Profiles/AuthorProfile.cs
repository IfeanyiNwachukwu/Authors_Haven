using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;

namespace CourseLibrary.API.Profiles;
public class AuthorsProfile : Profile
{
    public AuthorsProfile()
    {
        CreateMap<Entities.Author, Models.AuthorDto>()
            .ForMember(dest => dest.Name, opt => 
                opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.Age, opt => 
                opt.MapFrom(src => src.DateOfBirth.GetCurrentAge()));

        CreateMap<AuthorForCreationDto, Author>();
         
    }
}

