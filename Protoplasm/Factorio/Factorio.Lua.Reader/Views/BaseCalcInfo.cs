using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using Factorio.Lua.Reader.Annotations;
using Protoplasm.Collections;

namespace Factorio.Lua.Reader
{
    public abstract class BaseCalcInfo : INotifyPropertyChanged, IBindingListItem
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [Browsable(false)]
        public virtual Image Image => _image;

        protected Image _image;

        public event EventHandler Deleted;
        public void OnDeleted()
        {
            Deleted?.Invoke(this, EventArgs.Empty);
        }
    }
}