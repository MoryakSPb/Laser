using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Laser
{
    public class ProjectorManager
    {
        private ProjectorStatus _status = ProjectorStatus.Disconnected;
        public SerialPort Port { get; private set; }

        public bool ProjectorConnected { get; set; }
        public bool IsLaserEnabled { get; set; }
        public bool IsImagePlaying { get; set; }

        private readonly object _executeSyncRoot = new ();

        public ProjectorStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                StatusUpdated?.Invoke();
            }
        }
        public event Action StatusUpdated; 

        public CommandEnum SelectedImage { get; set; } = CommandEnum.Image00;

        public void SetPort(object parameter)
        {
            try
            {
                string portName = parameter?.ToString();
                if (string.IsNullOrWhiteSpace(portName)) return;
                Port?.Close();
                Port = new SerialPort
                       {
                           PortName = portName,
                           BaudRate = 9600,
                       };
                Port.Open();
            }
            catch
            {
                Status = ProjectorStatus.Disconnected;
                throw;
            }


        }

        public void Execute(CommandEnum parameter, Action update)
        {
            lock (_executeSyncRoot)
            {
                Status = ProjectorStatus.Processing;
                update?.Invoke();
                byte[] bytes = {
                                   (byte)parameter
                               };

                Port.Write(bytes, 0, bytes.Length);
                int answer = Port.ReadByte();
                if (answer != (int)CommandEnum.Ack)
                {
                    Status = ProjectorStatus.Disconnected;
                    throw new Exception("Некорректный ответ от устройства");
                }
                switch (parameter)
                {
                    case >= CommandEnum.Image00 and <= CommandEnum.Image15:
                        SelectedImage = parameter;
                        break;
                    case CommandEnum.ExecStart:
                        IsImagePlaying = true;
                        break;
                    case CommandEnum.ExecStop:
                        IsImagePlaying = false;
                        break;
                    case CommandEnum.LaserOff:
                        IsLaserEnabled = false;
                        break;
                    case CommandEnum.LaserOn:
                        IsLaserEnabled = true;
                        break;
                    case CommandEnum.Init:
                        Execute(CommandEnum.LaserOff, update);
                        Execute(CommandEnum.ExecStop, update);
                        Execute(SelectedImage, update);
                        ProjectorConnected = true;
                        break;
                    case CommandEnum.Detach:
                        Execute(CommandEnum.LaserOff, update);
                        Execute(CommandEnum.ExecStop, update);
                        Execute(SelectedImage, update);
                        ProjectorConnected = false;
                        Port.Close();
                        Port = null;
                        Status = ProjectorStatus.Disconnected;
                        update?.Invoke();
                        return;
                }
                Status = ProjectorStatus.Ready;
                update?.Invoke();
            }
        }
    }
}
