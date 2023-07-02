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
    public interface IFlightService
    {
        public Task<List<FlightProvider>> GetFlights();
    }
}
