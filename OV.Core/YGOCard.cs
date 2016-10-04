using OV.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace OV.Core
{
    class YGOCard
    {
        public string Name { get; set; }
        
        public ATTRIBUTE Attribute { get; set; }

        public double Level { get; set; }

        public double Rank { get; set; }

        public PROPERTY Property { get; set; }

        public byte[] ArtworkByte { get; set; }

        public TYPE Type { get; set; }
        
        public FRAME Frame { get; set; }        

        public List<ABILITY> Abilities;

        public string Description { get; set; }

        public string PendulumEffect { get; set; }

        public double ATK { get; set; }

        public double DEF { get; set; }

        public string Creator { get; set; }

        public RARITY Rarity { get; set; }

        public bool IsPendulum { get; set; }

        public EDITION Edition { get; set; }

        public STICKER Sticker { get; set; }

        public int Number { get; set; }

        public string Set { get; set; }

        public double ScaleLeft { get; set; }

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
                defaultCard.Frame = FRAME.Effect;
                defaultCard.Attribute = ATTRIBUTE.UNKNOWN;
                defaultCard.Level = 4;
                defaultCard.Creator = "© 1996 KAZUKI TAKAHASHI";
                defaultCard.Rarity = RARITY.Common;
                defaultCard.Type = TYPE.Warrior;
                defaultCard.Sticker = STICKER.PromoSilver;
                defaultCard.ArtworkByte = Images.GetImageByte(Utilities.GetLocationPath() + @"\Template\NoneImage.png");
                defaultCard.Set = "";
                defaultCard.ScaleLeft = defaultCard.ScaleRight = double.NaN;
                return defaultCard;
            }
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
