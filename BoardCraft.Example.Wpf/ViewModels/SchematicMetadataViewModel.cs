namespace BoardCraft.Example.Wpf.ViewModels
{
    using System;
    using System.Diagnostics;
    using System.Windows.Input;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;
    using Models;
    using Placement.GA;

    class SchematicMetadataViewModel : ViewModelBase
    {
        public Schematic Schematic { get; }      
        public GAPlacer Placer { get; }

        public SchematicMetadataViewModel(Schematic schematic, string tabTitle)
        {
            Schematic = schematic;
            TabTitle = tabTitle;

            Properties = new SchematicProperties(Schematic);
            RunGACommand = new GenericCommand(StartGA);
            PauseGACommand = new GenericCommand(PauseGA);

            Placer = ConstructGAPlacer();
        }

        private GAPlacer ConstructGAPlacer()
        {
            const int popSize = 20;
            IPopulationGenerator initPlacer = new RandomPopulationGenerator();
            IFitnessEvaluator fitnessEvaluator = new FitnessEvaluator();
            ISelectionOperator selectionOp = new TournamentSelectionOperator(8, 0.5);
            const double crossoverRate = 1.0;
            var crossedMin = (int)(0.4 * Schematic.Components.Count);
            var crossedMax = (int)(0.6 * Schematic.Components.Count);
            var crossoverOp = new CrossoverOperator(crossedMin, crossedMax);
            const double mutationRate = 0.1;
            IMutationOperator mutationOp = new MutationOperator();
            IReproductionOperator reproOp = new ReproductionOperator(selectionOp, crossoverRate, crossoverOp, mutationRate, mutationOp);

            return new GAPlacer(Schematic, popSize, initPlacer, fitnessEvaluator, reproOp);
        }

        private bool _pauseRequested;
        public async void StartGA()
        {
            _pauseRequested = false;
            await Task.Run(() =>
            {
                while (true)
                {
                    if(_pauseRequested)
                        break;
                                        
                    Placer.NextGeneration();
                    var bestP = Placer.CurrentPopulation.GetBestPlacement();

                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                    {
                        this.CurrentPlacement = bestP;
                    }));
                }
            });
        }

        public void PauseGA()
        {
            _pauseRequested = true;
        }

        private void PlacerOnNewGeneration(ComponentPlacement componentPlacement)
        {
            CurrentPlacement = componentPlacement;
        }

        public string TabTitle { get; }

        public int ComponentCount => Schematic.Components.Count;

        private ComponentPlacement _currentPlacement;

        public ComponentPlacement CurrentPlacement
        {
            get { return _currentPlacement; }
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
    }
}
