using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;
using Microsoft.VisualStudio.TestPlatform.TestHost; 

namespace PetHealthAPI.Tests
{
    public class PetApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        public PetApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }
        [Fact]
        public async Task Get_Pets_Endpoint_Returns_Unauthorized_Without_Token()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/Pets");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}