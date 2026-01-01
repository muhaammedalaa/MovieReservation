using AutoMapper;
using MovieReservation.Data.Contracts;
using MovieReservation.Data.Dtos;
using MovieReservation.Data.Entities;
using MovieReservation.Data.Service.Contract;
using MovieReservation.Data.Specification.Showtime_Specifications;
using MovieEntity = MovieReservation.Data.Entities.Movie;
using ShowtimeEntity = MovieReservation.Data.Entities.Showtime;


namespace MovieReservation.Service.Services.Showtime
{
    public class ShowtimeService : IShowtimeService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;

        public ShowtimeService(IMapper mapper, IUnitOfWork unitOfWork, ICacheService cacheService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
        }
        public async Task<ShowtimeDTO> CreateShowtimeAsync(CreateShowtimeDTO createDTO)
        {
            if (createDTO == null)
                throw new ArgumentNullException(nameof(createDTO));
            if (createDTO.MovieId <= 0)
                throw new ArgumentException("Movie ID must be greater than 0", nameof(createDTO.MovieId));
            if (createDTO.TheaterId <= 0)
                throw new ArgumentException("Theater ID must be greater than 0", nameof(createDTO.TheaterId));
            if (createDTO.Price <= 0)
                throw new ArgumentException("Price must be greater than 0", nameof(createDTO.Price));
            if (createDTO.StartDate <= DateTime.UtcNow)
                throw new ArgumentException("Start date must be in the future", nameof(createDTO.StartDate));
            var movie = await _unitOfWork.Repository<MovieEntity>().GetByIdAsync(createDTO.MovieId);
            if (movie == null)
                throw new InvalidOperationException($"Movie with ID {createDTO.MovieId} not found");
            var theater = await _unitOfWork.Repository<Theater>().GetByIdAsync(createDTO.TheaterId);
            if (theater == null)
                throw new InvalidOperationException($"Theater with ID {createDTO.TheaterId} not found");
            var showtime = _mapper.Map<ShowtimeEntity>(createDTO);
            await _unitOfWork.Repository<ShowtimeEntity>().AddAsync(showtime);
            await _unitOfWork.SaveChangesAsync();
            await _cacheService.RemoveByPatternAsync("showtimes:*"); // Invalidate all showtime-related caches
            return _mapper.Map<ShowtimeDTO>(showtime);

        }

