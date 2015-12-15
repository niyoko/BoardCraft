namespace BoardCraft.Example.Wpf.ViewModels
{
    using System.ComponentModel;
    using Models;

    class SchematicProperties
    {
        private Schematic Schematic { get; }

        public SchematicProperties(Schematic schematic)
        {
            Schematic = schematic;
        }

        [Category("Informasi Skematik")]
        [DisplayName("Jumlah Komponen")]
        [Description("Jumlah komponen yang terdapat pada skema elektronika")]
        public int ComponentCount => Schematic.Components.Count;


    }
}
