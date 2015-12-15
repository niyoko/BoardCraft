namespace BoardCraft.Example.Wpf.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Input;
    using Microsoft.Win32;

    internal class MainWindowViewModel : ViewModelBase
    {
        private readonly List<GenericCommand> _canExecuteChangedOnLibraryLoaded;
        private bool _libraryLoaded;

        private IComponentRepository _repository;

        private SchematicMetadataViewModel _selectedItem;

        public MainWindowViewModel()
        {
            OpenedSchematics = new ObservableCollection<SchematicMetadataViewModel>();

            _canExecuteChangedOnLibraryLoaded = new List<GenericCommand>(5);

            var openFileCmd = new GenericCommand(OpenFile, () => _libraryLoaded);
            OpenFileCommand = openFileCmd;
            _canExecuteChangedOnLibraryLoaded.Add(openFileCmd);

            LoadLibraryCommand = new GenericCommand(LoadRepository);
        }

        public ObservableCollection<SchematicMetadataViewModel> OpenedSchematics { get; }

        public SchematicMetadataViewModel SelectedItem
        {
            get
            {
                return _selectedItem;
            }

            set
            {
                if (value == _selectedItem)
                {
                    return;
                }

                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public ICommand OpenFileCommand { get; }

        public ICommand LoadLibraryCommand { get; }

        private async void LoadRepository()
        {
            JsonFileLibrary repo = null;
            await Task.Run(() =>
            {
                repo = new JsonFileLibrary(@"..\..\Library");
                repo.Load();
            });

            _libraryLoaded = true;
            _repository = repo;

            foreach (var command in _canExecuteChangedOnLibraryLoaded)
            {
                command.RaiseCanExecuteChanged();
            }
        }

        private void OpenFile()
        {
            var o = new OpenFileDialog
            {
                DefaultExt = ".json",
                Filter = "JSON Files (*.json)|*json",
                Multiselect = false,
                CheckFileExists = true
            };

            var r = o.ShowDialog();
            if (r == true)
            {
                var fn = o.FileName;
                var prs = new JsonInputParser(_repository);

                using (var file = File.OpenRead(fn))
                {
                    var sch = prs.Parse(file);
                    var sch2 = new SchematicMetadataViewModel(sch, Path.GetFileNameWithoutExtension(fn));

                    OpenedSchematics.Add(sch2);
                    SelectedItem = sch2;
                }
            }
        }
    }
}