using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Protoplasm.Utils.Collections;

namespace Factorio.Lua.Reader
{
    public class Calculation
    {
        private Storage _storage => Storage.Current;
        public IEnumerable<CraftInfo> Crafts => _crafts;
        public IEnumerable<Part> Parts => _parts;

        DataList<CraftInfo> _crafts = new DataList<CraftInfo>();
        DataList<Part> _parts = new DataList<Part>();

        public Calculation()
        {
            _crafts.BeforeRemoveItem += BeforeRemoveCrafts;
            _crafts.ListChanged += CraftsListChanged;
        }

        private void BeforeRemoveCrafts(object sender, BeforeRemoveEventArgs e)
        {
            
        }

        private void CraftsListChanged(object sender, ListChangedEventArgs e)
        {
            var parts = _crafts.SelectMany(x => x._recpe.AllParts).Distinct().Except(_parts.Select(x => x._part)).ToArray();
            foreach (var part in parts)
            {
                _parts.Add(new Part(part, _crafts));
            }
        }

        public void Add(Recipe recipe)
        {
            if (_crafts.Any(x => x._recpe == recipe))
                return;

            _crafts.Add(new CraftInfo(recipe));
        }
    }
}