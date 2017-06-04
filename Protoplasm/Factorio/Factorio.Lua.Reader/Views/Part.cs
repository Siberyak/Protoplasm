using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Protoplasm.Collections;

namespace Factorio.Lua.Reader
{
    public class Part : BaseCalcInfo
    {
        internal IRecipePart _part;
        private readonly PredicatedList<CraftInfo> _list;
        private double _available;
        private double _required;
        private bool _enabled;

        public Part(IRecipePart part, IBindingList<CraftInfo> crafts)
        {
            _part = part;
            _list = new PredicatedList<CraftInfo>(crafts, x => x.Enabled && x.Recipe.AllParts.Contains(part) && x.Enabled);
            _list.BeforeRemoveItem += BeforeRemove;
            _list.ListChanged += ListChanged;
            _image = _part?.Image32();
            Recalc();
        }

        private void Recalc()
        {
            var tmp = _list.Select
                (
                    x => new
                    {
                        CraftInfo = x,
                        Outs = x.Recipe.References.OfType<RecipePartEdge>().Where(y => y.Direction == Direction.Output && y.Part == _part).ToArray(),
                        Ins = x.Recipe.References.OfType<RecipePartEdge>().Where(y => y.Direction == Direction.Input && y.Part == _part).ToArray()
                    }
                )
                .Where(x => x.CraftInfo.CraftTime.HasValue)
                .Select(x => new {x.CraftInfo, x.Ins, x.Outs, Count = 60.0 / x.CraftInfo.CraftTime.Value })
                .ToArray();

            Enabled = tmp.Any(x => x.Ins.Length != 0 || x.Outs.Length != 0);
            Required = tmp.Sum(x => x.CraftInfo.Count * x.Count * x.Ins.Sum(y => y.Amount));
            Available = tmp.Sum(x => x.CraftInfo.Count * x.Count * x.Outs.Sum(y => y.Amount));
        }

        [Browsable(false)]
        public bool Enabled
        {
            get { return _enabled; }
            private set
            {
                if (value == _enabled) return;
                _enabled = value;
                OnPropertyChanged();
            }
        }

        private void BeforeRemove(object sender, BeforeRemoveEventArgs e)
        {
            
        }

        private void ListChanged(object sender, ListChangedEventArgs e)
        {
            Recalc();
        }

        public string Name => _part.LocalizedName;

        public double Available
        {
            get { return _available; }
            set
            {
                if (value.Equals(_available)) return;
                _available = value;
                OnPropertyChanged();
            }
        }

        public double Required
        {
            get { return _required; }
            set
            {
                if (value.Equals(_required)) return;
                _required = value;
                OnPropertyChanged();
            }
        }
    }
}