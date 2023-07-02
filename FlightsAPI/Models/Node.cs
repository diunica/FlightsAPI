using System.Collections.Generic;

namespace FlightsAPI.Models
{
    public class Node
    {
        private string Name;
        private Dictionary<Node, double> Neighbors;

        public Node(string NodeName)
        {
            this.Name = NodeName;
            Neighbors = new Dictionary<Node, double>();
        }

        /// <summary>
        /// Add neighbour from the dictionary
        /// <summary>
        public void AddNeighbour(Node n, double cost)
        {
            Neighbors.Add(n, cost);
        }

        /// <summary>
        /// Get the name of the node
        /// <summary>
        public string GetName()
        {
            return Name;
        }

        /// <summary>
        /// Returns the dictionary of connections from a node
        /// <summary>
        public Dictionary<Node, double> GetNeighbors()
        {
            return Neighbors;
        }
    }
}
