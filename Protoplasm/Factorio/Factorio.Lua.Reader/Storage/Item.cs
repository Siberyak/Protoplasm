using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Protoplasm.Utils.Graph;

namespace Factorio.Lua.Reader
{
    [JsonObject("item", MemberSerialization = MemberSerialization.OptIn)]
    public partial class Item : TypedNamedBase, ILocalized
    {
        string ILocalized.Category => "item-name";
        public override string LocalizedName => this.LocalisedName() ?? PlaceResult?.LocalisedName() ?? Name;
        public override string ToString()
        {
            return $"{Type}: '{LocalizedName}'" ?? base.ToString();
        }

        public Entity PlaceResult => References.OfType<PlaceResultEdge>().FirstOrDefault()?.Entity;

        private string _subgroup;

        [JsonProperty("flags")]
        public object _Flags { get; set; }

        [JsonProperty("order")]
        public string _Order { get; set; }

        [JsonProperty("stack_size")]
        public uint _StackSize { get; set; }

        [JsonProperty("icon")]
        public string _Icon { get; set; }

        [JsonProperty("subgroup")]
        public string _Subgroup
        {
            get { return _subgroup ?? Type; }
            set { _subgroup = value; }
        }

        [JsonProperty("place_result")]
        public string _PlaceResult { get; set; }

        [JsonProperty("fuel_value")]
        public string _FuelValue { get; set; }

        [JsonProperty("healing_value")]
        public double _HealingValue { get; set; }

        [JsonProperty("localised_name")]
        public object[] _LocalisedName { get; set; }
        public override void ProcessLinks()
        {
            base.ProcessLinks();

            var subGroup = Storage.FindNode<ItemSubGroup>(x => x.Name == _Subgroup);

            Storage.Link<ItemSubGroupEdge>(this, subGroup);

            if (!string.IsNullOrEmpty(_PlaceResult))
            {
                var entity = Storage.Nodes<Entity>(x => x.Name == _PlaceResult).FirstOrDefault();
                if (entity != null)
                {
                    Storage.Link<PlaceResultEdge>(this, entity);
                }
            }
        }

    }
}