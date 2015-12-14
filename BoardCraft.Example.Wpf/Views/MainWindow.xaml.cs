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
            var cRepo = new JsonFileLibrary(@"..\..\Library");
            cRepo.Load();

            var sch = new Schematic();
            for (var i = 0; i < 200; i++)
            {
                var res = cRepo.GetPackage("RES40");
                sch.AddComponent($"R{i + 1}", res);
            }

            for (var i = 0; i < 20; i++)
            {
                var res = cRepo.GetPackage("ELEC-RAD20");
                sch.AddComponent($"C{i + 1}", res);
            }

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            dispatcherTimer.Start();

            await Task.Run(() =>
            {
                var placer = new GAPlacer(20, new RandomPopulationGenerator(), 
                    new FitnessEvaluator(),
                    new ReproductionOperator(new TournamentSelectionOperator(8, 0.5), 
                    1, new CrossoverOperator((int)(0.4*sch.Components.Count), (int)(0.6*sch.Components.Count)),
                    0.1, new MutationOperator()));
                placer.NewGeneration += OnNewGen;
                placer.Place(sch);
            });

            dispatcherTimer.Stop();
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