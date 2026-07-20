using Moq;
using PetHealthAPI.Models;
using PetHealthAPI.Services;
using PetHealthAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; 
using Xunit;

namespace PetHealthAPI.Tests
{
    public class IntelligenceServiceTests
    {
        [Fact]
        public async Task ComputeIntelligence_ShouldIdentifyConflict_WhenHealthIsLow()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_Intelligence_DB")
                .Options;

            using var context = new AppDbContext(options);
            context.Pets.Add(new Pet { Id = 10, Name = "TestPet", HealthScore = 30 });
            await context.SaveChangesAsync();

            var mockLogger = new Mock<ILogger<CustomerIntelligenceService>>();
            var service = new CustomerIntelligenceService(context, mockLogger.Object);
            var result = await service.GetUnifiedViewAsync(1);
            Assert.Equal("At-Risk User", result.UserSegment);
            Assert.NotEmpty(result.ConflictingSignals);
        }
    }
}