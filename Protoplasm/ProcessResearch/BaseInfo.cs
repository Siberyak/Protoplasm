using System;
using System.ComponentModel;

namespace ProcessResearch
{
    public abstract class BaseInfo
    {
        [Browsable(false)]
        public Guid ID { get; set; }

        public string Caption { get; set; }
    }
}