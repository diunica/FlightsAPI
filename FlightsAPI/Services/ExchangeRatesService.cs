using FlightsAPI.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FlightsAPI.Services
{
    public class ExchangeRatesService
    {
        #region Defaults, Configuration & Constants

        private readonly string _exchangeRatesAddress = "https://open.er-api.com/v6/latest/";
        private readonly int _cacheExpiration = 120;
        private const string exchangeRatesCacheKey = "rates_api_data";

        #endregion

        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<ExchangeRatesService> _logger;

        public ExchangeRatesService(IMemoryCache cache,
                             IConfiguration configuration,
                             ILogger<ExchangeRatesService> logger)
        {
            this._cacheExpiration = Convert.ToInt32(configuration["FlightsCacheExpirationInSeconds"]);
            this._exchangeRatesAddress = configuration["ExchangeRatesAPIEndpoint"];
            this._memoryCache = cache;
            this._logger = logger;
        }

        public async Task<Rate> GetRates()
        {
            if (!_memoryCache.TryGetValue(exchangeRatesCacheKey, out Rate cacheValue))
            {
                var rates = await RetrieveExchageRatesFromServer();
                var cacheEntryOpts = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_cacheExpiration));
                _memoryCache.Set(exchangeRatesCacheKey, rates, cacheEntryOpts);
                return rates;
            }
            return cacheValue;
        }

        #region Private

        private async Task<Rate> RetrieveExchageRatesFromServer()
        {
            HttpClient flightsClient = InitializeHttpClient();
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, "USD");
            HttpResponseMessage response = await flightsClient.SendAsync(message);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }
            else
            {
                Rate rates = new Rate();
                //Read the contents into a string variable.
                string strJSON = response.Content.ReadAsStringAsync().Result;

                //Deserialize into object.
                dynamic arrJSON = (JObject)JsonConvert.DeserializeObject(strJSON);

                rates.COP = arrJSON["rates"]["COP"];
                rates.MXN = arrJSON["rates"]["MXN"];

                return rates;
            }
        }

        private HttpClient InitializeHttpClient()
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_exchangeRatesAddress);
            return httpClient;
        }

        #endregion
    }
}
