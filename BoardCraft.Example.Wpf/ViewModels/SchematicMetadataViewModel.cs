namespace BoardCraft.Example.Wpf.ViewModels
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;
    using Models;
    using Placement.GA;

    internal class SchematicMetadataViewModel : ViewModelBase
    {
        private Population _currentPopulation;
        private Population _showedPopulation;

        private ComponentPlacement _showedPlacement;        

        private bool _pauseRequested;
        private readonly DispatcherTimer _timer;

        public SchematicMetadataViewModel(Schematic schematic, string tabTitle)
        {
            Schematic = schematic;
            TabTitle = tabTitle;

            RunGACommand = new GenericCommand(StartGA);
            PauseGACommand = new GenericCommand(PauseGA);

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

        public ComponentPlacement ShowedPlacement
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

        public ICommand RunGACommand { get; }

        public ICommand PauseGACommand { get; }

        public ICommand WindowClosedCommand { get; }

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

                    Placer.NextGeneration();
                    _currentPopulation = Placer.CurrentPopulation;
                }
            });
        }

        public void PauseGA()
        {
            _pauseRequested = true;
        }

        private void PeriodicalUpdate(object sender, EventArgs args)
        {
            var c = _currentPopulation;
            if(c == _showedPopulation) return;

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

                ShowedPlacement = c.BestPlacement;
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