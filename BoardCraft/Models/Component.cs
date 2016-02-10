namespace BoardCraft.Models
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Drawing;

    /// <summary>
    ///     Represent component in the <see cref="Schematic" />, such as resistor, capacitor, IC, etc.
    /// </summary>
    public class Component
    {
        private Dictionary<string, ComponentPin> _internalPins; 

        /// <summary>
        ///     Create instance of a <see cref="Component" />
        /// </summary>
        /// <param name="id"></param>
        /// <param name="package">Package type of this <see cref="Component" /></param>
        internal Component(string id, Package package, bool isHighPower)
        {
            Id = id;
            Package = package;
            IsHighPower = isHighPower;

            var p = package.Pins.Select(ConvertPin);
            _internalPins = p.ToDictionary(x => x.Name);
        }

        private ComponentPin ConvertPin(Pin packagePin)
        {
            return new ComponentPin(this, packagePin);
        }

        public string Id { get; }
        public Package Package { get; }
        public bool IsHighPower { get; }
        public ICollection<ComponentPin> Pins => _internalPins.Values;

        public ComponentPin GetPin(string name)
        {
            return _internalPins[name];
        }
    }
}