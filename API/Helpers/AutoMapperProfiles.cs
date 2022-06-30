using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        }
    }
}