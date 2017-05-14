namespace Factorio.Lua.Reader
{
    public class ItemSubGroupGroupEdge : EdgeBase
    {
        public ItemSubGroupGroupEdge(Storage storage, ItemSubGroup @from, ItemGroup to) : base(storage, @from, to)
        {
        }

        public ItemSubGroup ItemSubGroup => (ItemSubGroup) _from;
        public ItemGroup ItemGroup => (ItemGroup) _to;
    }
}