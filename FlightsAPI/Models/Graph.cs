using System.Collections.Generic;
using System.Linq;

namespace FlightsAPI.Models
{
    public class Graph
    {
        private List<Node> Nodes;

        public Graph()
        {
            Nodes = new List<Node>();
        }

        /// <summary>
        /// Add nodes from the list
        /// <summary>
        public void Add(Node n)
        {
            Nodes.Add(n);
        }
        /// <summary>
        /// Remove nodes from the list
        /// <summary>
        public void Remove(Node n)
        {
            Nodes.Remove(n);
        }

        /// <summary>
        /// Returns the list of nodes
        /// <summary>
        public List<Node> GetNodes()
        {
            return Nodes.ToList();
        }

        /// <summary>
        /// Return number of nodes
        /// <summary>
        public int GetCount()
        {
            return Nodes.Count;
        }
    }
}
