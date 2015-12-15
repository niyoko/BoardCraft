namespace BoardCraft.Example.Wpf.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;
    using Models;
    using Placement.GA;

    internal class SchematicMetadataViewModel : ViewModelBase
    {
        private ComponentPlacement _currentPlacement;

        private bool _pauseRequested;

        public SchematicMetadataViewModel(Schematic schematic, string tabTitle)
        {
            Schematic = schematic;
            TabTitle = tabTitle;

            Properties = new SchematicProperties(Schematic);
            RunGACommand = new GenericCommand(StartGA);
            PauseGACommand = new GenericCommand(PauseGA);

            Placer = ConstructGAPlacer();
        }

        public Schematic Schematic { get; }

        public GAPlacer Placer { get; }

        public string TabTitle { get; }

        public int ComponentCount => Schematic.Components.Count;

        public ComponentPlacement CurrentPlacement
        {
            get
            {
                return _currentPlacement;
            }

            set
            {
                if (value == _currentPlacement)
                {
                    return;
                }

                _currentPlacement = value;
                OnPropertyChanged(nameof(CurrentPlacement));
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
                    var bestP = Placer.CurrentPopulation.GetBestPlacement();

                    Application.Current.Dispatcher.BeginInvoke(
                        DispatcherPriority.Background,
                        new Action(() => { CurrentPlacement = bestP; }));
                }
            });
        }

        public void PauseGA()
        {
            _pauseRequested = true;
        }

        private GAPlacer ConstructGAPlacer()
        {
            const int PopulationSize = 20;
            IPopulationGenerator initPlacer = new RandomPopulationGenerator();
            IFitnessEvaluator fitnessEvaluator = new FitnessEvaluator();
            ISelectionOperator selectionOp = new TournamentSelectionOperator(8, 0.5);
            const double CrossoverRate = 1.0;
            var crossedMin = (int)(0.4 * Schematic.Components.Count);
            var crossedMax = (int)(0.6 * Schematic.Components.Count);
            var crossoverOp = new CrossoverOperator(crossedMin, crossedMax);
            const double MutationRate = 0.1;
            IMutationOperator mutationOp = new MutationOperator();
            IReproductionOperator reproOp = new ReproductionOperator(
                selectionOp,
                CrossoverRate,
                crossoverOp,
                MutationRate,
                mutationOp);

            return new GAPlacer(Schematic, PopulationSize, initPlacer, fitnessEvaluator, reproOp);
        }

        private void PlacerOnNewGeneration(ComponentPlacement componentPlacement)
        {
            CurrentPlacement = componentPlacement;
        }
    }
}