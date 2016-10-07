using OV.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace OV.Core
{
    class YGOCard
    {
        public string Name { get; set; }

        private ATTRIBUTE _Attribute;
        public ATTRIBUTE Attribute { get { return _Attribute; } }

        public double Level { get; set; }

        public double Rank { get; set; }

        public PROPERTY Property { get; set; }

        public byte[] ArtworkByte { get; set; }

        public TYPE Type { get; set; }

        private FRAME _Frame;
        public FRAME Frame { get { return _Frame; } }        

        public List<ABILITY> Abilities;

        public string Description { get; set; }

        public string PendulumEffect { get; set; }
        /// <summary>
        /// Get value of ATK of card
        /// <para>-1 means "N/A", double.Nan means "?"</para>
        /// </summary>
        public double ATK { get; set; }
        /// <summary>
        /// Get value of DEF of card
        /// <para>-1 means "N/A", double.Nan means "?"</para>
        /// </summary>
        public double DEF { get; set; }

        public string Creator { get; set; }

        public RARITY Rarity { get; set; }

        public bool IsPendulum { get; set; }

        public EDITION Edition { get; set; }

        public STICKER Sticker { get; set; }

        public int Number { get; set; }

        public string Set { get; set; }
        /// <summary>
        /// Get value of left scale of card
        /// <para>-1 means "N/A", double.Nan means "?"</para>
        /// </summary>
        public double ScaleLeft { get; set; }
        /// <summary>
        /// Get value of right scale of card
        /// <para>-1 means "N/A", double.Nan means "?"</para>
        /// </summary>
        public double ScaleRight { get; set; }

        private YGOCard()
        {
            this.Abilities = new List<ABILITY>();
        }

        internal static YGOCard Default
        {
            get
            {
                YGOCard defaultCard = new YGOCard();
                defaultCard.Name = "Card Name";
                defaultCard.ATK = 1200;
                defaultCard.DEF = 1200;
                defaultCard._Frame = FRAME.Effect;
                defaultCard._Attribute = ATTRIBUTE.UNKNOWN;
                defaultCard.Level = 4;
                defaultCard.Creator = "© 1996 KAZUKI TAKAHASHI";
                defaultCard.Rarity = RARITY.Common;
                defaultCard.Type = TYPE.Warrior;
                defaultCard.Sticker = STICKER.PromoSilver;
                defaultCard.ArtworkByte = Images.GetImageByte(Utilities.GetLocationPath() + @"\Template\NoneImage.png");
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
                        _Frame = (attribute == ATTRIBUTE.SPELL) ? FRAME.Spell : FRAME.Trap;
                        Abilities.Clear();
                        Property = PROPERTY.Normal;                        
                    }
                }
                //Card is Spell/Trap
                else
                {                    
                    //From Trap To Spell
                    if (attribute == ATTRIBUTE.SPELL)
                    {
                        _Frame = FRAME.Spell;
                        if (Property.IsTrapPropertyOnly())
                        {
                            Property = PROPERTY.Normal;
                        }
                    }
                    //From Spell To Trap
                    else if (attribute == ATTRIBUTE.TRAP)
                    {
                        _Frame = FRAME.Trap;
                        if (Property.IsSpellPropertyOnly())
                        {
                            Property = PROPERTY.Normal;
                        }
                    }
                    //To Monster
                    else
                    {
                        ScaleLeft = ScaleRight = ATK = DEF = double.NaN;
                        _Frame = FRAME.Effect; //Default Frame
                        Property = PROPERTY.NONE;
                        Level = 4; //Default Level
                    }
                }
            }

            this._Attribute = attribute;
        }

        public void SetFrame(FRAME frame, bool setLogic = true)
        {
            this._Frame = frame;
        }
        
    }

    static class Static {

        public static bool IsFrame(this YGOCard card, FRAME frame)
        {            
            return frame == card.Frame;
        }

        public static bool IsMagic(this YGOCard card)
        {
            return (card.Frame == FRAME.Spell || card.Frame == FRAME.Trap);
        }

        public static bool IsMonster(this YGOCard card)
        {
            return !card.IsMagic();
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

    public enum FRAME
    {
        Normal, Effect, Token, Fusion, Ritual,
        Synchro, Xyz,
        Spell, Trap
    };

    public enum ABILITY
    {
        Flip, Toon, Union, Gemini, Spirit,
        Effect, Tuner,
    };

    public enum TYPE
    {
        NONE,
        Warrior, Machine, Spellcaster, Fiend, Dragon, Fairy,
        Beast, Aqua, Rock, Insect, Plant, Zombie, Reptile, Pyro,
        Fish, Thunder, Psychic, Wyrm, Dinosaur,
        WingedBeast, BeastWarrior, SeaSerpent, DivineBeast, CreatorGod
    }

    public enum STICKER
    {
        NONE, 
        Gold, Silver,
        PromoGold, PromoSilver
    }

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

    public enum EDITION
    {
        UnlimitedEdition,
        FirstEdition,
        LimitedEdition,
        DuelTerminal
    }
}
