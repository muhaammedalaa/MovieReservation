using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using MovieReservation.Data.Contracts;
using MovieReservation.Service.Services.Showtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Tests.Unit.Services
{
    public class ReservationServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<ShowtimeService>> _mockLogger;

        public ReservationServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<ShowtimeService>>();
        }
    }
}
