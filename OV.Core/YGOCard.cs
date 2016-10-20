using OV.Tools;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using OV.Pyramid;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Windows;

namespace OV.Core
{   
    class YGOCard
    {
        private FRAME DefaultFrame = FRAME.Effect;
        

        private YGOCard()
        {
            this.Abilities = new List<ABILITY>();
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
        internal static YGOCard Default
        {
            get
            {
                YGOCard defaultCard = new YGOCard();
                defaultCard.Name = "Card Name";
                defaultCard.ATK = 1200;
                defaultCard.DEF = 1200;
                defaultCard.Frame = FRAME.Effect;
                defaultCard.Attribute = ATTRIBUTE.UNKNOWN;
                defaultCard.Level = 4;
                defaultCard.Creator = CREATOR.KazukiTakahashi;;
                defaultCard.Rarity = RARITY.Common;
                //defaultCard.Type = TYPE.Warrior;
                defaultCard.Sticker = STICKER.PromoSilver;                
                defaultCard.ArtworkByte = Images.GetImageByte(Utilities.GetLocationPath() + @"\Resources\Template\NoneImage.png");
                defaultCard.Set = "";
                defaultCard.ScaleLeft = defaultCard.ScaleRight = -1;
                return defaultCard;
            }
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
                    if (frame == FRAME.Spell || frame == FRAME.Trap)
                    {
                        ScaleLeft = ScaleRight = ATK = DEF = Level = Rank = -1;
                        IsPendulum = false;
                        Attribute = frame == FRAME.Spell ? ATTRIBUTE.SPELL : ATTRIBUTE.TRAP;
                        Abilities.Clear();
                        Property = PROPERTY.Normal;
                        Type = TYPE.NONE;
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
                } else
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
                    if (ability.IsSingleAbility())
                    {
                        this.Abilities.RemoveAll(o => o.IsSingleAbility());
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
            if (setLogic)
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
            var other = obj as YGOCard;
            if (other== null) return false;

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
                && other.Rank.Equals( this.Rank)
                && other.ScaleLeft.Equals(this.ScaleLeft)
                && other.ScaleRight.Equals(this.ScaleRight)
                && other.Type == this.Type) { return true; }

            return this.GetHashCode() == other.GetHashCode();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
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


static class Static
    {

        public static bool IsFrame(this YGOCard card, FRAME frame)
        {
            return frame == card.Frame;
        }

        public static bool IsMagic(this YGOCard card)
        {
            if (card != null)
            {
                if (card.Frame == FRAME.Spell && card.Attribute == ATTRIBUTE.SPELL) { return true; }
                if (card.Frame == FRAME.Trap && card.Attribute == ATTRIBUTE.TRAP) { return true; }
            }
            //if (card.Attribute == ATTRIBUTE.UNKNOWN) { return false; }
            return false;
        }

        public static bool IsMonster(this YGOCard card)
        {
            return card!= null && !card.IsMagic();
        }

        public static bool IsSpellPropertyOnly(this PROPERTY property)
        {
            if (property == PROPERTY.Equip) return true;
            if (property == PROPERTY.Field) return true;
            if (property == PROPERTY.QuickPlay) return true;
            if (property == PROPERTY.Ritual) return true;
            return false;
        }

        public static bool IsTrapPropertyOnly(this PROPERTY property)
        {
            if (property == PROPERTY.Counter) return true;
            return false;
        }

        public static bool IsSingleAbility(this ABILITY ability)
        {
            if (ability != ABILITY.Effect && ability != ABILITY.Tuner) { return true; }
            return false;
        }
    }
}
