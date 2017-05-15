using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using Factorio.Lua.Reader.Annotations;

namespace Factorio.Lua.Reader
{
    public abstract class BaseCalcInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [Browsable(false)]
        public Image Image => _image;

        protected Image _image;
    }
}