using OV.Tools;
using System.Collections.Generic;
using System;

namespace OV.Core
{   
    class YGOCard
    {
        public FRAME DefaultFrame = FRAME.Effect;

        private List<ABILITY> _Abilities;
        private double _ATK;
        private ATTRIBUTE _Attribute;
        private double _DEF;
        private FRAME _Frame;
        private bool _IsPendulum;
        private double _Level;
        private string _PendulumEffect;
        private PROPERTY _Property;
        private double _Rank;
        private TYPE _Type;
        private double _ScaleLeft;
        private double _ScaleRight;
        private YGOCard()
        {
            this._Abilities = new List<ABILITY>();
        }

        public IReadOnlyList<ABILITY> Abilities { get { return _Abilities.AsReadOnly(); } }
        public byte[] ArtworkByte { get; set; }
        /// <summary>
        /// Get value of ATK of card
        /// <para>-1 means "N/A", double.Nan means "?"</para>
        /// </summary>
        public double ATK { get { return _ATK; } }

        public ATTRIBUTE Attribute { get { return _Attribute; } }
        public string Creator { get; set; }
        /// <summary>
        /// Get value of DEF of card
        /// <para>-1 means "N/A", double.Nan means "?"</para>
        /// </summary>
        public double DEF { get { return _DEF; } }

        public string Description { get; set; }
        public EDITION Edition { get; set; }
        public FRAME Frame { get { return _Frame; } }
        public bool IsPendulum { get { return _IsPendulum; } }
        public double Level { get { return _Level; } }
        public string Name { get; set; }
        public int Number { get; set; }
        public string PendulumEffect { get { return _PendulumEffect; } }
        public PROPERTY Property { get { return _Property; } }
        public double Rank { get { return _Rank; } }
        public RARITY Rarity { get; set; }
        /// <summary>
        /// Get value of left scale of card
        /// <para>-1 means "N/A", double.Nan means "?"</para>
        /// </summary>
        public double ScaleLeft { get { return _ScaleLeft; } }

        /// <summary>
        /// Get value of right scale of card
        /// <para>-1 means "N/A", double.Nan means "?"</para>
        /// </summary>
        public double ScaleRight { get { return _ScaleRight; } }

