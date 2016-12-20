using OV.Tools;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.IO;
using JsonNet.PrivateSettersContractResolvers;

namespace OV.Core
{
    [Serializable]
    internal class YgoCard
    {
        private FRAME DefaultFrame;
        private static string DatabasePath;

        private static ByteDatabase Database;
        public Version Version { get; private set; }
        
        private YgoCard()
        {
            DefaultFrame = FRAME.Effect;
            DatabasePath = DatabasePath = Utilities.GetLocationPath() + @"\Resources\Datas.ld";
            Database = new ByteDatabase(DatabasePath);
            this.Abilities = new List<ABILITY>();
            this.Version = new Version("0.1");
        }
         

        public List<ABILITY> Abilities { get; private set; }
        public byte[] ArtworkByte { get; set; }

        /// <summary>
        /// Get value of ATK of card
        /// <para>-1 means "N/A", double.Nan means "?"</para>
        /// </summary>

        public double ATK { get; private set; }

        public ATTRIBUTE Attribute { get; private set; }
        public CREATOR Creator { get; private set; }
        /// <summary>
        /// Get value of DEF of card
        /// <para>-1 means "N/A", double.Nan means "?"</para>
        /// </summary>
        public double DEF { get; private set; }

        public string Description { get; private set; }
        public EDITION Edition { get; private set; }

        public FRAME Frame { get; private set; }
        public bool IsPendulum { get; private set; }
        public double Level { get; private set; }
        public string Name { get; private set; }
        public int Number { get; private set; }
        public string PendulumEffect { get; private set; }
        public PROPERTY Property { get; private set; }
        public double Rank { get; private set; }
        public RARITY Rarity { get; private set; }
        /// <summary>
        /// Get value of left scale of card
        /// <para>-1 means "N/A", double.NaN means "?"</para>
        /// </summary>
        public double ScaleLeft { get; private set; }

        /// <summary>
        /// Get value of right scale of card
        /// <para>-1 means "N/A", double.NaN means "?"</para>
        /// </summary>
        public double ScaleRight { get; private set; }

        public string Set { get; private set; }
        public STICKER Sticker { get; private set; }
        public TYPE Type { get; private set; }
        internal static YgoCard Default
        {
            get
            {                
                YgoCard defaultCard = new YgoCard();
                
                defaultCard.Name = "Card Name";
                defaultCard.ATK = 1200;
                defaultCard.DEF = 1200;
                defaultCard.Frame = FRAME.Effect;
                defaultCard.Attribute = ATTRIBUTE.UNKNOWN;
                defaultCard.Level = 4;
                defaultCard.Creator = CREATOR.KazukiTakahashi;
                defaultCard.Rarity = RARITY.Common;
                defaultCard.Type = TYPE.Warrior;
                defaultCard.Sticker = STICKER.PromoSilver;
                defaultCard.ArtworkByte = Database.GetData(@"Template\NoneImage.png").Bytes;
                defaultCard.Set = "";
                defaultCard.ScaleLeft = defaultCard.ScaleRight = -1;
                defaultCard.Description = "";
                defaultCard.Edition = EDITION.UnlimitedEdition;
                defaultCard.IsPendulum = false;
                defaultCard.Number = 0;
                defaultCard.PendulumEffect = "";
                defaultCard.Property = PROPERTY.NONE;
                defaultCard.Rank = double.NaN;
                return defaultCard;
            }
        }

        public YgoCard Clone()
        {
            YgoCard clone = new YgoCard()
            {
                Name = this.Name,
                ATK = this.ATK,
                DEF = this.DEF,
                Frame = this.Frame,
                Attribute = this.Attribute,
                Level = this.Level,
                Creator = this.Creator,
                Rarity = this.Rarity,
                Type = this.Type,
                Sticker = this.Sticker,
                ArtworkByte = this.ArtworkByte,
                Set = this.Set,
                ScaleLeft = this.ScaleLeft,
                ScaleRight = this.ScaleRight,
                Description = this.Description,
                Edition = this.Edition,
                IsPendulum = this.IsPendulum,
                Number = this.Number,
                PendulumEffect = this.PendulumEffect,
                Property = this.Property,
                Rank = this.Rank
            };
            foreach (ABILITY ability in this.Abilities)
            {
                clone.Abilities.Add(ability);
            }
            return clone;
        }

