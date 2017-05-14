namespace Factorio.Lua.Reader
{
    public class ItemSubGroupEdge : EdgeBase
    {
        public ItemSubGroupEdge(Storage storage, Item @from, ItemSubGroup to) : base(storage, @from, to)
        {
        }

        public Item Item => (Item) _from;
        public ItemSubGroup ItemSubGroup => (ItemSubGroup) _to;
    }
}