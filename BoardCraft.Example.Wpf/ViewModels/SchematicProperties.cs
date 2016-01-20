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

        [Category("Informasi GA")]
        [DisplayName("Fitness Terbesar")]
        [Description("Nilai fitness terbesar pada populasi saat ini")]
        public double? MaxFitness
        {
            get { return _maxFitness; }
            set { SetProperty(nameof(MaxFitness), ref _maxFitness, value); }
        }

        [Category("Informasi GA")]
        [DisplayName("Fitness Rata-rata")]
        [Description("Rata-rata nilai fitness pada populasi saat ini")]
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

        [Category("Informasi GA")]
        [DisplayName("Waktu GA")]
        [Description("Waktu yang telah dihabiskan untuk melakukan proses penempatan dengan GA")]
        public string GATimeDisplay => _gaTime?.ToString(@"mm\:ss\.ff") ?? "";

        [Category("Informasi Routing")]
        [DisplayName("Waktu Routing")]
        [Description("Waktu yang telah dihabiskan untuk melakukan proses routing")]
        public string RoutingTimeDisplay => _routingTime?.ToString(@"mm\:ss\.ff") ?? "";

        [Category("Informasi PCB")]
        [DisplayName("Lebar")]
        [Description("Lebar PCB dalam satuan seperseribu inch")]
        public int? Lebar
        {
            get { return _lebar; }
            set
            {
                SetProperty(nameof(Lebar), ref _lebar, value);
            }
        }

        [Category("Informasi PCB")]
        [DisplayName("Panjang")]
        [Description("Panjang PCB dalam satuan seperseribu inch")]
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