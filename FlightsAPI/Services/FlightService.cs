using FlightsAPI.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FlightsAPI.Services
{
    public class FlightService 
    {
        #region Defaults, Configuration & Constants
        
        private readonly string _flightAddress = "https://recruiting-api.newshore.es/api/flights/";
        private readonly int _cacheExpiration = 120;
        private const string flightsCacheKey = "flights_api_data";

        #endregion

        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<FlightService> _logger;

        public FlightService(IMemoryCache cache,
                         IConfiguration configuration,
                         ILogger<FlightService> logger)
        {
            this._cacheExpiration = Convert.ToInt32(configuration["FlightsCacheExpirationInSeconds"]);
            this._flightAddress = configuration["FlightsAPIEndpoint"];
            this._memoryCache = cache;
            this._logger = logger;
        }

        public async Task<List<FlightProvider>> GetFlights()
        {
            if (!_memoryCache.TryGetValue(flightsCacheKey, out List<FlightProvider> cacheValue))
            {
                var flights = await RetrieveFlightsFromServer();
                var cacheEntryOpts = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_cacheExpiration));
                _memoryCache.Set(flightsCacheKey, flights, cacheEntryOpts);
                return flights;
            }
            return cacheValue;
        }

        #region Private

        private async Task<List<FlightProvider>> RetrieveFlightsFromServer()
        {
            HttpClient flightsClient = InitializeHttpClient();
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, "2");
            HttpResponseMessage response = await flightsClient.SendAsync(message);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }
            else
            {
                return JsonConvert.DeserializeObject<List<FlightProvider>>(await response.Content.ReadAsStringAsync());
            }
        }

        private HttpClient InitializeHttpClient()
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_flightAddress);
            return httpClient;
        }

        #endregion
    }
}
