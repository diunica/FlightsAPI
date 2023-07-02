using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FlightsAPI.Tests
{
    public class FlightsControllerTest : IntegrationTestBuilder
    {
        [Fact]
        public async Task GetFlightsSuccess()
        {
            const string origen = "MZL";
            const string destino = "BOG";
            const string badge = "USD";
            var payload = await this.TestClient.GetAsync($"/flights/{origen}/{destino}/{badge}");
            payload.EnsureSuccessStatusCode();
            var payloadData = await payload.Content.ReadAsStringAsync();
            var route = System.Text.Json.JsonSerializer
                .Deserialize<List<Dictionary<string, object>>>(payloadData);
            var flightCost = float.Parse(route?.FirstOrDefault()?["Price"].ToString(),CultureInfo.InvariantCulture);
            Assert.Equal((double)400.0, Convert.ToDouble(flightCost));
        }
    }
}
