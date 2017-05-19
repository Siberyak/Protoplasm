using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Protoplasm.Graph;

namespace Factorio.Lua.Reader
{
    [JsonObject("item", MemberSerialization = MemberSerialization.OptIn)]
    public partial class Item : TypedNamedIconedBase, ILocalized, IRecipePart
    {
        public virtual string Category => "item-name";
        public override string LocalizedName => this.LocalisedName()
            ?? PlaceResult?.LocalisedName()
            ?? PlaceAsEquipmentResult?.LocalisedName()
            ?? Name;

        public Entity PlaceResult => References.OfType<PlaceResultEdge>().FirstOrDefault()?.Entity;
        public Equipment PlaceAsEquipmentResult => References.OfType<PlaceAsEquipmentResultEdge>().FirstOrDefault()?.Equipment;
        public Tile PlaceAsTileResult => References.OfType<PlaceAsTileResultEdge>().FirstOrDefault()?.Tile;

        private string _subgroup;

        [JsonProperty("flags")]
        public object _Flags { get; set; }

        [JsonProperty("order")]
        public string _Order { get; set; }

        [JsonProperty("stack_size")]
        public uint _StackSize { get; set; }


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

        [JsonProperty("place_as_tile")]
        public object _PlaceAsTile { get; set; }

        [JsonProperty("icons")]
        public IconInfo[] Icons { get; set; }

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

            if (!string.IsNullOrEmpty(_PlacedAsEquipmentResult))
            {
                var equipment = Storage.Nodes<Equipment>(x => x.Name == _PlaceResult).FirstOrDefault();
                if (equipment != null)
                {
                    Storage.Link<PlaceAsEquipmentResultEdge>(this, equipment);
                }
            }

            if (_PlaceAsTile != null)
            {
                var result = ((JObject) _PlaceAsTile)["result"].Value<string>();
                var tile = Storage.Nodes<Tile>(x => x.Name == result).FirstOrDefault();
                if (tile != null)
                {
                    Storage.Link<PlaceAsTileResultEdge>(this, tile);
                }
            }
        }

    }
}