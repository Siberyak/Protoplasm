namespace ConsoleApplication1.TestData
{
    public abstract class Requirement : BaseRequirement
    {
        protected Requirement(MappingType mappingType)
        {
            MappingType = mappingType;
        }

        public MappingType MappingType { get; private set; }

    }
}