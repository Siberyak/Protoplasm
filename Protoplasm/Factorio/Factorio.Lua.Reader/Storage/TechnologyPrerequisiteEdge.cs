namespace Factorio.Lua.Reader
{
    public class TechnologyPrerequisiteEdge : EdgeBase
    {
        public TechnologyPrerequisiteEdge(Storage storage, Technology technology, Technology prerequisite) : base(storage, technology, prerequisite)
        {
        }

        public Technology Technology => (Technology) _from;
        public Technology Prerequisite => (Technology) _to;
    }
}