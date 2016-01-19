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
    using NLog;
    using Placement.GA;
    using Routing;
    internal class SchematicMetadataViewModel : ViewModelBase
    {
        private Population _currentPopulation;
        private Population _showedPopulation;

        private Board _showedPlacement;        

        private ManualResetEvent _stopRequested;

        enum State
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
            UpdatePropertiesFromSchema();

            _timer = new DispatcherTimer(DispatcherPriority.Normal, Application.Current.Dispatcher);
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += PeriodicalUpdate;
            _timer.Start();
        }

        public Schematic Schematic { get; }
        public GAPlacer Placer { get; }

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

        public GenericCommand RunGACommand { get; }
        public GenericCommand WindowClosedCommand { get; }
        public GenericCommand StopGACommand { get; }
        public GenericCommand BeginRouteCommand { get; }

        public async void BeginRouting()
        {
            _state = State.Routing;
            UpdateButtonState();

            Board b = null;
            await Task.Run(() =>
            {
                var p = _currentPopulation.BestPlacement;
                b = p;
                var t = new Router(30, 10);
                t.Route(p);                
            });

            _state = State.RoutingFinished;
            UpdateButtonState();
            UpdatePopulation(true);
        }

        public async void StartGA()
        {
            _state = State.GARunning;
            UpdateButtonState();
            await Task.Run(() =>
            {
                while (true)
                {
                    if (_stopRequested != null)
                    {
                        _stopRequested.Set();
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

        void UpdateButtonState()
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
            var c = _currentPopulation;

            var f2 = false;
            if (c == _showedPopulation)
            {
                if (!force)
                {
                    return;
                }
                else
                {
                    ShowedPlacement = null;
                }
            }

            _showedPopulation = c;
            if (c == null)
            {
                ShowedPlacement = null;
                Properties.GenerationCount = null;
                Properties.MaxFitness = null;
                Properties.AverageFitness = null;
            }
            else
            {
                var fits = c.Select(c.GetFitnessFor).ToList();
                var p = _currentPopulation.BestPlacement;
                ShowedPlacement = p;

                Properties.GenerationCount = c.Generation;
                Properties.AverageFitness = fits.Average();
                Properties.MaxFitness = fits.Max();
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