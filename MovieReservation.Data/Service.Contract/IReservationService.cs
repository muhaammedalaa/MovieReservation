using MovieReservation.Data.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Service.Contract
{
    public interface IReservationService
    {
        Task<Dtos.ReservationDetailDTO> CreateReservationAsync(Dtos.CreateReservationDTO createReservationDTO, string appUserId);
        Task<PaginatedResultDTO<Dtos.ReservationDTO>> GetReservationsByUserAsync(string appUserId, int pageNumber, int pageSize);
        Task<Dtos.ReservationDetailDTO> GetReservationByIdAsync(int reservationId);
        Task<(bool IsValid, Dtos.ReservationDetailDTO? Reservation)> ValidateReservationAsync(int reservationId, string secretCode);
        Task<SeatAvailabilityDTO> CheckSeatAvailabilityAsync(int showtimeId, int seatNumber);
        Task<bool> CancelReservationAsync(int reservationId, string appUserId);
        Task<IEnumerable<int>> GetBookedSeatsAsync(int showtimeId);
        Task<bool> ReservationExistsAsync(int reservationId);
        Task<bool> UpdateReservationSeatAsync(int reservationId, int newSeatNumber, string appUserId);
        Task<IEnumerable<int>> GetAvailableSeatsAsync(int showtimeId);
    }

}
