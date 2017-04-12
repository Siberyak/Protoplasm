namespace ConsoleApplication1.TestData
{
    public class Requirement : BaseRequirement
    {

        public Requirement(MappingType mappingType)
        {
            MappingType = mappingType;
        }

        public MappingType MappingType { get; private set; }

        protected override ConformResult Conformable(BaseAbility ability)
        {
            return ConformResult.Empty;
        }
    }
}