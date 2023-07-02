using FlightsAPI.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace FlightsAPI.Dijkstra
{
    public class DistanceCalculator
    {
        Dictionary<Node, double> Distances;
        Dictionary<Node, Node> Routes;
        Graph graph;
        public List<Node> AllNodes;
        public List<Route> Connections;
        public double totalPrice;

        public DistanceCalculator(Graph g) 
        {
            this.graph = g;
            this.AllNodes = g.GetNodes();
            Distances = SetDistances();
            Routes = SetRoutes();
        }

        /// <summary>
        /// The algorithm starts by selecting the city of origin from the set of all nodes. Then, 
        /// for each iteration, the chosen city must have the smallest distance value in the distance table. 
        /// It then goes through all of its neighbors and calculates the cost of traveling from that city to each of its neighbors. 
        /// If the cost is less than the value in the distance table, the new value will be updated in the distance table.
        /// <summary>
        public void Calculate(Node Source, Node Destination)
        {
            Distances[Source] = 0;

            while (AllNodes.ToList().Count != 0)
            {
                Node LeastExpensiveNode = GetLeastExpensiveNode();
                ExamineConnections(LeastExpensiveNode);
                AllNodes.Remove(LeastExpensiveNode);
            }
            Travel(Source, Destination);
        }

        /// <summary>
        /// Returns the total value of the trip
        /// <summary>
        private void Travel(Node Source, Node Destination)
        {           
            totalPrice = Distances[Destination];
            Connections = new List<Route>();
            Stops(Destination);
        }

        /// <summary>
        /// Returns the total value of the trip
        /// <summary>
        private void Stops(Node d)
        {            
            if (Routes[d] == null)
                return;

            Route route = new Route();
            route.Origin = Routes[d].GetName();
            route.Destination = d.GetName();
            Connections.Add(route);
            Stops(Routes[d]);
        }

        /// <summary>
        /// Examines the neighbors of the city of origin, updates the distances and the route table accordingly.
        /// <summary>
        private void ExamineConnections(Node n)
        {
            foreach (var neighbor in n.GetNeighbors())
            {
                if (Distances[n] + neighbor.Value < Distances[neighbor.Key])
                {
                    Distances[neighbor.Key] = neighbor.Value + Distances[n];
                    Routes[neighbor.Key] = n;
                }
            }
        }

        /// <summary>
        /// Select the city from the set of nodes with the least distance in the distance table
        /// <summary>        
        private Node GetLeastExpensiveNode()
        {
            Node LeastExpensive = AllNodes.FirstOrDefault();

            foreach (var n in AllNodes)
            {
                if (Distances[n] < Distances[LeastExpensive])
                    LeastExpensive = n;
            }

            return LeastExpensive;
        }

        /// <summary>
        /// Returns the distance directory
        /// <summary>
        private Dictionary<Node, double> SetDistances()
        {
            Dictionary<Node, double> Distances = new Dictionary<Node, double>();

            foreach (Node n in graph.GetNodes())
            {
                Distances.Add(n, double.MaxValue);
            }
            return Distances;
        }

        /// <summary>
        /// Returns the routes directory
        /// <summary>
        private Dictionary<Node, Node> SetRoutes()
        {
            Dictionary<Node, Node> Routes = new Dictionary<Node, Node>();

            foreach (Node n in graph.GetNodes())
            {
                Routes.Add(n, null);
            }
            return Routes;
        }
    }
}
