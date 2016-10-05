using System;
using System.Collections.Generic;
using System.Text;

namespace OV.Core
{
    class YGOSet
    {
        public string Name;
        public List<YGOCard> Cards;
        public string Prefix;
        public SETTYPE Type;

        public YGOSet()
        {
            Name = "";
            Cards = new List<YGOCard>();
            Prefix = "";
            Type = SETTYPE.BoosterPack;
        }
    }

    enum SETTYPE
    {
        BoosterPack
    }
}
