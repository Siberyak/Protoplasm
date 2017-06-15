using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;
using Protoplasm.Collections;

namespace Factorio.Lua.Reader
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CraftChain
    {
        private Calculation _calculation;

        PredicatedList<CraftInfo>  _crafts;

        [JsonConstructor]
        public CraftChain()
        {
        }

        public CraftChain(Calculation calculation)
        {
            Calculation = calculation;
        }

        [JsonProperty("calculation")]
        private Calculation Calculation
        {
            get { return _calculation; }
            set
            {
                _calculation = value;
                _crafts = new PredicatedList<CraftInfo>(_calculation.Crafts, x => x.Chain == this);
            }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class CraftInfo : BaseCalcInfo
    {

        [JsonProperty("chain")]
        public CraftChain Chain;

        private Storage _storage => Storage.Current;

        ICrafter _crafter;

        [JsonProperty("count")]
        private int _count = 1;

        internal Recipe Recipe;

        [JsonProperty("mode")]
        private ViewMode _mode = ViewMode.Crafter;

        [JsonProperty("enabled")]
        private bool _enabled;

        private Recipe _requestRecipe;

        [JsonProperty("location")]
        private Point _location;

        [JsonConstructor]
        public CraftInfo()
        {
        }

        [JsonProperty("recipe")]
        string RecipeID
        {
            get { return Recipe.Name; }
            set { SetRecipe(_storage.Recipies.First(x => x.Name == value)); }
        }

        [JsonProperty("crafter")]
        string CrafterID
        {
            get { return Crafter?.Name; }
            set { Crafter = _storage.Nodes.OfType<ICrafter>().FirstOrDefault(x => x.Name == value); }
        }

        public CraftInfo(Recipe recipe)
        {
            SetRecipe(recipe);
        }

        private void SetRecipe(Recipe recipe)
        {
            Recipe = recipe;
            _image = Recipe.Image32();
        }

        [Browsable(true)]
        public override Image Image => base.Image;

        public string Name => Recipe.LocalizedName;


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

                if (_crafter == null)
                    Enabled = false;

            }
        }
        [Browsable(false)]
        public double RecipeTime => Recipe._EnergyRequired;

        [Browsable(false)]
        public double? Speed => Crafter?._CraftingSpeed;

        [Browsable(false)]
        public double? CraftTime => Recipe._EnergyRequired/Speed;

        [Browsable(false)]
        public ViewMode Mode
        {
            get { return _mode; }
            set
            {
                if (value == _mode) return;
                _mode = value;
                OnPropertyChanged();
            }
        }

        [Browsable(false)]
        public bool Enabled
        {
            get { return _enabled && Crafter != null; }
            set
            {
                if (value == _enabled) return;
                _enabled = value;
                OnPropertyChanged();
            }
        }

        [Browsable(false)]
        public Recipe RequestRecipe
        {
            get { return _requestRecipe; }
            set
            {
                if (Equals(value, _requestRecipe)) return;
                _requestRecipe = value;
                if(_requestRecipe != null)
                    OnPropertyChanged();
            }
        }

        public Point Location
        {
            get { return _location; }
            set
            {
                if (value.Equals(_location)) return;
                _location = value;
                OnPropertyChanged();
            }
        }

        [Flags]
        public enum ViewMode
        {
            None = 0,
            Recipe = 1,
            Crafter = 2, 
            Count = 4,
            PerMinute = 8
        }


    }
}