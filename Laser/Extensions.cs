using System;
using System.Windows.Media;

namespace Laser
{
    public static class Extensions
    {
        public static Brush GetColor(this ProjectorStatus status) => status switch {
            ProjectorStatus.Disconnected => new SolidColorBrush(System.Windows.Media.Color.FromRgb(255,0,0)),
            ProjectorStatus.Processing => new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 215, 0)),
            ProjectorStatus.Ready => new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 128, 0)),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}
