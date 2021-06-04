using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Laser.Commands;

namespace Laser
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly Dispatcher _dispatcher;
        private readonly Image[] _imagesArray;

        private readonly ProjectorManager _manager = new();
        private string _selectedPort;

        public IList<Image> Images => _imagesArray;
        public CommandEnum SelectedImage
        {
            get => _manager.SelectedImage;
            set => _manager.SelectedImage = value;
        }
        public IList<string> PortNames => SerialPort.GetPortNames();

        public string StatusString => _manager.Status switch
        {
            ProjectorStatus.Disconnected => "Нет подключения",
            ProjectorStatus.Processing => "Обработка",
            ProjectorStatus.Ready => "Готов",
            _ => throw new ArgumentOutOfRangeException(),
        };

        public Brush StatusColor => _manager.Status.GetColor();

        public string SelectedPort
        {
            get => _selectedPort;
            set
            {
                _selectedPort = value;

                _manager.SetPort(_selectedPort);
            }
        }

        public ICommand EnabledProjectorCommand { get; }
        public ICommand DisabledProjectorCommand { get; }

        public ICommand EnabledLaserCommand { get; }
        public ICommand DisabledLaserCommand { get; }

        public ICommand StoppedCommand { get; }
        public ICommand PlayingCommand { get; }

        public bool ProjectorDisconnected => !_manager.ProjectorConnected;

        public MainWindowViewModel()
        {
            EnabledProjectorCommand = new RelayCommand(RunCommand, _ => _manager.ProjectorConnected);
            DisabledProjectorCommand = new RelayCommand(RunCommand, _ => !_manager.ProjectorConnected && SelectedPort is not null);

            EnabledLaserCommand = new RelayCommand(RunCommand, _ => _manager.ProjectorConnected && _manager.IsLaserEnabled);
            DisabledLaserCommand = new RelayCommand(RunCommand, _ => _manager.ProjectorConnected && !_manager.IsLaserEnabled);

            StoppedCommand = new RelayCommand(RunCommand, _ => _manager.ProjectorConnected && !_manager.IsImagePlaying);
            PlayingCommand = new RelayCommand(RunCommand, _ => _manager.ProjectorConnected && _manager.IsImagePlaying);

            _imagesArray = new Image[]
                           {
                               new(CommandEnum.Image00, "(нет)", this),
                               new(CommandEnum.Image01, "Квадрат", this),
                               new(CommandEnum.Image02, "Треугольник", this),
                               new(CommandEnum.Image03, "Шестиугольник", this),
                               new(CommandEnum.Image04, "Пятиконечая звезда", this),
                               //new(CommandEnum.Image05, "Спираль", this),
                           };

            _manager.StatusUpdated += () =>
            {
                OnPropertyChanged(nameof(StatusString));
                OnPropertyChanged(nameof(StatusColor));
            };

            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RunCommand(object parameter)
        {
            try
            {
                _manager.Execute((CommandEnum)parameter, Update);
            }
            catch (InvalidOperationException e)
            {
                MessageBox.Show(e.Message, e.ToString());
            }
            catch (TimeoutException e)
            {
                MessageBox.Show(e.Message, e.ToString());
            }
        }

        public void Update()
        {
            _dispatcher.Invoke(() =>
            {
                OnPropertyChanged(nameof(SelectedImage));
                OnPropertyChanged(nameof(ProjectorDisconnected));
                OnPropertyChanged(nameof(StatusString));

                OnPropertyChanged(nameof(EnabledProjectorCommand));
                OnPropertyChanged(nameof(DisabledProjectorCommand));
                OnPropertyChanged(nameof(EnabledLaserCommand));
                OnPropertyChanged(nameof(DisabledLaserCommand));
                OnPropertyChanged(nameof(StoppedCommand));
                OnPropertyChanged(nameof(PlayingCommand));
            });
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Closed()
        {
            if (EnabledProjectorCommand.CanExecute(default)) EnabledProjectorCommand.Execute(CommandEnum.Detach);
        }

        public record Image(CommandEnum CommandEnumType, string Text, MainWindowViewModel ViewModel)
        {
            public bool IsChecked
            {
                get => ViewModel.SelectedImage == CommandEnumType;
                set => ViewModel.SelectedImage = value ? CommandEnumType : default;
            }
        }
    }
}
