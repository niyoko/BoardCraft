namespace BoardCraft.Example.Wpf.ViewModels
{
    using System.ComponentModel;
    using Output.Wpf;

    public class OutputProperties : INotifyPropertyChanged
    {
        private ColorPallete _pallete;
        private bool _component, _bottomCopper, _topCopper, _pad, _drillHole, _via, _boardEdge;

        public OutputProperties()
        {
            _component = true;
            _bottomCopper = true;
            _topCopper = true;
            _pad = true;
            _drillHole = true;
            _via = true;
            _boardEdge = true;
        }

        [Category("Color Setting")]
        [DisplayName("Color Pallete")]
        [Description("The color pallete used to display PCB")]
        public ColorPallete ColorPallete
        {
            get { return _pallete; }
            set { SetProperty(nameof(ColorPallete), ref _pallete, value); }
        }

        [Category("Layers")]
        [DisplayName("Component")]
        public bool Component
        {
            get { return _component; }
            set { SetProperty(nameof(Component), ref _component, value);}
        }

        [Category("Layers")]
        [DisplayName("Bottom Copper")]
        public bool BottomCopper
        {
            get { return _bottomCopper; }
            set { SetProperty(nameof(BottomCopper), ref _bottomCopper, value); }
        }

        [Category("Layers")]
        [DisplayName("Top Copper")]
        public bool TopCopper
        {
            get { return _topCopper; }
            set { SetProperty(nameof(TopCopper), ref _topCopper, value); }
        }

        [Category("Layers")]
        [DisplayName("Pad")]
        public bool Pad
        {
            get { return _pad; }
            set { SetProperty(nameof(Pad), ref _pad, value); }
        }

        [Category("Layers")]
        [DisplayName("Drill Hole")]
        public bool DrillHole
        {
            get { return _drillHole; }
            set { SetProperty(nameof(DrillHole), ref _drillHole, value); }
        }

        [Category("Layers")]
        [DisplayName("Via")]
        public bool Via
        {
            get { return _via; }
            set { SetProperty(nameof(Via), ref _via, value); }
        }

        [Category("Layers")]
        [DisplayName("Board Edge")]
        public bool BoardEdge
        {
            get { return _boardEdge; }
            set { SetProperty(nameof(BoardEdge), ref _boardEdge, value); }
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
