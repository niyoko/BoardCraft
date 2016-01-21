namespace BoardCraft.Models
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    ///     Represent a connection between component pins
    /// </summary>
    public sealed class Connection
    {
        private readonly List<PinConnectionInfo> _pins;

        public ICollection<PinConnectionInfo> Pins { get; } 

        public Connection(string id)
        {
            Id = id;
            _pins = new List<PinConnectionInfo>();
            Pins = new ReadOnlyCollection<PinConnectionInfo>(_pins);
        }

        public string Id { get; }

        public PinConnectionInfo AddPin(Component component, string pin)
        {
            var cp = component.GetPin(pin);
            return AddPin(cp);
        }

        public PinConnectionInfo AddPin(ComponentPin pin)
        {
            var info = new PinConnectionInfo(this, pin);
            _pins.Add(info);
            return info;
        }
    }

    public sealed class PinConnectionInfo
    {
        internal PinConnectionInfo(Connection connection, ComponentPin pin)
        {
            Connection = connection;
            Pin = pin;
        }

        public Connection Connection { get; }
        public ComponentPin Pin { get; }
    }
}