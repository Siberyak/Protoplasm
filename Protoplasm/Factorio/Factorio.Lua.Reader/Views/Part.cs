using System.ComponentModel;
using System.Linq;
using Protoplasm.Utils.Collections;

namespace Factorio.Lua.Reader
{
    public class Part : BaseCalcInfo
    {
        internal IRecipePart _part;
        private readonly PredicatedList<CraftInfo> _list;

        public Part(IRecipePart part, IBindingList<CraftInfo> crafts)
        {
            _part = part;
            _list = new PredicatedList<CraftInfo>(crafts, x => x._recpe.AllParts.Contains(part));
            _list.BeforeRemoveItem += BeforeRemove;
            _list.ListChanged += ListChanged;
            _image = _part?.Image32();
        }

        private void BeforeRemove(object sender, BeforeRemoveEventArgs e)
        {
            
        }

        private void ListChanged(object sender, ListChangedEventArgs e)
        {
        }

        public string LocalizedName
        {
            get { return _part.LocalizedName; }
        }

        public double Available { get; set; }
        public double Required { get; set; }
    }
}