        public static YgoCard LoadFrom(string fileName)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new PrivateSetterContractResolver(),
            };
            return JsonConvert.DeserializeObject<YgoCard>(File.ReadAllText(fileName), settings);
            
        }

        public string GetData()
        {
            if (this == null) { return ""; }
            string result = "";
            result += this.Name;
            if (this.IsMonster())
            {
                result += Environment.NewLine;
                result += this.IsFrame(FRAME.Xyz)
                    ? string.Format("Rank {0}", this.Rank)
                    : string.Format("Level {0}", this.Level);
                result += " " + this.Attribute.ToString();
                result += " " + string.Format("{0}-Type", this.Type);
                result += " " + this.Frame;
                if (this.Abilities.Count > 0)
                {
                    result += " " + string.Join(" ", this.Abilities);
                }
                result += Environment.NewLine;
                result += string.Format("ATK {0}\nDEF {1}", this.ATK, this.DEF);
            }
            result += Environment.NewLine;
            result += this.Description.CleanUpUnnecessarySpace();
            return result;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            /*
            if (Creator.ToString().TryEnum<CREATOR>() == false)
            {
                //MessageBox.Show("here");
            } else
            {
                
            } */
        }

        public void SetAttribute(ATTRIBUTE attribute, bool setLogic = true)
        {
            if (setLogic)
            {
                //Card is Monster
                if (this.IsMonster())
                {
                    if (attribute == ATTRIBUTE.SPELL || attribute == ATTRIBUTE.TRAP)
                    {
                        ScaleLeft = ScaleRight = ATK = DEF = Level = Rank = -1;
                        IsPendulum = false;
                        Frame = (attribute == ATTRIBUTE.SPELL) ? FRAME.Spell : FRAME.Trap;
                        Abilities.Clear();
                        Property = PROPERTY.Normal;
                        Type = TYPE.NONE;
                    }
                }
                //Card is Spell/Trap
                else
                {
                    //From Trap To Spell
                    if (attribute == ATTRIBUTE.SPELL)
                    {
                        Frame = FRAME.Spell;
                        if (Property.IsTrapPropertyOnly())
                        {
                            Property = PROPERTY.Normal;
                        }
                    }
                    //From Spell To Trap
                    else if (attribute == ATTRIBUTE.TRAP)
                    {
                        Frame = FRAME.Trap;
                        if (Property.IsSpellPropertyOnly())
                        {
                            Property = PROPERTY.Normal;
                        }
                    }
                    //To Monster
                    else
                    {
                        ScaleLeft = ScaleRight = ATK = DEF = double.NaN;
                        Frame = DefaultFrame;
                        Property = PROPERTY.NONE;
                        Level = 4; //Default Level
                        Type = TYPE.NONE;
                    }
                }
            }

            this.Attribute = attribute;
        }

        public void SetFrame(FRAME frame, bool setLogic = true)
        {
            if (setLogic)
            {
                //Card is Monster
                if (this.IsMonster())
                {
                    //To Spell/Trap
                    if (frame == FRAME.Spell || frame == FRAME.Trap)
                    {
                        ScaleLeft = ScaleRight = ATK = DEF = Level = Rank = -1;
                        IsPendulum = false;
                        Attribute = frame == FRAME.Spell ? ATTRIBUTE.SPELL : ATTRIBUTE.TRAP;
                        Abilities.Clear();
                        Property = PROPERTY.Normal;
                        Type = TYPE.NONE;
                    }

                    if (Abilities.Contains(ABILITY.Effect) && frame == FRAME.Effect)
                    {
                        Abilities.Remove(ABILITY.Effect);
                    }
                    //To Xyz
                    if (frame == FRAME.Xyz && Frame != FRAME.Xyz)
                    {
                        Level = double.NaN;
                        Rank = 4;
                    }
                    //To Non-Xyz
                    if (frame != FRAME.Xyz && Frame == FRAME.Xyz)
                    {
                        Rank = double.NaN;
                        Level = 4;
                    }
                    if (frame == FRAME.Normal)
                    {
                        Abilities.Clear();
                    }
                }
                //Card is Spell/Trap
                else
                {
                    //From Trap To Spell
                    if (frame == FRAME.Spell)
                    {
                        Attribute = ATTRIBUTE.SPELL;
                        if (Property.IsTrapPropertyOnly())
                        {
                            Property = PROPERTY.Normal;
                        }
                    }
                    //From Spell To Trap
                    else if (frame == FRAME.Trap)
                    {
                        Attribute = ATTRIBUTE.TRAP;
                        if (Property.IsSpellPropertyOnly())
                        {
                            Property = PROPERTY.Normal;
                        }
                    }
                    //To Monster
                    else
                    {
                        ScaleLeft = ScaleRight = ATK = DEF = double.NaN;
                        Attribute = ATTRIBUTE.UNKNOWN;
                        Property = PROPERTY.NONE;
                        Level = 4; //Default Level
                    }
                }
            }
            this.Frame = frame;
        }

        public void SetLevel(double level, bool setLogic = true)
        {
            if (setLogic)
            {
                if (this.IsMagic())
                {
                    ScaleLeft = ScaleRight = ATK = DEF = double.NaN;
                    Attribute = ATTRIBUTE.UNKNOWN;
                    Frame = DefaultFrame;
                    Property = PROPERTY.NONE;
                }
                else
                {
                    if (this.IsFrame(FRAME.Xyz))
                    {
                        Rank = double.NaN;
                        Frame = DefaultFrame;
                    }
                }
            }
            this.Level = level;
        }

        internal void SetDEF(double value, bool setLogic = true)
        {
            if (setLogic)
            {
                if (this.IsMagic())
                {
                    Frame = DefaultFrame;
                    Attribute = ATTRIBUTE.UNKNOWN;
                    Property = PROPERTY.NONE;
                    Level = 4; //Default Level
                    ATK = double.NaN;
                }
            }
            DEF = value;
        }

        public void SetRank(double rank, bool setLogic = true)
        {
            if (setLogic)
            {
                if (this.IsMagic())
                {
                    ScaleLeft = ScaleRight = ATK = DEF = double.NaN;
                    Attribute = ATTRIBUTE.UNKNOWN;
                    Frame = DefaultFrame;
                    Property = PROPERTY.NONE;
                }
                else
                {
                    if (this.IsFrame(FRAME.Xyz) == false)
                    {
                        Frame = FRAME.Xyz;
                        Level = double.NaN;
                    }
                }
            }
            this.Rank = rank;
        }

        internal void SetAbility(ABILITY ability, bool isAdd, bool setLogic = true)
        {
            if (setLogic)
            {
                if (this.IsMonster() && this.IsFrame(FRAME.Normal) == false)
                {
                    //Another Logic
                    if (ability.IsEffectAbility())
                    {
                        this.Abilities.RemoveAll(o => o.IsEffectAbility());
                    }
                }
                else
                {
                    Frame = DefaultFrame;
                    Attribute = ATTRIBUTE.UNKNOWN;
                    Property = PROPERTY.NONE;
                    Level = 4; //Default Level
                    ATK = DEF = double.NaN;
                }
            }
            if (isAdd && Abilities.Contains(ability) == false)
            {
                if (ability.IsEffectAbility() && Frame != FRAME.Effect)
                {
                    Abilities.Add(ABILITY.Effect);
                }
                Abilities.Add(ability);
            }
            else if (isAdd == false && Abilities.Contains(ability))
            {
                Abilities.Remove(ability);
            }

            Abilities.Sort();
        }

        internal void SetATK(double value, bool setLogic = true)
        {
            if (setLogic)
            {
                if (this.IsMagic())
                {
                    Frame = DefaultFrame;
                    Attribute = ATTRIBUTE.UNKNOWN;
                    Property = PROPERTY.NONE;
                    Level = 4; //Default Level
                    DEF = double.NaN;
                }
            }
            ATK = value;
        }

        internal void SetPendulum(bool isPendulum, bool setLogic = true)
        {
            if (setLogic)
            {
                if (this.IsMonster())
                {
                    if (this.IsPendulum == false)
                    {
                        ScaleLeft = ScaleRight = 4; //Default Scale
                    }
                    PendulumEffect = "";
                }
                else
                {
                    ScaleLeft = ScaleRight = 4; //Default Scale
                    Frame = DefaultFrame;
                    Attribute = ATTRIBUTE.UNKNOWN;
                    Property = PROPERTY.NONE;
                    Level = 4; //Default Level
                    ATK = DEF = double.NaN;
                }
            }
            this.IsPendulum = isPendulum;
        }

        internal void SetPendulumEffect(string text, bool setLogic = true)
        {
            if (string.IsNullOrWhiteSpace(text) == false && setLogic)
            {
                if (this.IsMonster() && IsPendulum == false)
                {
                    ScaleLeft = ScaleRight = 4; //Default Scale
                    IsPendulum = true;
                }
                else if (this.IsMagic())
                {
                    ScaleLeft = ScaleRight = 4; //Default Scale
                    Frame = DefaultFrame;
                    Attribute = ATTRIBUTE.UNKNOWN;
                    Property = PROPERTY.NONE;
                    Level = 4; //Default Level
                    ATK = DEF = double.NaN;
                    IsPendulum = true;
                }
            }
            PendulumEffect = text;
        }

        internal void SetScaleLeft(double value, bool setLogic = true)
        {
            if (setLogic)
            {
                if (this.IsMonster() && IsPendulum == false)
                {
                    ScaleRight = 4; //Default Scale
                    IsPendulum = true;
                }
                else if (this.IsMagic())
                {
                    ScaleRight = 4; //Default Scale
                    Frame = DefaultFrame;
                    Attribute = ATTRIBUTE.UNKNOWN;
                    Property = PROPERTY.NONE;
                    Level = 4; //Default Level
                    ATK = DEF = double.NaN;
                    IsPendulum = true;
                }
            }
            if (double.IsNaN(value) == false)
            {
                value = (int)value;
                value = value.Clamp(0, 13);
            }
            ScaleLeft = value;
        }

        internal void SetScaleRight(double value, bool setLogic = true)
        {
            if (setLogic)
            {
                if (this.IsMonster() && IsPendulum == false)
                {
                    ScaleLeft = 4; //Default Scale
                    IsPendulum = true;
                }
                else if (this.IsMagic())
                {
                    ScaleLeft = 4; //Default Scale
                    Frame = DefaultFrame;
                    Attribute = ATTRIBUTE.UNKNOWN;
                    Property = PROPERTY.NONE;
                    Level = 4; //Default Level
                    ATK = DEF = double.NaN;
                    IsPendulum = true;
                }
            }
            if (double.IsNaN(value) == false)
            {
                value = (int)value;
                value = value.Clamp(0, 13);
            }
            ScaleRight = value;
        }

        internal void SetProperty(FRAME frame, PROPERTY property, bool setLogic = true)
        {
            this.Frame = frame;
            if (setLogic)
            {
                if (this.IsMonster())
                {
                    ScaleLeft = ScaleRight = ATK = DEF = Level = Rank = -1;
                    IsPendulum = false;
                    Attribute = Frame == FRAME.Spell ? ATTRIBUTE.SPELL : ATTRIBUTE.TRAP;
                    Abilities.Clear();
                    Property = PROPERTY.Normal;
                    Type = TYPE.NONE;
                }
                else
                {
                    if (property.IsSpellPropertyOnly())
                    {
                        Attribute = ATTRIBUTE.SPELL;
                        //_Frame = FRAME.Spell;
                    }
                    else if (property.IsTrapPropertyOnly())
                    {
                        Attribute = ATTRIBUTE.TRAP;
                        //_Frame = FRAME.Trap;
                    }
                }
            }
            Property = property;
        }

        internal void SetType(TYPE type, bool setLogic = true)
        {
            if (setLogic)
            {
                if (this.IsMagic())
                {
                    Frame = DefaultFrame;
                    Attribute = ATTRIBUTE.UNKNOWN;
                    Property = PROPERTY.NONE;
                    Level = 4; //Default Level
                    ATK = DEF = double.NaN;
                }
            }
            Type = type;
        }

        internal void SetSticker(STICKER sticker)
        {
            Sticker = sticker;
        }

        internal void SetSet(string set)
        {
            Set = set;
        }

        internal void SetDescription(string description)
        {
            Description = description;
        }

        internal void SetEdition(EDITION edition)
        {
            Edition = edition;
        }

        internal void SetNumber(int number)
        {
            Number = number;
        }

        internal void SetName(string name)
        {
            Name = name;
        }

        internal void SetRarity(RARITY rarity)
        {
            Rarity = rarity;
        }

        internal void SetCreator(CREATOR creator)
        {
            Creator = creator;
        }
        /*
internal void CleanUp()
{
   ATK = Math.Round(ATK, MidpointRounding.ToEven);
   DEF = Math.Round(DEF, MidpointRounding.ToEven);
   ScaleLeft = Math.Round(ScaleLeft, MidpointRounding.ToEven);
   ScaleRight = Math.Round(ScaleRight, MidpointRounding.ToEven);
   Level = Math.Round(_Level, MidpointRounding.ToEven);
   Rank = Math.Round(_Rank, MidpointRounding.ToEven);
}


*/

        public override bool Equals(Object obj)
        {
            if (obj == null) return false;
            var other = obj as YgoCard;
            if (other == null) return false;

            if (other.Name == this.Name
                && (!other.Abilities.Except(this.Abilities).Any() && !this.Abilities.Except(other.Abilities).Any())
                && ((other.ArtworkByte == null && this.ArtworkByte == null)
                    || (other.ArtworkByte != null && other.ArtworkByte.SequenceEqual(this.ArtworkByte)))
                && other.ATK.Equals(this.ATK)
                && other.DEF.Equals(this.DEF)
                && other.Attribute == this.Attribute
                && other.Description == this.Description
                && other.Frame == this.Frame
                && other.IsPendulum == this.IsPendulum
                && other.Level.Equals(this.Level)
                && other.PendulumEffect == this.PendulumEffect
                && other.Number == this.Number
                && other.Property == this.Property
                && other.Rank.Equals(this.Rank)
                && other.ScaleLeft.Equals(this.ScaleLeft)
                && other.ScaleRight.Equals(this.ScaleRight)
                && other.Type == this.Type
                && other.Rarity == this.Rarity) { return true; }

            return this.GetHashCode() == other.GetHashCode();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(YgoCard a, YgoCard b)
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

        public static bool operator !=(YgoCard a, YgoCard b)
        {
            return !(a == b);
        }

        internal void ResetAbility()
        {
            this.Abilities.Clear();
        }
    }

    public enum ABILITY
    {
        Flip, Toon, Union, Gemini, Spirit,
        Effect, Tuner,
    };

    public enum EDITION
    {
        UnlimitedEdition,
        FirstEdition,
        LimitedEdition,
        DuelTerminal
    }

    public enum FRAME
    {
        Normal, Effect, Token, Fusion, Ritual,
        Synchro, Xyz,
        Spell, Trap
    };

    public enum RARITY
    {
        Common,
        Rare, UltimateRare,
        SuperRare,
        UltraRare,
        SecretRare,
        ParallelRare,
        StarfoilRare,
        MosaicRare,
        GoldRare,
        GhostRare
    }

    public enum STICKER
    {
        NONE,
        Gold, Silver,
        PromoGold, PromoSilver
    }

    public enum TYPE
    {
        NONE,
        Warrior, Machine, Spellcaster, Fiend, Dragon, Fairy,
        Beast, Aqua, Rock, Insect, Plant, Zombie, Reptile, Pyro,
        Fish, Thunder, Psychic, Wyrm, Dinosaur,
        WingedBeast, BeastWarrior, SeaSerpent, DivineBeast, CreatorGod
    }

    enum ATTRIBUTE
    {
        UNKNOWN,
        EARTH, DARK, DIVINE, FIRE, LIGHT, WATER, WIND,
        SPELL, TRAP
    }

    enum PROPERTY
    {
        NONE,
        Continuous, Normal,
        QuickPlay, Ritual, Equip, Field,
        Counter
    }

    internal enum CREATOR
    {
        NONE,
        KazukiTakahashi
    }


    
}
