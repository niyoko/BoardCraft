using System.Windows;
using System.Windows.Controls;

namespace BoardCraft.Example.Wpf.Views
{
    using System.Diagnostics;

    /// <summary>
    /// Interaction logic for SchematicMetadata.xaml
    /// </summary>
    public partial class SchematicMetadata : UserControl
    {
        public SchematicMetadata()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(DataContext?.GetType().Name ?? "null");
        }
    }
}