        public string Set { get; set; }
        public STICKER Sticker { get; set; }
        public TYPE Type { get { return _Type; } }
        internal static YGOCard Default
        {
            get
            {
                YGOCard defaultCard = new YGOCard();
                defaultCard.Name = "Card Name";
                defaultCard._ATK = 1200;
                defaultCard._DEF = 1200;
                defaultCard._Frame = FRAME.Effect;
                defaultCard._Attribute = ATTRIBUTE.UNKNOWN;
                defaultCard._Level = 4;
                defaultCard.Creator = "© 1996 KAZUKI TAKAHASHI";
                defaultCard.Rarity = RARITY.Common;
                //defaultCard._Type = TYPE.Warrior;
                defaultCard.Sticker = STICKER.PromoSilver;
                defaultCard.ArtworkByte = Images.GetImageByte(Utilities.GetLocationPath() + @"\Template\NoneImage.png");
                defaultCard.Set = "";
                defaultCard._ScaleLeft = defaultCard._ScaleRight = -1;
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
                        _ScaleLeft = _ScaleRight = _ATK = _DEF = _Level = _Rank = -1;
                        _IsPendulum = false;
                        _Frame = (attribute == ATTRIBUTE.SPELL) ? FRAME.Spell : FRAME.Trap;
                        _Abilities.Clear();
                        _Property = PROPERTY.Normal;
                        _Type = TYPE.NONE;                       
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
                            _Property = PROPERTY.Normal;
                        }
                    }
                    //From Spell To Trap
                    else if (attribute == ATTRIBUTE.TRAP)
                    {
                        _Frame = FRAME.Trap;
                        if (Property.IsSpellPropertyOnly())
                        {
                            _Property = PROPERTY.Normal;
                        }
                    }
                    //To Monster
                    else
                    {
                        _ScaleLeft = _ScaleRight = _ATK = _DEF = double.NaN;
                        _Frame = DefaultFrame;
                        _Property = PROPERTY.NONE;
                        _Level = 4; //Default Level
                        _Type = TYPE.NONE;
                    }
                }
            }

            this._Attribute = attribute;
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
                        _ScaleLeft = _ScaleRight = _ATK = _DEF = _Level = _Rank = -1;
                        _IsPendulum = false;
                        _Attribute = frame == FRAME.Spell ? ATTRIBUTE.SPELL : ATTRIBUTE.TRAP;
                        _Abilities.Clear();
                        _Property = PROPERTY.Normal;
                        _Type = TYPE.NONE;
                    }
                }
                //Card is Spell/Trap
                else
                {
                    //From Trap To Spell
                    if (frame == FRAME.Spell)
                    {
                        _Attribute = ATTRIBUTE.SPELL;
                        if (Property.IsTrapPropertyOnly())
                        {
                            _Property = PROPERTY.Normal;
                        }
                    }
                    //From Spell To Trap
                    else if (frame == FRAME.Trap)
                    {
                        _Attribute = ATTRIBUTE.TRAP;
                        if (Property.IsSpellPropertyOnly())
                        {
                            _Property = PROPERTY.Normal;
                        }
                    }
                    //To Monster
                    else
                    {
                        _ScaleLeft = _ScaleRight = _ATK = _DEF = double.NaN;
                        _Attribute = ATTRIBUTE.UNKNOWN;
                        _Property = PROPERTY.NONE;
                        _Level = 4; //Default Level
                    }
                }
            }
            this._Frame = frame;
        }

        public void SetLevel(double level, bool setLogic = true)
        {            
            if (setLogic)
            {
                if (this.IsMagic())
                {
                    _ScaleLeft = _ScaleRight = _ATK = _DEF = double.NaN;
                    _Attribute = ATTRIBUTE.UNKNOWN;
                    _Frame = DefaultFrame;
                    _Property = PROPERTY.NONE;
                } else
                {
                    if (this.IsFrame(FRAME.Xyz))
                    {
                        _Rank = double.NaN;
                        _Frame = DefaultFrame;
                    }
                }                
            }
            this._Level = level;
        }

        internal void SetDEF(double value, bool setLogic = true)
        {
            if (setLogic)
            {
                if (this.IsMagic())
                {
                    _Frame = DefaultFrame;
                    _Attribute = ATTRIBUTE.UNKNOWN;
                    _Property = PROPERTY.NONE;
                    _Level = 4; //Default Level
                    _ATK = double.NaN;
                }
            }
            _DEF = value;
        }

        public void SetRank(double rank, bool setLogic = true)
        {
            if (setLogic)
            {
                if (this.IsMagic())
                {
                    _ScaleLeft = _ScaleRight = _ATK = _DEF = double.NaN;
                    _Attribute = ATTRIBUTE.UNKNOWN;
                    _Frame = DefaultFrame;
                    _Property = PROPERTY.NONE;
                }
                else
                {
                    if (this.IsFrame(FRAME.Xyz) == false)
                    {
                        _Level = double.NaN;
                    }
                }
            }
            this._Rank = rank;
        }

        internal void SetAbility(ABILITY ability, bool isAdd, bool setLogic = true)
        {
            if (setLogic)
            {
                if (this.IsMonster())
                {
                    //Other Logic
                }
                else
                {
                    _Frame = DefaultFrame;
                    _Attribute = ATTRIBUTE.UNKNOWN;
                    _Property = PROPERTY.NONE;
                    _Level = 4; //Default Level
                    _ATK = _DEF = double.NaN;
                }
            }
            if (isAdd && _Abilities.Contains(ability) == false)
            {
                _Abilities.Add(ability);
            }
            else if (isAdd == false && _Abilities.Contains(ability))
            {
                _Abilities.Remove(ability);
            }

            _Abilities.Sort();
        }

        internal void SetATK(double value, bool setLogic = true)
        {
            if (setLogic)
            {
                if (this.IsMagic())
                {
                    _Frame = DefaultFrame;
                    _Attribute = ATTRIBUTE.UNKNOWN;
                    _Property = PROPERTY.NONE;
                    _Level = 4; //Default Level
                    _DEF = double.NaN;
                }
            }
            _ATK = value;
        }

        internal void SetPendulum(bool isPendulum, bool setLogic = true)
        {
            if (setLogic)
            {
                if (this.IsMonster())
                {
                    if (this.IsPendulum == false)
                    {
                        _ScaleLeft = _ScaleRight = 4; //Default Scale
                    }
                }
                else
                {
                    _ScaleLeft = _ScaleRight = 4; //Default Scale
                    _Frame = DefaultFrame;
                    _Attribute = ATTRIBUTE.UNKNOWN;
                    _Property = PROPERTY.NONE;
                    _Level = 4; //Default Level
                    _ATK = _DEF = double.NaN;
                }
            }
            this._IsPendulum = isPendulum;
        }

        internal void SetPendulumEffect(string text, bool setLogic = true)
        {
            if (setLogic)
            {
                if (this.IsMonster() && IsPendulum == false)
                {
                    _ScaleLeft = _ScaleRight = 4; //Default Scale
                    _IsPendulum = true;
                }
                else if (this.IsMagic())
                {
                    _ScaleLeft = _ScaleRight = 4; //Default Scale
                    _Frame = DefaultFrame;
                    _Attribute = ATTRIBUTE.UNKNOWN;
                    _Property = PROPERTY.NONE;
                    _Level = 4; //Default Level
                    _ATK = _DEF = double.NaN;
                    _IsPendulum = true;
                }
            }
            _PendulumEffect = text;
        }

        internal void SetScaleLeft(double value, bool setLogic = true)
        {
            if (setLogic)
            {
                if (this.IsMonster() && IsPendulum == false)
                {
                    _ScaleRight = 4; //Default Scale
                    _IsPendulum = true;
                }
                else if (this.IsMagic())
                {
                    _ScaleRight = 4; //Default Scale
                    _Frame = DefaultFrame;
                    _Attribute = ATTRIBUTE.UNKNOWN;
                    _Property = PROPERTY.NONE;
                    _Level = 4; //Default Level
                    _ATK = _DEF = double.NaN;
                    _IsPendulum = true;
                }
            }
            _ScaleLeft = value;
        }

        internal void SetScaleRight(double value, bool setLogic = true)
        {
            if (setLogic)
            {
                if (this.IsMonster() && IsPendulum == false)
                {
                    _ScaleLeft = 4; //Default Scale
                    _IsPendulum = true;
                }
                else if (this.IsMagic())
                {
                    _ScaleLeft = 4; //Default Scale
                    _Frame = DefaultFrame;
                    _Attribute = ATTRIBUTE.UNKNOWN;
                    _Property = PROPERTY.NONE;
                    _Level = 4; //Default Level
                    _ATK = _DEF = double.NaN;
                    _IsPendulum = true;
                }
            }
            _ScaleRight = value;
        }

        internal void SetProperty(FRAME frame, PROPERTY property, bool setLogic = true)
        {
            this._Frame = frame;
            if (setLogic)
            {
                if (this.IsMonster())
                {
                    _ScaleLeft = _ScaleRight = _ATK = _DEF = _Level = _Rank = -1;
                    _IsPendulum = false;                    
                    _Attribute = _Frame == FRAME.Spell ? ATTRIBUTE.SPELL : ATTRIBUTE.TRAP;
                    _Abilities.Clear();
                    _Property = PROPERTY.Normal;
                    _Type = TYPE.NONE;
                }
                else
                {
                    if (property.IsSpellPropertyOnly())
                    {
                        _Attribute = ATTRIBUTE.SPELL;
                        //_Frame = FRAME.Spell;
                    }
                    else if (property.IsTrapPropertyOnly())
                    {
                        _Attribute = ATTRIBUTE.TRAP;
                        //_Frame = FRAME.Trap;
                    }
                }
            }
            _Property = property;
        }

        internal void SetType(TYPE type, bool setLogic = true)
        {
            if (setLogic)
            {
                if (this.IsMagic())
                {                    
                    _Frame = DefaultFrame;
                    _Attribute = ATTRIBUTE.UNKNOWN;
                    _Property = PROPERTY.NONE;
                    _Level = 4; //Default Level
                    _ATK = _DEF = double.NaN;
                }
            }
            _Type = type;
        }
        /*
        internal void CleanUp()
        {
            ATK = Math.Round(ATK, MidpointRounding.ToEven);
            DEF = Math.Round(DEF, MidpointRounding.ToEven);
            ScaleLeft = Math.Round(ScaleLeft, MidpointRounding.ToEven);
            ScaleRight = Math.Round(ScaleRight, MidpointRounding.ToEven);
            _Level = Math.Round(_Level, MidpointRounding.ToEven);
            _Rank = Math.Round(_Rank, MidpointRounding.ToEven);
        }
        */
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

    static class Static
    {

        public static bool IsFrame(this YGOCard card, FRAME frame)
        {
            return frame == card.Frame;
        }

        public static bool IsMagic(this YGOCard card)
        {
            if (card.Frame == FRAME.Spell && card.Attribute == ATTRIBUTE.SPELL) { return true; }
            if (card.Frame == FRAME.Trap && card.Attribute == ATTRIBUTE.TRAP) { return true; }
            //if (card.Attribute == ATTRIBUTE.UNKNOWN) { return false; }
            return false;
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
}
