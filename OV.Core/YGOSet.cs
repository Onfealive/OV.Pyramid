using JsonNet.PrivateSettersContractResolvers;
using Newtonsoft.Json;
using OV.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public YgoSet Clone()
        {
            YgoSet newSet = new YgoSet();
            newSet.Version = this.Version;
            newSet.Name = this.Name;
            newSet.Prefix = this.Prefix;
            newSet.Type = this.Type;
            
            foreach(YgoCard card in this.Cards)
            {
                newSet.Cards.Add(card.Clone());
            }
            return newSet;
        }

        public static YgoSet LoadFrom(string fileName)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new PrivateSetterContractResolver(),
            };
            return JsonConvert.DeserializeObject<YgoSet>(File.ReadAllText(fileName), settings);
        }

        internal void SetName(string name)
        {
            Name = name;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null) return false;
            var other = obj as YgoSet;
            if (other == null) return false;

            if (other.Name == Name
                && other.Prefix == Prefix
                && other.Type == Type)
            {
                if (other.Cards.Count() != Cards.Count())
                {
                    return false;
                }
                for(int i = 0; i < other.Cards.Count();i++)
                {
                    if (Cards[i] != other.Cards[i]) { return false; }
                }
                return true;
            }
            return this.GetHashCode() == other.GetHashCode();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(YgoSet a, YgoSet b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.Equals(b);
        }

        public static bool operator !=(YgoSet a, YgoSet b)
        {
            return !(a == b);
        }
    }

    enum SETTYPE
    {
        BoosterPack
    }

    
}
