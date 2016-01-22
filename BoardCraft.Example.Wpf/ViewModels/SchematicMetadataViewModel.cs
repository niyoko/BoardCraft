namespace BoardCraft.Example.Wpf.ViewModels
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;
    using Models;
    using Output.Wpf;
    using Placement.GA;
    using Routing;
    internal class SchematicMetadataViewModel : ViewModelBase
    {
        private Population _currentPopulation;
        private Population _showedPopulation;

        private readonly Stopwatch _gaStopwatch;
        private readonly Stopwatch _routingStopwatch;

        private Board _showedPlacement;        

        private ManualResetEvent _stopRequested;

        private enum State
        {
            Initial,            
            GARunning,
            GAPaused,
            Routing,
            RoutingFinished
        }

        private State _state;

        private readonly DispatcherTimer _timer;

        public SchematicMetadataViewModel(Schematic schematic, string tabTitle)
        {
            Schematic = schematic;
            TabTitle = tabTitle;

            RunGACommand = new GenericCommand(StartGA, () => _state == State.Initial || _state == State.GAPaused);
            StopGACommand = new GenericCommand(StopGA, () => _state == State.GARunning && _stopRequested == null);
            BeginRouteCommand = new GenericCommand(BeginRouting, () => _state == State.GAPaused);

            Placer = ConstructGAPlacer();

            Properties = new SchematicProperties();
            OutputProperties = new OutputProperties();
            UpdatePropertiesFromSchema();

            OutputProperties.PropertyChanged += (sender, args) =>
            {
                OnPropertyChanged(args.PropertyName);
            };

            _gaStopwatch = new Stopwatch();
            _routingStopwatch = new Stopwatch();

            _timer = new DispatcherTimer(DispatcherPriority.Normal, Application.Current.Dispatcher)
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += PeriodicalUpdate;
            _timer.Start();

        }

        public Schematic Schematic { get; }
        public GAPlacer Placer { get; }

        //Layers
        public bool Component => OutputProperties.Component;
        public bool BottomCopper => OutputProperties.BottomCopper;
        public bool TopCopper => OutputProperties.TopCopper;
        public bool DrillHole => OutputProperties.DrillHole;
        public bool Pad => OutputProperties.Pad;
        public bool Via => OutputProperties.Via;
        public bool BoardEdge => OutputProperties.BoardEdge;

        public ColorPallete ColorPallete => OutputProperties.ColorPallete;

        public string TabTitle { get; }

        public int ComponentCount => Schematic.Components.Count;

        public Board ShowedPlacement
        {
            get { return _showedPlacement; }
            set
            {
                if (value == _showedPlacement)
                {
                    return;
                }

                _showedPlacement = value;
                OnPropertyChanged(nameof(ShowedPlacement));
            }
        }

        public SchematicProperties Properties { get; }
        public OutputProperties OutputProperties { get; }

        public GenericCommand RunGACommand { get; }
        public GenericCommand WindowClosedCommand { get; }
        public GenericCommand StopGACommand { get; }
        public GenericCommand BeginRouteCommand { get; }

        public async void BeginRouting()
        {
            _state = State.Routing;
            UpdateButtonState();

            lock (_routingStopwatch)
            {
                _routingStopwatch.Start();
            }
            await Task.Run(() =>
            {
                var p = _currentPopulation.BestPlacement;
                var t = new Router(20, 10, 3);
                t.Route(p);
            });
            lock (_routingStopwatch)
            {
                _routingStopwatch.Stop();
            }

            _state = State.RoutingFinished;
            UpdateButtonState();
            UpdatePopulation(true);
            MessageBox.Show("Routing selesai");
        }

        public async void StartGA()
        {
            _state = State.GARunning;
            UpdateButtonState();
            await Task.Run(() =>
            {
                lock (_gaStopwatch)
                {
                    _gaStopwatch.Start();
                }

                while (true)
                {
                    if (_stopRequested != null)
                    {
                        _stopRequested.Set();
                        lock (_gaStopwatch)
                        {
                            _gaStopwatch.Stop();
                        }
                        break;
                    }

                    Placer.NextGeneration();
                    _currentPopulation = Placer.CurrentPopulation;
                }
            });
        }

        public void StopGA()
        {
            _stopRequested = new ManualResetEvent(false);
            UpdateButtonState();

            //wait until GA stopped
            if (_stopRequested.WaitOne(30000))
            {
                _stopRequested = null;
                _state = State.GAPaused;
                UpdateButtonState();
                return;
            }

            throw new Exception("GA stuck");
        }

        private void UpdateButtonState()
        {
            RunGACommand.RaiseCanExecuteChanged();            
            StopGACommand.RaiseCanExecuteChanged();
            BeginRouteCommand.RaiseCanExecuteChanged();
        }

        private void PeriodicalUpdate(object sender, EventArgs args)
        {
            UpdatePopulation(false);
        }

        private void UpdatePopulation(bool force)
        {
            lock (_gaStopwatch)
            {
                Properties.GATime = _gaStopwatch.Elapsed;
            }

            lock (_routingStopwatch)
            {
                Properties.RoutingTime = _routingStopwatch.Elapsed;
            }

            var c = _currentPopulation;
            if (c == _showedPopulation)
            {
                if (!force)
                {
                    return;
                }

                ShowedPlacement = null;
            }

            _showedPopulation = c;
            if (c == null)
            {
                ShowedPlacement = null;
                Properties.GenerationCount = null;
                Properties.MaxFitness = null;
                Properties.AverageFitness = null;
                Properties.Panjang = null;
                Properties.Lebar = null;
            }
            else
            {
                var fits = c.Select(c.GetFitnessFor).ToList();
                var p = _currentPopulation.BestPlacement;
                ShowedPlacement = p;

                Properties.GenerationCount = c.Generation;
                Properties.AverageFitness = fits.Average();
                Properties.MaxFitness = fits.Max();

                var s = p.GetSize();
                var pjg = (int) s.Width + p.Margin.Left + p.Margin.Right;
                var lbr = (int) s.Height + p.Margin.Bottom + p.Margin.Top;

                Properties.Panjang = pjg;
                Properties.Lebar = lbr;
            }
        }

        private GAPlacer ConstructGAPlacer()
        {
            const int populationSize = 20;
            IPopulationGenerator initPlacer = new RandomPopulationGenerator();
            IFitnessEvaluator fitnessEvaluator = new FitnessEvaluator();
            ISelectionOperator selectionOp = new TournamentSelectionOperator(8, 0.5);
            const double crossoverRate = 1.0;
            var crossedMin = (int)(0.4 * Schematic.Components.Count);
            var crossedMax = (int)(0.6 * Schematic.Components.Count);
            var crossoverOp = new CrossoverOperator(crossedMin, crossedMax);
            const double mutationRate = 0.1;
            IMutationOperator mutationOp = new MutationOperator();

            IReproductionOperator reproOp = new ReproductionOperator(
                selectionOp,
                crossoverRate,
                crossoverOp,
                mutationRate,
                mutationOp);

            return new GAPlacer(Schematic, populationSize, initPlacer, fitnessEvaluator, reproOp);
        }

        private void UpdatePropertiesFromSchema()
        {
            Properties.ComponentCount = Schematic.Components.Count;            
        }
    }
}