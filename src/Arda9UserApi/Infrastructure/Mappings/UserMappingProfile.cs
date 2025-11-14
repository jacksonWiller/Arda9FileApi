using Arda9UserApi.Application.DTOs;
using Arda9UserApi.Domain.Entities.UserAggregate;
using AutoMapper;
using Catalog.Domain.ValueObjects;

namespace Arda9UserApi.Infrastructure.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        // User -> UserDto
        CreateMap<User, UserDto>()
            //.ForMember(dest => dest.PK, opt => opt.Ignore())
            //.ForMember(dest => dest.SK, opt => opt.Ignore())
            //.ForMember(dest => dest.GSI1PK, opt => opt.Ignore())
            //.ForMember(dest => dest.GSI1SK, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Value))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone != null 
                ? new PhoneDto { CountryCode = src.Phone.CountryCode, Number = src.Phone.Number } 
                : null))
            .ForMember(dest => dest.PictureUrl, opt => opt.MapFrom(src => src.PictureUrl != null ? src.PictureUrl.Value : null))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // UserDto -> User
        CreateMap<UserDto, User>()
            .ConstructUsing(dto => new User(
                dto.CompanyId,
                new Email(dto.Email),
                new PersonName(dto.Name),
                dto.Phone != null ? new Phone(dto.Phone.Number, dto.Phone.CountryCode) : null,
                dto.Roles,
                !string.IsNullOrEmpty(dto.PictureUrl) ? new Url(dto.PictureUrl) : null,
                dto.Locale,
                dto.CognitoSub,
                dto.Username,
                dto.SubCompanyId,
                dto.CreatedBy
            ));
    }
}