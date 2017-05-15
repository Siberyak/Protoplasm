namespace Factorio.Lua.Reader
{
    public class PlaceAsEquipmentResultEdge : EdgeBase
    {
        public PlaceAsEquipmentResultEdge(Storage storage, Item item, Equipment equipment) : base(storage, item, equipment)
        {
        }

        public Item Item => (Item)_from;
        public Equipment Equipment => (Equipment)_to;
    }

    public class PlaceAsTileResultEdge : EdgeBase
    {
        public PlaceAsTileResultEdge(Storage storage, Item item, Tile tile) : base(storage, item, tile)
        {
        }

        public Item Item => (Item)_from;
        public Tile Tile => (Tile)_to;
    }
}