namespace Factorio.Lua.Reader
{
    public class Language
    {
        private string localName;
        public string Name;

        public string LocalName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(localName))
                {
                    return localName;
                }
                return Name;
            }
            set { localName = value; }
        }
    }
}