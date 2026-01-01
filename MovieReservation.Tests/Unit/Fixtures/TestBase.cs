using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using MovieReservation.Data.Contracts;
using MovieReservation.Data.Service.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Tests.Unit.Fixtures
{
    public class TestBase
    {
        protected readonly Mock<IUnitOfWork> MockUnitOfWork;
        protected readonly Mock<IMapper> MockMapper;
        protected readonly Mock<ICacheService> MockCacheService;


        protected TestBase()
        {
            MockUnitOfWork = new Mock<IUnitOfWork>();
            MockMapper = new Mock<IMapper>();
            MockCacheService = new Mock<ICacheService>();
        }
        protected Mock<ILogger<T>> CreateMockLogger<T>() where T : class
        {
            return new Mock<ILogger<T>>();
        }
        protected Mock<IGenericRepository<T>> CreateMockRepository<T>() where T : class
        {
            return new Mock<IGenericRepository<T>>();
        }
    }
}
