using System;

namespace BoardCraft.Example.Wpf.ViewModels
{
    using Output.Wpf;
    using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

    class ColorPalleteItemSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            var s = new ItemCollection
            {
                { ColorPallete.Color, "Color" },
                { ColorPallete.Black, "Black" }
            };

            return s;
        }
    }
}
