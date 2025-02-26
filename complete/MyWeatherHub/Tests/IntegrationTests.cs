using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Threading.Tasks;

namespace MyWeatherHub.Tests
{
    [TestClass]
    public class IntegrationTests
    {
        private static readonly HttpClient client = new HttpClient();

        [TestMethod]
        public async Task TestHomePageAsync()
        {
            var response = await client.GetAsync("https://localhost:7274/");
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(responseBody);
            Assert.IsTrue(responseBody.Contains("My Weather Hub"));
        }

        [TestMethod]
        public async Task TestGetForecastByZoneAsync()
        {
            var response = await client.GetAsync("https://localhost:7274/forecast/AKZ318");
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(responseBody);
            Assert.IsTrue(responseBody.Contains("DetailedForecast"));
        }
    }
}
