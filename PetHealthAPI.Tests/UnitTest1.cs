using Moq;
using PetHealthAPI.Models;
using PetHealthAPI.Repositories;
using PetHealthAPI.Services;
using Microsoft.Extensions.Caching.Distributed; 
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics.Metrics;

namespace PetHealthAPI.Tests
{
    public class PetServiceTests
    {
        [Fact]
        public async Task GetAllPetsAsync_ShouldReturnAllPets()
        {
           
            var mockRepo = new Mock<IPetRepository>();
            var mockCache = new Mock<IDistributedCache>(); 
            var mockMeterFactory = new Mock<IMeterFactory>(); 
            var fakePets = new List<Pet>
            {
                new Pet { Id = 1, Name = "Simba", Breed = "Dog", HealthScore = 90 },
                new Pet { Id = 2, Name = "Tiger", Breed = "Cat", HealthScore = 85 }
            };
            mockRepo.Setup(repo => repo.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ReturnsAsync((fakePets, 2));
            var service = new PetService(mockRepo.Object, mockCache.Object, mockMeterFactory.Object);
            var result = await service.GetAllPetsAsync(1, 10);
            Assert.NotNull(result.Pets);
            Assert.Equal(2, result.Pets.Count());
            Assert.Equal("Simba", result.Pets.First().Name);
        }
    }
}