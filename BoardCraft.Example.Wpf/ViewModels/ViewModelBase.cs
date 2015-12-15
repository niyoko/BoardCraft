namespace BoardCraft.Example.Wpf.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    internal class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}