using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace BoardCraft.Run.ViewModels
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Input;

    class MainWindowViewModel : ViewModelBase
    {
        private bool _libraryLoaded;
        private readonly List<GenericCommand> _canExecuteChangedOnLibraryLoaded;

        private IComponentRepository _repository;

        public MainWindowViewModel()
        {
            OpenedSchematics = new ObservableCollection<SchematicMetadataViewModel>();

            _canExecuteChangedOnLibraryLoaded = new List<GenericCommand>(5);

            var ofCommand = new GenericCommand(OpenFile, ()=>_libraryLoaded);
            OpenFileCommand = ofCommand;
            _canExecuteChangedOnLibraryLoaded.Add(ofCommand);

            LoadLibraryCommand = new GenericCommand(LoadRepository);
        }

        async void LoadRepository()
        {
            JsonFileLibrary cRepo = null;
            await Task.Run(() =>
            {
                cRepo = new JsonFileLibrary(@"..\..\Library");
                cRepo.Load();
            });

            _libraryLoaded = true;
            _repository = cRepo;

            foreach (var command in _canExecuteChangedOnLibraryLoaded)
            {
                command.RaiseCanExecuteChanged();
            }
        }

        void OpenFile()
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

        private SchematicMetadataViewModel _selectedItem;

        public ObservableCollection<SchematicMetadataViewModel> OpenedSchematics { get; }
        public SchematicMetadataViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem)
                    return;

                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public ICommand OpenFileCommand { get; }
        public ICommand LoadLibraryCommand { get; }
    }
}
