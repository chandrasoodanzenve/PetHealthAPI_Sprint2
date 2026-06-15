using Moq;
using PetHealthAPI.Models;
using PetHealthAPI.Repositories;
using PetHealthAPI.Services;
using Xunit;

namespace PetHealthAPI.Tests
{
    public class PetServiceTests
    {
        [Fact]
        public async Task GetAllPetsAsync_ShouldReturnAllPets()
        {
            var mockRepo = new Mock<IPetRepository>();
            
            var fakePets = new List<Pet>
            {
                new Pet { Id = 1, Name = "Simba", Breed = "Dog", HealthScore = 90 },
                new Pet { Id = 2, Name = "Tiger", Breed = "Cat", HealthScore = 85 }
            };
            mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(fakePets);

            var service = new PetService(mockRepo.Object);

            var result = await service.GetAllPetsAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("Simba", result.First().Name);
        }
    }
}