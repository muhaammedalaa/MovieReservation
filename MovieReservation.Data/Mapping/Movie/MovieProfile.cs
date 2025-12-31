using AutoMapper;
using MovieReservation.Data.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Mapping.Movie
{
    public class MovieProfile : Profile
    {
        public MovieProfile()
        {
            // CreateMap<Source, Destination>();
            CreateMap<Entities.Movie, MovieDTO>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Titel))
                .ForMember(dest => dest.ReleaseDate, opt => opt.MapFrom(src => src.releaseDate))
                .ReverseMap()
                .ForMember(dest => dest.Titel, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.releaseDate, opt => opt.MapFrom(src => src.ReleaseDate));
            CreateMap<Entities.Movie, MovieDetailDTO>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Titel))
            .ForMember(dest => dest.ReleaseDate, opt => opt.MapFrom(src => src.releaseDate))
            .ForMember(dest => dest.Showtimes, opt => opt.MapFrom(src => src.Showtimes))
            .ReverseMap()
            .ForMember(dest => dest.Titel, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.releaseDate, opt => opt.MapFrom(src => src.ReleaseDate));
        }
    }
}
