namespace Factorio.Lua.Reader
{
    public class PlaceResultEdge : EdgeBase
    {
        public PlaceResultEdge(Storage storage, Item item, Entity entity) : base(storage, item, entity)
        {
        }

        public Item Item => (Item) _from;
        public Entity Entity => (Entity) _to;
    }
}