        public async Task<bool> DeleteShowtimeAsync(int showtimeId)
        {
            if (showtimeId <= 0)
                throw new ArgumentException("Showtime ID must be greater than 0", nameof(showtimeId));
            var showtime = await _unitOfWork.Repository<ShowtimeEntity>().GetByIdAsync(showtimeId);
            if (showtime == null)
                return false;
            var spec = new GetShowtimeByIdSpec(showtimeId);
            var showtimeWithReservations = await _unitOfWork.Repository<ShowtimeEntity>().GetSingleAsync(spec);
            if (showtimeWithReservations?.Reservations?.Any() == null)
                throw new InvalidOperationException("Cannot delete showtime with existing reservations");
            _unitOfWork.Repository<ShowtimeEntity>().Delete(showtime);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<PaginatedResultDTO<ShowtimeDTO>> GetAllShowtimesAsync(int pageNumber = 1, int pageSize = 10)
        {
            ValidatePagination(pageNumber, pageSize);
            var cacheKey = $"showtimes:page:{pageNumber}:size:{pageSize}";
            var cached = await _cacheService.GetAsync<PaginatedResultDTO<ShowtimeDTO>>(cacheKey);
            if (cached != null)
                return cached;
            var spec = new GetAllShowtimesSpec(pageSize, pageNumber);
            var showtimes = await _unitOfWork.Repository<ShowtimeEntity>().GetAsync(spec);
            var countspec = new GetShowtimeCountSpec();
            var totalCount = await _unitOfWork.Repository<ShowtimeEntity>().CountAsync(countspec);
            var Mappedshowtime = _mapper.Map<IEnumerable<ShowtimeDTO>>(showtimes);
            var result = new PaginatedResultDTO<ShowtimeDTO>
            {
                Items = Mappedshowtime.ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
            return result;
        }

        public async Task<IEnumerable<int>> GetReservedSeatsAsync(int showtimeId)
        {
            if (showtimeId <= 0)
                throw new ArgumentException("Showtime ID must be greater than 0", nameof(showtimeId));
            var spec = new GetShowtimeByIdSpec(showtimeId);
            var showtime = await _unitOfWork.Repository<ShowtimeEntity>().GetSingleAsync(spec);
            if (showtime == null)
                return Enumerable.Empty<int>();
            return showtime.Reservations?.Select(r => r.SeatNumber).ToList() ?? Enumerable.Empty<int>();
        }

        public async Task<ShowtimeAvailabilityDTO> GetShowtimeAvailabilityAsync(int showtimeId)
        {
            if (showtimeId <= 0)
                throw new ArgumentException("Showtime ID must be greater than 0", nameof(showtimeId));
            var spec = new GetShowtimeByIdSpec(showtimeId);
            var showtime = await _unitOfWork.Repository<ShowtimeEntity>().GetSingleAsync(spec);
            if (showtime == null)
                return null;
            var reservedCount = showtime.Reservations?.Count ?? 0;
            var availableSeats = showtime.Theater.totalSeats - reservedCount;
            var occupancyPercentag = (decimal)reservedCount / showtime.Theater.totalSeats * 100;
            return new ShowtimeAvailabilityDTO
            {
                ShowtimeId = showtimeId,
                AvailableSeats = availableSeats,
                ReservedSeats = reservedCount,
                TotalSeats = showtime.Theater.totalSeats,
                OccupancyPercentage = Math.Round(occupancyPercentag, 2),
                IsAvailable = availableSeats > 0
            };

        }

        public async Task<ShowtimeDetailDTO> GetShowtimeDetailsAsync(int showtimeId)
        {
            if (showtimeId < 0)
                throw new ArgumentException("Showtime ID must be greater than 0", nameof(showtimeId));
            var spec = new GetShowtimeByIdSpec(showtimeId);
            var showtime = await _unitOfWork.Repository<ShowtimeEntity>().GetSingleAsync(spec);
            if (showtime == null)
                return null;
            var detailDto = _mapper.Map<ShowtimeDetailDTO>(showtime);
            var reservedCount = showtime.Reservations?.Count ?? 0;
            detailDto.ReservedSeats = reservedCount;
            detailDto.AvailableSeats = showtime.Theater.totalSeats - reservedCount;
            detailDto.ReservedSeatNumbers = showtime.Reservations?.Select(r => r.SeatNumber).ToList() ?? new();
            return detailDto;
        }

        public async Task<IEnumerable<ShowtimeDTO>> GetShowtimesByMovieAndDateAsync(int movieid, DateTime date)
        {
            if (movieid <= 0)
                throw new ArgumentException("Movie ID must be greater than 0", nameof(movieid));
            var spec = new GetShowtimesForMovieSpec(movieid, date);
            var showtimes = await _unitOfWork.Repository<ShowtimeEntity>().GetAsync(spec);
            return _mapper.Map<IEnumerable<ShowtimeDTO>>(showtimes);
        }

        public async Task<PaginatedResultDTO<ShowtimeDTO>> GetShowtimesByMovieAsync(int movieId, int pageNumber = 1, int pageSize = 10)
        {
            if (movieId <= 0)
                throw new ArgumentException("Movie ID must be greater than 0", nameof(movieId));
            ValidatePagination(pageNumber, pageSize);
            var movie = await _unitOfWork.Repository<MovieEntity>().GetByIdAsync(movieId);
            if (movie == null)
                throw new InvalidOperationException($"Movie with ID {movieId} not found");
            var spec = new GetShowtimesWithDetailsSpec(movieId, pageNumber, pageSize);
            var showtime = await _unitOfWork.Repository<ShowtimeEntity>().GetAsync(spec);
            var allShowtimes = await _unitOfWork.Repository<ShowtimeEntity>().GetAllAsync();
            var totalCount = allShowtimes
                .Count(s => s.MovieId == movieId && s.StartDate >= DateTime.UtcNow);
            var showtimeDtos = _mapper.Map<IEnumerable<ShowtimeDTO>>(showtime);
            return new PaginatedResultDTO<ShowtimeDTO>
            {
                Items = showtimeDtos.ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

        }

        public async Task<PaginatedResultDTO<ShowtimeDTO>> GetShowtimesByTheaterAsync(int theaterId, int pageNumber = 1, int pageSize = 10)
        {
            ValidatePagination(pageNumber, pageSize);
            if (theaterId < 0) throw new ArgumentOutOfRangeException("Theater ID must be greater than 0", nameof(theaterId));
            var theater = _unitOfWork.Repository<Theater>().GetByIdAsync(theaterId);
            if (theater == null)
                throw new ArgumentNullException($"Theater with ID {theaterId} not found");
            var spec = new GetShowtimesByTheaterSpec(theaterId, pageNumber, pageSize);
            var showtimes = await _unitOfWork.Repository<ShowtimeEntity>().GetAsync(spec);
            var allShowtimes = await _unitOfWork.Repository<ShowtimeEntity>().GetAllAsync();
            var totalCount = ((List<ShowtimeEntity>)allShowtimes)
                .Count(s => s.TheaterId == theaterId && s.StartDate >= DateTime.UtcNow);
            var showtimeDtos = _mapper.Map<IEnumerable<ShowtimeDTO>>(showtimes);
            return new PaginatedResultDTO<ShowtimeDTO>
            {
                Items = showtimeDtos.ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<ShowtimeDTO>> GetUpcomingShowtimesAsync(int daysAhead = 7)
        {
            if (daysAhead <= 0)
                throw new ArgumentException("Days ahead must be greater than 0", nameof(daysAhead));
            var allShowtimes = await _unitOfWork.Repository<ShowtimeEntity>().GetAllAsync();
            var now = DateTime.UtcNow;
            var futureDate = now.AddDays(daysAhead);
            var upcomingShowtimes = ((List<ShowtimeEntity>)allShowtimes)
                .Where(s => s.StartDate >= now && s.StartDate <= futureDate)
                .OrderBy(s => s.StartDate)
                .ToList();
            return _mapper.Map<IEnumerable<ShowtimeDTO>>(upcomingShowtimes);
        }

        public async Task<bool> ShowtimeExistsAsync(int showtimeId)
        {
            if (showtimeId <= 0)
                return false;
            var showtime = await _unitOfWork.Repository<ShowtimeEntity>().GetByIdAsync(showtimeId);
            return showtime != null;
        }

        public async Task<bool> UpdateShowtimeAsync(int showtimeId, CreateShowtimeDTO updateDTO)
        {
            if (updateDTO == null)
                throw new ArgumentNullException(nameof(updateDTO));
            if (showtimeId <= 0)
                throw new ArgumentException("Showtime ID must be greater than 0", nameof(showtimeId));
            var showtime = await _unitOfWork.Repository<ShowtimeEntity>().GetByIdAsync(showtimeId);
            if (showtime == null)
                return false;
            showtime.Price = updateDTO.Price;
            showtime.StartDate = updateDTO.StartDate;
            showtime.TheaterId = updateDTO.TheaterId;
            _unitOfWork.Repository<ShowtimeEntity>().Update(showtime);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        private static void ValidatePagination(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Page number must be at least 1", nameof(pageNumber));

            if (pageSize < 1 || pageSize > 100)
                throw new ArgumentException("Page size must be between 1 and 100", nameof(pageSize));
        }

        public async Task<bool> IsSeatAvailableAsync(int showtimeId, int seatNumber)
        {
            if (showtimeId <= 0)
                throw new ArgumentException("Showtime ID must be greater than 0", nameof(showtimeId));
            if (seatNumber <= 0)
                throw new ArgumentException("Seat number must be greater than 0", nameof(seatNumber));
            var reservedSeats = await GetReservedSeatsAsync(showtimeId);
            return !reservedSeats.Contains(seatNumber);
        }
    }
}
