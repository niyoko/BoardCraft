namespace BoardCraft.Example.Wpf.ViewModels
{
    using System.ComponentModel;
    using Models;

    internal class SchematicProperties
    {
        public SchematicProperties(Schematic schematic)
        {
            Schematic = schematic;
        }

        [Category("Informasi Skematik")]
        [DisplayName("Jumlah Komponen")]
        [Description("Jumlah komponen yang terdapat pada skema elektronika")]
        public int ComponentCount => Schematic.Components.Count;

        private Schematic Schematic { get; }
    }
}