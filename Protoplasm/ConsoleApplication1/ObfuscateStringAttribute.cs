namespace ConsoleApplication1
{
    public class ObfuscateStringAttribute : ObfuscateAttribute
    {
        public string Format { get; set; }

        protected override object GetObfuscatedValue(ObfuscationSequenceAttribute sequence, object data, object originalValue)
        {
            if (string.IsNullOrEmpty((string)originalValue))
                return originalValue;

            var dataDisplayName = sequence.DataDisplayName(data);

            var format = Format ?? "{0} ¹ {1}";
            return string.Format(format, dataDisplayName, sequence.Current, originalValue);
        }
    }
}