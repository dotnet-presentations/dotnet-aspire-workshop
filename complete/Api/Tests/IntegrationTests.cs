using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Threading.Tasks;

namespace Api.Tests
{
    [TestClass]
    public class IntegrationTests
    {
        private static readonly HttpClient client = new HttpClient();

        [TestMethod]
        public async Task TestGetZonesAsync()
        {
            var response = await client.GetAsync("https://localhost:7032/zones");
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(responseBody);
            Assert.IsTrue(responseBody.Contains("ZoneId"));
        }

        [TestMethod]
        public async Task TestGetForecastByZoneAsync()
        {
            var response = await client.GetAsync("https://localhost:7032/forecast/AKZ318");
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(responseBody);
            Assert.IsTrue(responseBody.Contains("DetailedForecast"));
        }
    }
}
