using AutoMapper;
using MovieReservation.Data.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Mapping.Showtime
{
    public class ShowtimeProfile : Profile
    {
        public ShowtimeProfile()
        {
            // CreateMap<Source, Destination>();
            CreateMap<Entities.Showtime, Dtos.ShowtimeDTO>()
                .ForMember(dest => dest.TheaterName, opt => opt.MapFrom(src => src.Theater!.Name))
                .ForMember(dest => dest.MovieTitle, opt => opt.MapFrom(src => src.Movie!.Titel))
                 .ForMember(dest => dest.MoviePoster, opt => opt.MapFrom(src => src.Movie!.Poster))
                 .ForMember(dest => dest.DurationInMinutes, opt => opt.MapFrom(src => src.Movie!.DurationInMinutes))
                 .ForMember(dest => dest.AvailableSeats, opt => opt.MapFrom(src =>
                    src.Theater!.totalSeats - (src.Reservations!.Count)))
                 .ForMember(dest => dest.ReservedSeats, opt => opt.MapFrom(src => src.Reservations!.Count))
                 .ReverseMap()
                 .ForMember(dest => dest.Reservations, opt => opt.Ignore());

            CreateMap<Entities.Showtime, ShowtimeDetailDTO>()
               .ForMember(dest => dest.MovieTitle, opt => opt.MapFrom(src => src.Movie!.Titel))
               .ForMember(dest => dest.MovieDescription, opt => opt.MapFrom(src => src.Movie!.Description))
               .ForMember(dest => dest.MoviePoster, opt => opt.MapFrom(src => src.Movie!.Poster))
               .ForMember(dest => dest.MovieDurationInMinutes, opt => opt.MapFrom(src => src.Movie!.DurationInMinutes))
               .ForMember(dest => dest.MovieSuitableAge, opt => opt.MapFrom(src => src.Movie!.SuitableAge))
               .ForMember(dest => dest.TheaterName, opt => opt.MapFrom(src => src.Theater!.Name))
               .ForMember(dest => dest.TheaterTotalSeats, opt => opt.MapFrom(src => src.Theater!.totalSeats))
               .ForMember(dest => dest.ReservedSeats, opt => opt.MapFrom(src => src.Reservations!.Count))
               .ForMember(dest => dest.AvailableSeats, opt => opt.MapFrom(src =>
                   src.Theater!.totalSeats - (src.Reservations!.Count)));

            CreateMap<CreateShowtimeDTO, Entities.Showtime>();
            

        }
    }
}
