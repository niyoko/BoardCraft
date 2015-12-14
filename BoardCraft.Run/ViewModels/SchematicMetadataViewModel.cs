using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardCraft.Run.ViewModels
{
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Input;
    using Input;
    using Models;

    class SchematicMetadataViewModel : ViewModelBase
    {
        public Schematic Schematic { get; }      

        public SchematicMetadataViewModel(Schematic schematic, string tabTitle)
        {
            Schematic = schematic;
            TabTitle = tabTitle;

            Properties = new SchematicProperties();
        }

        public string TabTitle { get; }

        public int ComponentCount => Schematic.Components.Count;

        public SchematicProperties Properties { get; }
    }
}
