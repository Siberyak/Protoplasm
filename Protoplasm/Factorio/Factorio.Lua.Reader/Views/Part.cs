using System.ComponentModel;
using System.Linq;
using DevExpress.XtraLayout.Converter;
using Protoplasm.Utils.Collections;

namespace Factorio.Lua.Reader
{
    public class Part : BaseCalcInfo
    {
        internal IRecipePart _part;
        private readonly PredicatedList<CraftInfo> _list;
        private double _available;
        private double _required;

        public Part(IRecipePart part, IBindingList<CraftInfo> crafts)
        {
            _part = part;
            _list = new PredicatedList<CraftInfo>(crafts, x => x._recipe.AllParts.Contains(part) && x.Speed.HasValue);
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
                        x,
                        Outs = x._recipe.References.OfType<RecipePartEdge>().Where(y => y.Direction == Direction.Output && y.Part == _part).ToArray(),
                        Ins = x._recipe.References.OfType<RecipePartEdge>().Where(y => y.Direction == Direction.Input && y.Part == _part).ToArray()
                    }
                ).ToArray();
        }

        private void BeforeRemove(object sender, BeforeRemoveEventArgs e)
        {
            
        }

        private void ListChanged(object sender, ListChangedEventArgs e)
        {
            Recalc();
        }

        public string LocalizedName
        {
            get { return _part.LocalizedName; }
        }

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