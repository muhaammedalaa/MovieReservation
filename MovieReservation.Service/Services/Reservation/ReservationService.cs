using AutoMapper;
using MovieReservation.Data.Contracts;
using MovieReservation.Data.Dtos;
using MovieReservation.Data.Entities;
using MovieReservation.Data.Service.Contract;
using MovieReservation.Data.Specification.Reservation_Specifications;
using MovieReservation.Data.Specification.Showtime_Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Service.Services.Reservation
{
    public class ReservationService : IReservationService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public ReservationService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> CancelReservationAsync(int reservationId, string appUserId)
        {
            if (reservationId <= 0)
                throw new ArgumentException("Reservation ID must be a positive integer", nameof(reservationId));
            if (string.IsNullOrWhiteSpace(appUserId))
                throw new ArgumentException("App user ID cannot be null or empty", nameof(appUserId));
            var reservation = await _unitOfWork.Repository<Data.Entities.Reservation>()
                 .GetByIdAsync(reservationId);
            if (reservation == null)
                throw new InvalidOperationException($"Reservation with ID {reservationId} does not exist.");
            if (reservation.AppUserId != appUserId)
                throw new UnauthorizedAccessException("You are not authorized to cancel this reservation.");
            _unitOfWork.Repository<Data.Entities.Reservation>().Delete(reservation);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<SeatAvailabilityDTO> CheckSeatAvailabilityAsync(int showtimeId, int seatNumber)
        {
            if (showtimeId <= 0)
                throw new ArgumentException("Showtime ID must be a positive integer", nameof(showtimeId));
            if (seatNumber <= 0)
                throw new ArgumentException("Seat number must be a positive integer", nameof(seatNumber));
            var spec = new CheckSeatAvailabilitySpec(showtimeId, seatNumber);
            var reservationTask = await _unitOfWork.Repository<Data.Entities.Reservation>()
                .GetSingleAsync(spec);
            var isAvailable = reservationTask == null;
            return new SeatAvailabilityDTO
            {
                ShowtimeId = showtimeId,
                SeatNumber = seatNumber,
                IsAvailable = isAvailable,
                Message = isAvailable ? $"Seat {seatNumber} for showtime {showtimeId} is available." : $"Seat {seatNumber} for showtime {showtimeId} is already booked."
            };



        }

        public async Task<ReservationDetailDTO> CreateReservationAsync(CreateReservationDTO createReservationDTO, string appUserId)
        {
            if (createReservationDTO == null)
                throw new ArgumentNullException(nameof(createReservationDTO));
            if (string.IsNullOrWhiteSpace(appUserId))
                throw new ArgumentException("App user ID cannot be null or empty", nameof(appUserId));
            if (createReservationDTO.SeatNumber <= 0)
                throw new ArgumentException("Seat number must be a positive integer", nameof(createReservationDTO.SeatNumber));
            if (createReservationDTO.ShowtimeId <= 0)
                throw new ArgumentException("Showtime ID must be a positive integer", nameof(createReservationDTO.ShowtimeId));
            var showtimeTask = await _unitOfWork.Repository<Data.Entities.Showtime>()
                .GetByIdAsync(createReservationDTO.ShowtimeId);
            if (showtimeTask == null)
                throw new InvalidOperationException($"Showtime with ID {createReservationDTO.ShowtimeId} does not exist.");
            var seatBookedTask = await CheckSeatAvailabilityAsync(createReservationDTO.ShowtimeId, createReservationDTO.SeatNumber);
            if (!seatBookedTask.IsAvailable)
                throw new InvalidOperationException($"Seat {createReservationDTO.SeatNumber} for showtime {createReservationDTO.ShowtimeId} is already booked.");
            if (createReservationDTO.SeatNumber > showtimeTask.Theater.totalSeats)
                throw new ArgumentException($"Seat number {createReservationDTO.SeatNumber} exceeds total seats {showtimeTask.Theater.totalSeats} for the theater.", nameof(createReservationDTO.SeatNumber));
            var reservation = new Data.Entities.Reservation
            {
                SeatNumber = createReservationDTO.SeatNumber,
                CreatedAt = DateTime.UtcNow,
                SecretCode = GenerateSecretCode(),
                ShowTimeId = createReservationDTO.ShowtimeId,
                AppUserId = appUserId
            };
            await _unitOfWork.Repository<Data.Entities.Reservation>().AddAsync(reservation);
            await _unitOfWork.SaveChangesAsync();
            var spec = new GetReservationByIdSpec(reservation.Id);
            var createdReservation = await _unitOfWork.Repository<Data.Entities.Reservation>()
               .GetSingleAsync(spec);
            return _mapper.Map<ReservationDetailDTO>(createdReservation);

        }

        public async Task<IEnumerable<int>> GetAvailableSeatsAsync(int showtimeId)
        {
            if (showtimeId <= 0)
                throw new ArgumentException("Showtime ID must be a positive integer", nameof(showtimeId));
            var spec = new GetReservationsByShowtimeSpec(showtimeId);
            var reservationsTask = await _unitOfWork.Repository<Data.Entities.Reservation>()
                .GetAsync(spec);
            var bookedSeats = reservationsTask.Select(r => r.SeatNumber).ToList();
            return bookedSeats;
        }

        public async Task<IEnumerable<int>> GetBookedSeatsAsync(int showtimeId)
        {
            if (showtimeId <= 0)
                throw new ArgumentException("Showtime ID must be a positive integer", nameof(showtimeId));
            var spec = new GetAvailableSeatsSpec(showtimeId);
            var showtime = await _unitOfWork.Repository<Data.Entities.Showtime>()
                .GetSingleAsync(spec);
            if (showtime == null)
                throw new InvalidOperationException($"Showtime with ID {showtimeId} does not exist.");
            var bookedSeats = await GetBookedSeatsAsync(showtimeId);
            var availableSeats = new HashSet<int>(bookedSeats);
            var allSeats = Enumerable.Range(1, showtime.Theater.totalSeats)
                .Where(seat => !availableSeats.Contains(seat))
                .ToList();
            return allSeats;

        }

        public async Task<ReservationDetailDTO> GetReservationByIdAsync(int reservationId)
        {
            if (reservationId <= 0)
                throw new ArgumentException("Reservation ID must be a positive integer", nameof(reservationId));
            var spec = new GetReservationByIdSpec(reservationId);
            var reservationTask = await _unitOfWork.Repository<Data.Entities.Reservation>()
                .GetSingleAsync(spec);
            if (reservationTask == null)
                throw new InvalidOperationException($"Reservation with ID {reservationId} does not exist.");
            return reservationTask == null ? null : _mapper.Map<ReservationDetailDTO>(reservationTask);
        }

        public async Task<PaginatedResultDTO<ReservationDTO>> GetReservationsByUserAsync(string appUserId, int pageNumber, int pageSize)
        {
            ValidatePagination(pageNumber, pageSize);
            if (string.IsNullOrWhiteSpace(appUserId))
                throw new ArgumentException("App user ID cannot be null or empty", nameof(appUserId));
            var spec = new GetUserReservationsSpec(appUserId, pageNumber, pageSize);
            var reservationsTask = await _unitOfWork.Repository<Data.Entities.Reservation>().GetAsync(spec);
            var countSpec = new GetReservationCountByUserSpec(appUserId);
            var totalItemsTask = await _unitOfWork.Repository<Data.Entities.Reservation>().CountAsync(countSpec);
            var mappedReservations = _mapper.Map<IEnumerable<ReservationDTO>>(reservationsTask);
            return new PaginatedResultDTO<ReservationDTO>
            {
                Items = mappedReservations.ToList(),
                TotalCount = totalItemsTask,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<bool> ReservationExistsAsync(int reservationId)
        {
            if (reservationId <= 0)
                throw new ArgumentException("Reservation ID must be a positive integer", nameof(reservationId));
            var reservation = await _unitOfWork.Repository<Data.Entities.Reservation>()
                .GetByIdAsync(reservationId);
            return reservation != null;
        }

        public async Task<bool> UpdateReservationSeatAsync(int reservationId, int newSeatNumber, string appUserId)
        {
            if (reservationId <= 0)
                throw new ArgumentException("Reservation ID must be a positive integer", nameof(reservationId));
            if (newSeatNumber <= 0)
                throw new ArgumentException("New seat number must be a positive integer", nameof(newSeatNumber));
            if (string.IsNullOrWhiteSpace(appUserId))
                throw new ArgumentException("App user ID cannot be null or empty", nameof(appUserId));
            var reservation = await _unitOfWork.Repository<Data.Entities.Reservation>()
                 .GetByIdAsync(reservationId);
            if (reservation == null)
                throw new InvalidOperationException($"Reservation with ID {reservationId} does not exist.");
            if (reservation.AppUserId != appUserId)
                throw new UnauthorizedAccessException("You are not authorized to update this reservation.");
            var seatAvailability = await CheckSeatAvailabilityAsync(reservation.ShowTimeId.Value, newSeatNumber);
            if (!seatAvailability.IsAvailable)
                throw new InvalidOperationException($"Seat {newSeatNumber} for showtime {reservation.ShowTimeId} is already booked.");
            reservation.SeatNumber = newSeatNumber;
            _unitOfWork.Repository<Data.Entities.Reservation>().Update(reservation);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<(bool IsValid, ReservationDetailDTO? Reservation)> ValidateReservationAsync(int reservationId, string secretCode)
        {
            if (reservationId <= 0)
                throw new ArgumentException("Reservation ID must be a positive integer", nameof(reservationId));
            if (string.IsNullOrWhiteSpace(secretCode))
                throw new ArgumentException("Secret code cannot be null or empty", nameof(secretCode));
            var spec = new VerifyReservationSpec(reservationId, secretCode);
            var reservationTask = await _unitOfWork.Repository<Data.Entities.Reservation>()
                .GetSingleAsync(spec);
            if (reservationTask == null)
                return (false, null);
            var mappedReservation = _mapper.Map<ReservationDetailDTO>(reservationTask);
            return (true, mappedReservation);
        }
        private static void ValidatePagination(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Page number must be at least 1", nameof(pageNumber));

            if (pageSize < 1 || pageSize > 100)
                throw new ArgumentException("Page size must be between 1 and 100", nameof(pageSize));
        }
        private static string GenerateSecretCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Range(0, 8)
                .Select(_ => chars[random.Next(chars.Length)])
                .ToArray());
        }
    }
}
