namespace Factorio.Lua.Reader
{
    public interface ILocalized
    {
        string Category { get; }
        Storage Storage { get; }
        string Name { get; }
        object[] _LocalisedName { get; }

        string LocalizedName { get; }
    }
}