using System.ComponentModel;
using System.Drawing;

namespace Factorio.Lua.Reader
{
    public class CraftInfo : BaseCalcInfo
    {
        private Storage _storage => Storage.Current;

        ICrafter _crafter;
        private int _count = 1;
        internal Recipe _recipe;

        public CraftInfo(Recipe recipe)
        {
            _recipe = recipe;
            _image = _recipe.Image32();
        }

        public string Name => _recipe.LocalizedName;


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

        [Browsable(false)]
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
        public double RecipeTime => _recipe._EnergyRequired;

        public double? Speed => Crafter?._CraftingSpeed;

        public double? CraftTime => _recipe._EnergyRequired/Speed;
    }
}