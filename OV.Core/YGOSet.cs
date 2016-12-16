using System;
using System.Collections.Generic;

namespace OV.Core
{
    class YgoSet
    {
        public Version Version { get; private set; }
        public string Name { get; private set; }
        public List<YgoCard> Cards { get; private set; }
        public string Prefix { get; private set; }
        public SETTYPE Type { get; private set; }

        public YgoSet()
        {
            Version = new Version("0.1");
            Name = "Set Name";
            Cards = new List<YgoCard>();
            Prefix = "";
            Type = SETTYPE.BoosterPack;
        }

        internal void SetName(string name)
        {
            Name = name;
        }
    }

    enum SETTYPE
    {
        BoosterPack
    }

    
}
