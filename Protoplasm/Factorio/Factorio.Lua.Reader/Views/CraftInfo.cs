using System.ComponentModel;
using System.Drawing;

namespace Factorio.Lua.Reader
{
    public class CraftInfo : BaseCalcInfo
    {
        private Storage _storage => Storage.Current;

        ICrafter _crafter;
        private int _count = 1;
        internal Recipe _recpe;

        public CraftInfo(Recipe recpe)
        {
            _recpe = recpe;
            _image = _recpe.Image32();
        }

        public string Name => _recpe.LocalizedName;


        public int Count
        {
            get { return _count; }
            set
            {
                if (value == _count) return;
                _count = value;
                OnPropertyChanged();
            }
        }

        //[Browsable(false)]
        public ICrafter Crafter
        {
            get { return _crafter; }
            set
            {
                if (Equals(value, _crafter)) return;
                _crafter = value;
                OnPropertyChanged();
            }
        }
    }
}