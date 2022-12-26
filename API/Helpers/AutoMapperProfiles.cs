using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser,MemberDto>()
                .ForMember(dest => dest.PhotoUrl,op => op.MapFrom(src => 
                    src.Photos.FirstOrDefault(x=>x.IsMain).Url))
                .ForMember(dest => dest.Age,op => op.MapFrom(src =>
                src.DateOfBirth.CalculateAge()));
            CreateMap<Photo,PhotoDto>();
            CreateMap<MemberUpdateDto,AppUser>();
            CreateMap<RegisterDto,AppUser>();
            CreateMap<Message,MessageDto>()
                .ForMember(d => d.SenderPhotoUrl,o=> o.MapFrom(s => s.Sender.Photos
                        .FirstOrDefault(x=>x.IsMain).Url))
                .ForMember(d => d.RecipientPhotoUrl,o=> o.MapFrom(s => s.Recipient.Photos
                        .FirstOrDefault(x=>x.IsMain).Url));
        }
    }
}