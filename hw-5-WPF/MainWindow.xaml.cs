using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace hw_5_WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = (NotebookViewModel)Resources["NotebookViewModel"];
        }
    }

    public class NotebookViewModel : INotifyPropertyChanged
    {
        private Record _selectedRecord;
        private Record _newRecord = new Record();
        private bool _isDataChanged = false;

        public ObservableCollection<Record> Records { get; set; } = new ObservableCollection<Record>();

        public Record SelectedRecord
        {
            get => _selectedRecord;
            set
            {
                if (_selectedRecord != value)
                {
                    _selectedRecord = value;
                    OnPropertyChanged();
                    UpdateButtonStates();
                }
            }
        }

        public Record NewRecord
        {
            get => _newRecord;
            set
            {
                if (_newRecord != value)
                {
                    _newRecord.PropertyChanged -= OnNewRecordPropertyChanged;
                    _newRecord = value;
                    _newRecord.PropertyChanged += OnNewRecordPropertyChanged;
                    OnPropertyChanged();
                    UpdateButtonStates();
                }
            }
        }

        public ICommand AddRecordCommand { get; }
        public ICommand DeleteRecordCommand { get; }
        public ICommand SaveToFileCommand { get; }
        public ICommand LoadFromFileCommand { get; }

        public bool CanAddRecord => NewRecord != null &&
                                     !string.IsNullOrWhiteSpace(NewRecord.Name) &&
                                     !string.IsNullOrWhiteSpace(NewRecord.Address) &&
                                     !string.IsNullOrWhiteSpace(NewRecord.Phone) &&
                                     !string.IsNullOrWhiteSpace(NewRecord.Email);

        public bool CanDeleteRecord => SelectedRecord != null;

        public bool CanSaveRecords => Records.Count > 0;

        public NotebookViewModel()
        {
            AddRecordCommand = new RelayCommand(AddRecord);
            DeleteRecordCommand = new RelayCommand(DeleteRecord);
            SaveToFileCommand = new RelayCommand(SaveToFile);
            LoadFromFileCommand = new RelayCommand(LoadFromFile);

            NewRecord.PropertyChanged += OnNewRecordPropertyChanged;
        }

        private void AddRecord()
        {
            if (CanAddRecord)
            {
                Records.Add(new Record
                {
                    Name = NewRecord.Name,
                    Address = NewRecord.Address,
                    Phone = NewRecord.Phone,
                    Email = NewRecord.Email
                });
                _isDataChanged = true;
                NewRecord = new Record();
                UpdateButtonStates();
            }
        }

        private void OnNewRecordPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Record.Name) ||
                e.PropertyName == nameof(Record.Address) ||
                e.PropertyName == nameof(Record.Phone) ||
                e.PropertyName == nameof(Record.Email))
            {
                UpdateButtonStates();
            }
        }

        private void DeleteRecord()
        {
            if (SelectedRecord != null && CanDeleteRecord)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить запись?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Records.Remove(SelectedRecord);
                    _isDataChanged = true;
                    SelectedRecord = null;
                    UpdateButtonStates();
                }
            }
        }

        private void SaveToFile()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, JsonConvert.SerializeObject(Records));
                    _isDataChanged = false;
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadFromFile()
        {
            if (Records.Count > 0)
            {
                var result = MessageBox.Show("В таблице уже есть данные. Хотите сохранить их перед загрузкой?", "Подтверждение загрузки", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    SaveToFile();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var fileContent = File.ReadAllText(openFileDialog.FileName);
                    var loadedRecords = JsonConvert.DeserializeObject<ObservableCollection<Record>>(fileContent);
                    if (loadedRecords != null)
                    {
                        Records.Clear();
                        foreach (var record in loadedRecords)
                        {
                            Records.Add(record);
                        }
                        _isDataChanged = false;
                        UpdateButtonStates();
                    }
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Ошибка при загрузке файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (System.Text.Json.JsonException ex)
                {
                    MessageBox.Show($"Ошибка в формате файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UpdateButtonStates()
        {
            OnPropertyChanged(nameof(CanAddRecord));
            OnPropertyChanged(nameof(CanDeleteRecord));
            OnPropertyChanged(nameof(CanSaveRecords));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Record : INotifyPropertyChanged
    {
        private string _name;
        private string _address;
        private string _phone;
        private string _email;

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Address
        {
            get => _address;
            set
            {
                if (_address != value)
                {
                    _address = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Phone
        {
            get => _phone;
            set
            {
                if (_phone != value)
                {
                    _phone = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;

        public RelayCommand(Action execute)
        {
            _execute = execute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => _execute();
    }
}