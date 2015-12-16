namespace BoardCraft.Example.Wpf.ViewModels
{
    using System.ComponentModel;
    using Models;
    using Placement.GA;

    internal class SchematicProperties : INotifyPropertyChanged
    {
        private int? _componentCount;
        private int? _generationNumber;
        private double? _maxFitness;
        private double? _averageFitness;

        [Category("Informasi Skematik")]
        [DisplayName("Jumlah Komponen")]
        [Description("Jumlah komponen yang terdapat pada skema elektronika")]
        public int? ComponentCount
        {
            get { return _componentCount; }
            set { SetProperty(nameof(ComponentCount), ref _componentCount, value); }
        }

        [Category("Informasi GA")]
        [DisplayName("Generasi ke")]
        [Description("Banyaknya iterasi yang telah ditempuh oleh algoritma genetika")]
        public int? GenerationCount
        {
            get { return _generationNumber; }
            set { SetProperty(nameof(GenerationCount), ref _generationNumber, value); }
        }

        public double? MaxFitness
        {
            get { return _maxFitness; }
            set { SetProperty(nameof(MaxFitness), ref _maxFitness, value); }
        }

        public double? AverageFitness
        {
            get { return _averageFitness; }
            set { SetProperty(nameof(AverageFitness), ref _averageFitness, value); }
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