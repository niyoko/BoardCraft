using System.Windows;
using System.Windows.Controls;

namespace BoardCraft.Run.Views
{
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
            MessageBox.Show(DataContext.GetType().Name);
        }
    }
}
