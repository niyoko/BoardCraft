namespace BoardCraft.Example.Wpf.ViewModels
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;
    using Models;
    using NLog;
    using Placement.GA;
    using Routing;
    internal class SchematicMetadataViewModel : ViewModelBase
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private Population _currentPopulation;
        private Population _showedPopulation;

        private Board _showedPlacement;        

        private bool _pauseRequested;
        private ManualResetEvent _stopRequested;
        private readonly DispatcherTimer _timer;

        public SchematicMetadataViewModel(Schematic schematic, string tabTitle)
        {
            Schematic = schematic;
            TabTitle = tabTitle;

            RunGACommand = new GenericCommand(StartGA, CanExecuteGA);
            PauseGACommand = new GenericCommand(PauseGA, CanExecuteGA);
            StopGACommand = new GenericCommand(StopGA, CanExecuteGA);

            Placer = ConstructGAPlacer();

            Properties = new SchematicProperties();
            UpdatePropertiesFromSchema();

            _timer = new DispatcherTimer(DispatcherPriority.Normal, Application.Current.Dispatcher);
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += PeriodicalUpdate;
            _timer.Start();
        }

        private bool CanExecuteGA()
        {
            return _stopRequested == null;
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

        public GenericCommand PauseGACommand { get; }

        public GenericCommand WindowClosedCommand { get; }

        public GenericCommand StopGACommand { get; }

        public async void StartGA()
        {
            _pauseRequested = false;
            await Task.Run(() =>
            {
                while (true)
                {
                    if (_pauseRequested)
                    {
                        break;
                    }

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

        public void PauseGA()
        {
            _pauseRequested = true;
        }

        public void StopGA()
        {
            _stopRequested = new ManualResetEvent(false);
            RunGACommand.RaiseCanExecuteChanged();
            PauseGACommand.RaiseCanExecuteChanged();
            StopGACommand.RaiseCanExecuteChanged();

            //wait until GA stopped
            _stopRequested.WaitOne(3000);

            var p = _currentPopulation.BestPlacement;
            var t = new Router(1, 1);
            t.Route(p);
            UpdatePopulation(true);
        }

        private void PeriodicalUpdate(object sender, EventArgs args)
        {
            UpdatePopulation(false);
        }

        private void UpdatePopulation(bool force)
        {
            var c = _currentPopulation;
            if (c == _showedPopulation) return;

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
                if(force)
                    OnPropertyChanged(nameof(ShowedPlacement));

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