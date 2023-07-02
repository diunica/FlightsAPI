using System.Collections.Generic;

namespace FlightsAPI.Models
{
    public class Journey
    {
        public List<Flight> Flights { get; set; }

        public string Origin { get; set; }

        public string Destination { get; set; }

        public double Price { get; set; }
    }
}
