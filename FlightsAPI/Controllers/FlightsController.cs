using FlightsAPI.Dijkstra;
using FlightsAPI.Models;
using FlightsAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightsAPI.Controllers
{
    [Route("[controller]")]
    public class FlightsController : ControllerBase
    {

        private readonly ILogger<FlightsController> logger;
        private IFlightService service;
        private ExchangeRatesService exchangeRatesService;

        public FlightsController(ILogger<FlightsController> logger, IFlightService service, ExchangeRatesService exchangeRatesService)
        {
            this.logger = logger;
            this.service = service;
            this.exchangeRatesService = exchangeRatesService;
        }

        /// <summary>
        /// Returns the list of flights
        /// </summary>
        /// <param name="origin">origin (string)</param>
        /// <param name="destination">destination (string)</param>
        /// <param name="badge">badge (string)</param>
        /// <returns>The list of Journey</returns>
        /// <response code="200">OK. Returns the list of Journey objects</response>
        [HttpGet("{origin}/{destination}")]
        public async Task<ActionResult<List<Journey>>> Get(string origin, string destination, string badge = "USD")
        {
            return Ok(await FindFlight(origin, destination, badge));
        }

        /// <summary>
        /// Returns the list of flights
        /// </summary>
        /// <param name="origin">origin (string)</param>
        /// <param name="destination">destination (string)</param>
        /// <param name="badge">badge (string)</param>
        /// <returns>The list of Journey objects</returns>
        public async Task<List<Journey>> FindFlight(string origin, string destination, string badge)
        {
            try
            {
                List<Journey> flights = new List<Journey>();
                List<Flight> routes = new List<Flight>();
                Rate rates = new Rate();

                List<FlightProvider> result = await service.GetFlights();

                // First, it is validated that origin and destination exist in the api response object, in case one of the two does not exist, an empty list is returned.
                List<FlightProvider> destinations = result.Where(f => f.ArrivalStation == destination).ToList();
                List<FlightProvider> origins = result.Where(f => f.DepartureStation == origin).ToList();

                if (destinations.Count == 0 || origins.Count == 0)
                {
                    logger.LogInformation("There is no flight path. destination: {0}, origin: {1}", destination, origin);
                    return new List<Journey>();
                }

                try
                {
                    //It is searched if there is a route that is direct between origin and destination
                    FlightProvider directo = result.Where(f => f.DepartureStation == origin && f.ArrivalStation == destination).FirstOrDefault();

                    if (directo != null)
                    {
                        double price = directo.Price;
                        if (badge != "USD")
                        {
                            rates = await exchangeRatesService.GetRates();
                            if (badge == "COP")
                            {
                                price = Math.Round(directo.Price * rates.COP, 2);
                            } else if (badge == "MXN")
                            {
                                price = Math.Round(directo.Price * rates.MXN, 2);
                            }
                        }
                        Journey flight = new Journey();
                        flight.Origin = origin;
                        flight.Destination = destination;
                        flight.Price = price;

                        Flight infoFlight = new Flight();
                        infoFlight.Origin = origin;
                        infoFlight.Destination = destination;
                        infoFlight.Price = price;

                        Transport transport = new Transport();
                        transport.FlightCarrier = directo.FlightCarrier;
                        transport.FlightNumber = directo.FlightNumber;

                        infoFlight.Transport = transport;
                        routes.Add(infoFlight);

                        flight.Flights = routes;

                        flights.Add(flight);
                    }
                    else
                    {
                        //If there is no direct route between origin and destination, the dijkstra algorithm is used to obtain the cheapest route
                        flights = FindCheaperFlights(origin, destination, badge, result, rates);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error found flight destination: {0}, origin: {1}", destination, origin);
                    return new List<Journey>();
                }

                return flights;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error found flight destination: {0}, origin: {1}", destination, origin);
                return new List<Journey>();
            }

        }

        /// <summary>
        /// Create the list of nodes and the graph of cities.
        /// </summary>
        /// <param name="flightProvider">flightProvider (List<FlightProvider>)</param>
        /// <returns>Returns the DistanceCalculator</returns>
        public DistanceCalculator DefineNodes(List<FlightProvider> flightProvider)
        {
            Graph Cities = new Graph();

            List<string> citiesOrigin = flightProvider.Select(c => c.DepartureStation).Distinct().ToList();
            List<string> citiesDestination = flightProvider.Select(c => c.ArrivalStation).Distinct().ToList();
            List<string> citiesList = citiesOrigin.Concat(citiesDestination).Distinct().ToList();

            List<Node> nodes = new List<Node>();
            foreach (string citie in citiesList)
            {
                Node node = new Node(citie);
                nodes.Add(node);
                Cities.Add(node);
            }
            foreach (Node node in nodes)
            {
                string name = node.GetName();
                List<FlightProvider> destinations = flightProvider.Where(f => f.DepartureStation == name).ToList();
                foreach (FlightProvider flight in destinations)
                {
                    Node nodeAdd = nodes.Where(n => n.GetName() == flight.ArrivalStation).FirstOrDefault();
                    node.AddNeighbour(nodeAdd, flight.Price);
                }
            }

            DistanceCalculator c = new DistanceCalculator(Cities);

            return c;
        }

        /// <summary>
        /// Gets the response from dijkstra's algorithm and returns a Journey list
        /// </summary>
        /// <param name="origin">origin (string)</param>
        /// <param name="destination">destination (string)</param>
        /// <param name="badge">badge (string)</param>
        /// <param name="flightProvider">flightProvider (List<FlightProvider>)</param>
        /// <param name="rates">rates (Rate)</param>
        /// <returns>Returns the list of Journey objects</returns>
        public List<Journey> FindCheaperFlights(string origin, string destination, string badge, List<FlightProvider> flightProvider, Rate rates)
        {
            List<Journey> flights = new List<Journey>();
            List<Flight> routes = new List<Flight>();

            DistanceCalculator c = DefineNodes(flightProvider);
            List<Node> nodes = c.AllNodes;

            Node departure = null;
            Node arrival = null;

            foreach (Node node in nodes)
            {
                if (node.GetName() == origin)
                {
                    departure = node;
                }
                if (node.GetName() == destination)
                {
                    arrival = node;
                }
            }
            if (departure != null && arrival != null)
            {
                c.Calculate(departure, arrival);

                double price = c.totalPrice;
                if (badge != "USD")
                {
                    if (badge == "COP")
                    {
                        price = Math.Round(c.totalPrice * rates.COP, 2);
                    }
                    else if (badge == "MXN")
                    {
                        price = Math.Round(c.totalPrice * rates.MXN, 2);
                    }
                }

                Journey flight = new Journey();
                flight.Origin = origin;
                flight.Destination = destination;
                flight.Price = price;

                List<Route> connections = c.Connections;

                //the connection list is reverted
                for (int i = 0, j = connections.Count - 1; i < j; i++)
                {
                    Route temp = connections[j];
                    connections.RemoveAt(j);
                    connections.Insert(i, temp);
                }

                foreach (Route connection in connections)
                {
                    Flight infoFlight = new Flight();

                    FlightProvider directo = flightProvider.Where(f => f.DepartureStation == connection.Origin && f.ArrivalStation == connection.Destination).FirstOrDefault();

                    price = directo.Price;
                    if (badge != "USD")
                    {
                        if (badge == "COP")
                        {
                            price = Math.Round(directo.Price * rates.COP, 2);
                        }
                        else if (badge == "MXN")
                        {
                            price = Math.Round(directo.Price * rates.MXN, 2);
                        }
                    }

                    infoFlight.Origin = connection.Origin;
                    infoFlight.Destination = connection.Destination;
                    infoFlight.Price = price;

                    Transport transport = new Transport();
                    transport.FlightCarrier = directo.FlightCarrier;
                    transport.FlightNumber = directo.FlightNumber;

                    infoFlight.Transport = transport;

                    routes.Add(infoFlight);

                    flight.Flights = routes;
                }

                flights.Add(flight);

            }
            return flights;
        }
    }
}