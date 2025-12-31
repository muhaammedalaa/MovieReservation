using AutoMapper;
using AutoMapper.Configuration.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Mapping.Reservation
{
    public class ReservationProfile : Profile
    {
        public ReservationProfile()
        {
            CreateMap<Entities.Reservation, Dtos.ReservationDTO>()
                .ForMember(dest => dest.Showtime, opt => opt.MapFrom(src => src.Showtime))
                .ReverseMap();

            CreateMap<Entities.Reservation, Dtos.ReservationDetailDTO>()
                .ForMember(dest => dest.ShowtimeId, opt => opt.MapFrom(src => src.Showtime.Id))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Showtime!.Price))
                .ForMember(dest => dest.ShowDateTime, opt => opt.MapFrom(src => src.Showtime!.StartDate))
                .ForMember(dest => dest.MovieTitle, opt => opt.MapFrom(src => src.Showtime!.Movie!.Titel))
                .ForMember(dest => dest.MoviePoster, opt => opt.MapFrom(src => src.Showtime!.Movie!.Poster))
                .ForMember(dest => dest.DurationInMinutes, opt => opt.MapFrom(src => src.Showtime!.Movie!.DurationInMinutes))
                .ForMember(dest => dest.TheaterName, opt => opt.MapFrom(src => src.Showtime!.Theater!.Name))
                .ForMember(dest => dest.TotalSeats, opt => opt.MapFrom(src => src.Showtime!.Theater!.totalSeats))
                .ReverseMap();
            CreateMap<Entities.Reservation, Dtos.UserReservationDTO>()
                .ForMember(dest => dest.ReservationId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ReservationDate, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.MovieTitle, opt => opt.MapFrom(src => src.Showtime!.Movie!.Titel))
                .ForMember(dest => dest.ShowDateTime, opt => opt.MapFrom(src => src.Showtime!.StartDate))
                .ForMember(dest => dest.MoviePoster, opt => opt.MapFrom(src => src.Showtime!.Movie!.Poster))
                .ForMember(dest => dest.TicketPrice, opt => opt.MapFrom(src => src.Showtime!.Price))
                .ForMember(dest => dest.TheaterName, opt => opt.MapFrom(src => src.Showtime!.Theater!.Name));

        }
    }
}
