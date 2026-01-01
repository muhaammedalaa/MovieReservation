using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MovieReservation.Data.Contracts;
using MovieReservation.Data.Dtos;
using MovieReservation.Data.Entities;
using MovieReservation.Data.Service.Contract;
using MovieReservation.Data.Specification;
using MovieReservation.Service.Services.Showtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MovieReservation.Tests.Unit.Services
{
    /// <summary>
    /// Comprehensive unit tests for ShowtimeService
    /// </summary>
    public class ShowtimeServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<ShowtimeService>> _mockLogger;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly ShowtimeService _service;

        public ShowtimeServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<ShowtimeService>>();
            _mockCacheService = new Mock<ICacheService>();

            _service = new ShowtimeService(
                _mockMapper.Object,
                _mockUnitOfWork.Object,
                _mockCacheService.Object);
        }

        #region GetAllShowtimesAsync Tests

        [Fact]
        public async Task GetAllShowtimesAsync_WithValidPagination_ReturnsPaginatedShowtimes()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var showtimes = new List<Showtime>
            {
                new Showtime { Id = 1, Price = 12.99m, StartDate = DateTime.UtcNow.AddDays(1) },
                new Showtime { Id = 2, Price = 14.99m, StartDate = DateTime.UtcNow.AddDays(2) }
            };

            var showtimeDtos = new List<ShowtimeDTO>
            {
                new ShowtimeDTO { Id = 1, Price = 12.99m },
                new ShowtimeDTO { Id = 2, Price = 14.99m }
            };

            // Cache miss
            _mockCacheService
                .Setup(c => c.GetAsync<PaginatedResultDTO<ShowtimeDTO>>(It.IsAny<string>()))
                .ReturnsAsync((PaginatedResultDTO<ShowtimeDTO>?)null);

            var mockShowtimeRepository = new Mock<IGenericRepository<Showtime>>();
            mockShowtimeRepository
                .Setup(r => r.GetAsync(It.IsAny<ISpecification<Showtime>>()))
                .ReturnsAsync(showtimes);
            mockShowtimeRepository
                .Setup(r => r.CountAsync(It.IsAny<ISpecification<Showtime>>()))
                .ReturnsAsync(2);

            _mockUnitOfWork
                .Setup(u => u.Repository<Showtime>())
                .Returns(mockShowtimeRepository.Object);

            _mockMapper
                .Setup(m => m.Map<IEnumerable<ShowtimeDTO>>(It.IsAny<IEnumerable<Showtime>>()))
                .Returns(showtimeDtos);

            // Act
            var result = await _service.GetAllShowtimesAsync(pageNumber, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.PageNumber.Should().Be(pageNumber);
            result.PageSize.Should().Be(pageSize);

            // Verify cache was called
            _mockCacheService.Verify(c => c.GetAsync<PaginatedResultDTO<ShowtimeDTO>>(It.IsAny<string>()), Times.Once);
            _mockCacheService.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<PaginatedResultDTO<ShowtimeDTO>>(), It.IsAny<TimeSpan?>()), Times.Once);
        }

        [Fact]
        public async Task GetAllShowtimesAsync_WithInvalidPageNumber_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetAllShowtimesAsync(pageNumber: 0, pageSize: 10));
        }

        [Fact]
        public async Task GetAllShowtimesAsync_WithInvalidPageSize_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetAllShowtimesAsync(pageNumber: 1, pageSize: 101));
        }

        #endregion

        #region GetShowtimeDetailsAsync Tests

        [Fact]
        public async Task GetShowtimeDetailsAsync_WithValidId_ReturnsShowtimeDetail()
        {
            // Arrange
            int showtimeId = 1;
            var showtime = new Showtime
            {
                Id = showtimeId,
                Price = 12.99m,
                StartDate = DateTime.UtcNow.AddDays(1),
                MovieId = 1,
                TheaterId = 1,
                Movie = new Movie { Id = 1, Titel = "Test Movie" },
                Theater = new Theater { Id = 1, Name = "Test Theater", totalSeats = 100 },
                Reservations = new List<Reservation>()
            };

            _mockCacheService
                .Setup(c => c.GetAsync<ShowtimeDetailDTO>(It.IsAny<string>()))
                .ReturnsAsync((ShowtimeDetailDTO?)null);

            var mockRepository = new Mock<IGenericRepository<Showtime>>();
            mockRepository
                .Setup(r => r.GetSingleAsync(It.IsAny<ISpecification<Showtime>>()))
                .ReturnsAsync(showtime);

            _mockUnitOfWork
                .Setup(u => u.Repository<Showtime>())
                .Returns(mockRepository.Object);

            _mockMapper
                .Setup(m => m.Map<ShowtimeDetailDTO>(It.IsAny<Showtime>()))
                .Returns(new ShowtimeDetailDTO { Id = showtimeId, Price = 12.99m, AvailableSeats = 100 });

            // Act
            var result = await _service.GetShowtimeDetailsAsync(showtimeId);

            // Assert
            result.Should().NotBeNull();
            result?.Id.Should().Be(showtimeId);
            result?.Price.Should().Be(12.99m);
            result?.AvailableSeats.Should().Be(100);
        }

        [Fact]
        public async Task GetShowtimeDetailsAsync_WithInvalidId_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetShowtimeDetailsAsync(showtimeId: -1));
        }

        [Fact]
        public async Task GetShowtimeDetailsAsync_WithNonExistentId_ReturnsNull()
        {
            // Arrange
            var mockRepository = new Mock<IGenericRepository<Showtime>>();
            mockRepository
                .Setup(r => r.GetSingleAsync(It.IsAny<ISpecification<Showtime>>()))
                .ReturnsAsync((Showtime?)null);

            _mockUnitOfWork
                .Setup(u => u.Repository<Showtime>())
                .Returns(mockRepository.Object);

            // Act
            var result = await _service.GetShowtimeDetailsAsync(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateShowtimeAsync Tests

        [Fact]
        public async Task CreateShowtimeAsync_WithValidData_CreatesShowtime()
        {
            // Arrange
            var createDTO = new CreateShowtimeDTO
            {
                MovieId = 1,
                TheaterId = 1,
                Price = 12.99m,
                StartDate = DateTime.UtcNow.AddDays(1)
            };

            var mockMovieRepo = new Mock<IGenericRepository<Movie>>();
            var mockTheaterRepo = new Mock<IGenericRepository<Theater>>();
            var mockShowtimeRepo = new Mock<IGenericRepository<Showtime>>();

            mockMovieRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new Movie { Id = 1, Titel = "Test Movie" });

            mockTheaterRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new Theater { Id = 1, Name = "Test Theater" });

            _mockUnitOfWork
                .Setup(u => u.Repository<Movie>())
                .Returns(mockMovieRepo.Object);

            _mockUnitOfWork
                .Setup(u => u.Repository<Theater>())
                .Returns(mockTheaterRepo.Object);

            _mockUnitOfWork
                .Setup(u => u.Repository<Showtime>())
                .Returns(mockShowtimeRepo.Object);

            _mockMapper
                .Setup(m => m.Map<Showtime>(createDTO))
                .Returns(new Showtime { Id = 1, Price = 12.99m, StartDate = createDTO.StartDate });

            _mockMapper
                .Setup(m => m.Map<ShowtimeDTO>(It.IsAny<Showtime>()))
                .Returns(new ShowtimeDTO { Id = 1, Price = 12.99m });

            // Act
            var result = await _service.CreateShowtimeAsync(createDTO);

            // Assert
            result.Should().NotBeNull();
            result.Price.Should().Be(12.99m);
            mockShowtimeRepo.Verify(r => r.AddAsync(It.IsAny<Showtime>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
            _mockCacheService.Verify(c => c.RemoveByPatternAsync(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Theory]
        [InlineData(-1, 1, 12.99, "2025-02-01")]  // Invalid movie ID
        [InlineData(1, -1, 12.99, "2025-02-01")]  // Invalid theater ID
        [InlineData(1, 1, -10, "2025-02-01")]     // Invalid price
        public async Task CreateShowtimeAsync_WithInvalidData_ThrowsArgumentException(
            int movieId, int theaterId, decimal price, string startDate)
        {
            // Arrange
            var createDTO = new CreateShowtimeDTO
            {
                MovieId = movieId,
                TheaterId = theaterId,
                Price = price,
                StartDate = DateTime.Parse(startDate)
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateShowtimeAsync(createDTO));
        }

        #endregion

        #region DeleteShowtimeAsync Tests

        [Fact]
        public async Task DeleteShowtimeAsync_WithValidId_DeletesShowtime()
        {
            // Arrange
            int showtimeId = 1;
            var showtime = new Showtime
            {
                Id = showtimeId,
                Reservations = new List<Reservation>()  // No reservations
            };

            var mockRepository = new Mock<IGenericRepository<Showtime>>();
            mockRepository
                .Setup(r => r.GetByIdAsync(showtimeId))
                .ReturnsAsync(showtime);

            mockRepository
                .Setup(r => r.GetSingleAsync(It.IsAny<ISpecification<Showtime>>()))
                .ReturnsAsync(showtime);

            _mockUnitOfWork
                .Setup(u => u.Repository<Showtime>())
                .Returns(mockRepository.Object);

            // Act
            var result = await _service.DeleteShowtimeAsync(showtimeId);

            // Assert
            result.Should().BeTrue();
            mockRepository.Verify(r => r.Delete(showtime), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteShowtimeAsync_WithNonExistentId_ReturnsFalse()
        {
            // Arrange
            var mockRepository = new Mock<IGenericRepository<Showtime>>();
            mockRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Showtime?)null);

            _mockUnitOfWork
                .Setup(u => u.Repository<Showtime>())
                .Returns(mockRepository.Object);

            // Act
            var result = await _service.DeleteShowtimeAsync(999);

            // Assert
            result.Should().BeFalse();
            mockRepository.Verify(r => r.Delete(It.IsAny<Showtime>()), Times.Never);
        }

        #endregion

        #region IsSeatAvailableAsync Tests

        [Fact]
        public async Task IsSeatAvailableAsync_WithAvailableSeat_ReturnsTrue()
        {
            // Arrange
            int showtimeId = 1;
            int seatNumber = 5;

            var reservations = new List<Reservation>
            {
                new Reservation { Id = 1, SeatNumber = 10 },
                new Reservation { Id = 2, SeatNumber = 15 }
            };

            var showtime = new Showtime
            {
                Id = showtimeId,
                Theater = new Theater { totalSeats = 100 }, // Assuming a theater with total seats
                Reservations = reservations
            };

            var mockShowtimeRepository = new Mock<IGenericRepository<Showtime>>();
            mockShowtimeRepository
                .Setup(r => r.GetSingleAsync(It.IsAny<ISpecification<Showtime>>()))
                .ReturnsAsync(showtime);

            _mockUnitOfWork
                .Setup(u => u.Repository<Showtime>())
                .Returns(mockShowtimeRepository.Object);

            // Act
            var result = await _service.IsSeatAvailableAsync(showtimeId, seatNumber);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsSeatAvailableAsync_WithReservedSeat_ReturnsFalse()
        {
            // Arrange
            int showtimeId = 1;
            int seatNumber = 10;

            var reservations = new List<Reservation>
            {
                new Reservation { Id = 1, SeatNumber = 10, ShowTimeId = showtimeId }
            };

            var showtime = new Showtime
            {
                Id = showtimeId,
                Theater = new Theater { totalSeats = 100 }, // Assuming a theater with total seats
                Reservations = reservations
            };

            var mockShowtimeRepository = new Mock<IGenericRepository<Showtime>>();
            mockShowtimeRepository
                .Setup(r => r.GetSingleAsync(It.IsAny<ISpecification<Showtime>>()))
                .ReturnsAsync(showtime);

            _mockUnitOfWork
                .Setup(u => u.Repository<Showtime>())
                .Returns(mockShowtimeRepository.Object);

            // Act
            var result = await _service.IsSeatAvailableAsync(showtimeId, seatNumber);

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
}