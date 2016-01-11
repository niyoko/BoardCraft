namespace BoardCraft.Models
{
    using System.Collections.Generic;

    /// <summary>
    ///     Represent an electronic schema.
    /// </summary>
    public sealed class Schematic
    {
        private Dictionary<string, Component> ComponentDictionary { get; }
        private readonly Dictionary<string, Connection> _connections;

        public Schematic()
        {
            _connections = new Dictionary<string, Connection>();
            ComponentDictionary = new Dictionary<string, Component>();
        }

        public ICollection<Component> Components => ComponentDictionary.Values;

        public Component AddComponent(string id, Package package)
        {
            var comp = new Component(id, package);
            ComponentDictionary.Add(id, comp);            
            return comp;
        }

        public Connection AddConnection(string id)
        {
            var con = new Connection(id);
            _connections.Add(id, con);
            return con;
        }

        public Component GetComponent(string id)
        {
            return ComponentDictionary[id];
        }
    }
}