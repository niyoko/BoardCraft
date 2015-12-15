namespace BoardCraft.Example.Wpf.Views
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using System.Windows;
    using Input;
    using Models;
    using Output.Wpf;
    using Placement.GA;
    using Microsoft.Win32;
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly WpfCanvas _cnv;

        public MainWindow()
        {
            InitializeComponent();
            //_cnv = new WpfCanvas(Canvas1);
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter",
            Justification = "This is event handler")]
        private async void button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private ComponentPlacement _p;
        private ComponentPlacement _lastDrawn;
        private object _pLocker = new object();
        void OnNewGen(ComponentPlacement p)
        {
            _p = p;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            var z = _p;
            if (z != null && z != _lastDrawn)
            {
                z.Draw(_cnv);
                _lastDrawn = z;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

    }
}