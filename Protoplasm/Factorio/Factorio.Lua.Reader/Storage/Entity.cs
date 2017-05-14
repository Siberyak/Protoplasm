using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject("Prototype/Entity", MemberSerialization = MemberSerialization.OptIn)]
    public abstract class Entity: TypedNamedIconedBase, ILocalized
    {
        string ILocalized.Category => "entity-name";

        object[] ILocalized._LocalisedName => null;

        public override string LocalizedName => this.LocalisedName() ?? Name;

        public override string ToString()
        {
            return $"{Type}: '{LocalizedName}'" ?? base.ToString();
        }

        [JsonProperty("flags")]
        public object _Flags { get; set; }

        [JsonProperty("collision_box")]
        public object _CollisionBox { get; set; }

        [JsonProperty("selection_box")]
        public object _SelectionBox { get; set; }

        [JsonProperty("drawing_box")]
        public object _DrawingBox { get; set; }

        [JsonProperty("sticker_box")]
        public object _StickerBox { get; set; }

        [JsonProperty("weight")]
        public double _Weight { get; set; }

        [JsonProperty("minable")]
        public object _Minable { get; set; }

        [JsonProperty("emissions_per_tick")]
        public object _EmissionsPerTick { get; set; }

        [JsonProperty("fast_replaceable_group")]
        public string _FastReplaceableGroup { get; set; }

        [JsonProperty("tile_width")]
        public uint _TileWidth { get; set; }

        [JsonProperty("tile_height")]
        public uint _TileHeight { get; set; }

        [JsonProperty("autoplace")]
        public object _Autoplace { get; set; }

    }

    //[JsonObject("arrow", MemberSerialization = MemberSerialization.OptIn)]
    //public partial class Arrow : Entity
    //{ }

    //[JsonObject("corpse", MemberSerialization = MemberSerialization.OptIn)]
    //public partial class Corpse : Entity
    //{ }

    //[JsonObject("decorative", MemberSerialization = MemberSerialization.OptIn)]
    //public partial class Decorative : Entity
    //{ }

    //[JsonObject("explosion", MemberSerialization = MemberSerialization.OptIn)]
    //public partial class Explosion : Entity
    //{ }

    //[JsonObject("flame-thrower-explosion", MemberSerialization = MemberSerialization.OptIn)]
    //public partial class FlameThrowerExplosion : Explosion
    //{ }

    //[JsonObject("entity-with-health", MemberSerialization = MemberSerialization.OptIn)]
    //public abstract class EntityWithHealth : Entity
    //{
    //    [JsonProperty("max_health")]
    //    public double MaxHealth { get; set; }

    //    [JsonProperty("healing_per_tick")]
    //    public double HealingPerTick { get; set; }

    //    [JsonProperty("dying_explosion")]
    //    public object DyingExplosion { get; set; }

    //    [JsonProperty("loot")]
    //    public object Loot { get; set; }


    //    [JsonProperty("resistances")]
    //    public object Resistances { get; set; }

    //    [JsonProperty("corpse")]
    //    public object Corpse { get; set; }
    //}

    //[JsonObject("accumulator", MemberSerialization = MemberSerialization.OptIn)]
    //public partial class Accumulator : EntityWithHealth
    //{ }


    //[JsonObject("assembling-machine", MemberSerialization = MemberSerialization.OptIn)]
    //public partial class AssemblingMachine : EntityWithHealth
    //{ }

    //[JsonObject("beacon", MemberSerialization = MemberSerialization.OptIn)]
    //public partial class Beacon : EntityWithHealth
    //{ }

    //[JsonObject("car", MemberSerialization = MemberSerialization.OptIn)]
    //public partial class Car : EntityWithHealth
    //{ }

    //[JsonObject("player", MemberSerialization = MemberSerialization.OptIn)]
    //public partial class Player : EntityWithHealth
    //{ }


    //[JsonObject("container", MemberSerialization = MemberSerialization.OptIn)]
    //public partial class Container : EntityWithHealth
    //{ }


    //[JsonObject("smart-container", MemberSerialization = MemberSerialization.OptIn)]
    //public partial class SmartContainer : Container
    //{ }


    //[JsonObject("logistic-container", MemberSerialization = MemberSerialization.OptIn)]
    //public partial class LogisticContainer : SmartContainer
    //{ }


    //[JsonObject("electric-pole", MemberSerialization = MemberSerialization.OptIn)]
    //public partial class ElectricPole : EntityWithHealth
    //{ }


    //[JsonObject("fish", MemberSerialization = MemberSerialization.OptIn)]
    //public partial class Fish : EntityWithHealth
    //{ }

    //[JsonObject("furnace", MemberSerialization = MemberSerialization.OptIn)]
    //public partial class Furnace : EntityWithHealth
    //{ }
}