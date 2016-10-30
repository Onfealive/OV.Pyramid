using System;
using System.Collections.Generic;
using System.Text;

namespace OV.Core
{
    class YGOSet
    {
        public Version Version { get; private set; }
        public string Name { get; private set; }
        public List<YGOCard> Cards { get; private set; }
        public string Prefix { get; private set; }
        public SETTYPE Type { get; private set; }

        public YGOSet()
        {
            Version = new Version("0.1");
            Name = "Set Name";
            Cards = new List<YGOCard>();
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
