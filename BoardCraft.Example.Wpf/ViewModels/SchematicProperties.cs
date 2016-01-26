namespace BoardCraft.Example.Wpf.ViewModels
{
    using System;
    using System.ComponentModel;

    internal class SchematicProperties : INotifyPropertyChanged
    {
        private int? _componentCount;
        private int? _generationNumber;
        private int? _panjang;
        private int? _lebar;
        private double? _maxFitness;
        private double? _averageFitness;
        private TimeSpan? _gaTime;
        private TimeSpan? _routingTime;

        [Category("Schematic Information")]
        [DisplayName("Component Count")]
        public int? ComponentCount
        {
            get { return _componentCount; }
            set { SetProperty(nameof(ComponentCount), ref _componentCount, value); }
        }

        [Category("GA Information")]
        [DisplayName("Generation #")]
        public int? GenerationCount
        {
            get { return _generationNumber; }
            set { SetProperty(nameof(GenerationCount), ref _generationNumber, value); }
        }

        [Category("GA Information")]
        [DisplayName("Max Fitness")]
        public double? MaxFitness
        {
            get { return _maxFitness; }
            set { SetProperty(nameof(MaxFitness), ref _maxFitness, value); }
        }

        [Category("GA Information")]
        [DisplayName("Average Fitness")]
        public double? AverageFitness
        {
            get { return _averageFitness; }
            set { SetProperty(nameof(AverageFitness), ref _averageFitness, value); }
        }

        internal TimeSpan? GATime
        {
            get { return _gaTime; }
            set
            {
                _gaTime = value;
                NotifyPropertyChanged(nameof(GATimeDisplay));
            }
        }

        internal TimeSpan? RoutingTime
        {
            get { return _routingTime; }
            set
            {
                _routingTime = value;
                NotifyPropertyChanged(nameof(RoutingTimeDisplay));
            }
        }

        [Category("GA Information")]
        [DisplayName("GA Time")]
        public string GATimeDisplay => _gaTime?.ToString(@"mm\:ss\.ff") ?? "";

        [Category("Routing Information")]
        [DisplayName("Routing Time")]
        public string RoutingTimeDisplay => _routingTime?.ToString(@"mm\:ss\.ff") ?? "";

        [Category("PCB Information")]
        [DisplayName("Height (th)")]
        public int? Lebar
        {
            get { return _lebar; }
            set
            {
                SetProperty(nameof(Lebar), ref _lebar, value);
            }
        }

        [Category("PCB Information")]
        [DisplayName("Width (th)")]
        public int? Panjang
        {
            get { return _panjang; }
            set
            {
                SetProperty(nameof(Panjang), ref _panjang, value);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            var h = PropertyChanged;
            h?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetProperty<T>(string propertyName, ref T backingField, T value)
        {
            if (!value.Equals(backingField))
            {
                backingField = value;
                NotifyPropertyChanged(propertyName);
            }
        }
    }
}