namespace BoardCraft.Models
{
    using System.Collections.Generic;

    /// <summary>
    ///     Represent a connection between component pins
    /// </summary>
    public sealed class Connection
    {
        private IList<PinConnectionInfo> Pins { get; } 

        public Connection(string id)
        {
            Id = id;
            Pins = new List<PinConnectionInfo>();
        }

        public string Id { get; }

        public PinConnectionInfo AddPin(Component component, string pin)
        {
            var info= new PinConnectionInfo(this, component, pin);
            Pins.Add(info);
            return info;
        }
    }

    public sealed class PinConnectionInfo
    {
        internal PinConnectionInfo(Connection connection, Component component, string pin)
        {
            Connection = connection;
            Component = component;
            Pin = pin;
        }

        public Connection Connection { get; }
        public Component Component { get; }
        public string Pin { get; }
    }
}