using System;
using System.Collections.Generic;
using System.Text;

namespace OV.Core
{
    class YGOCard
    {
        public string Name { get; set; }

        public ATTRIBUTE Attribute { get; set; } = ATTRIBUTE.NONE;

        public double Level { get; set } = double.NaN;

        public double Rank { get; set; } = double.NaN;

        public PROPERTY Property { get; set; } = PROPERTY.NONE;

        public byte[] ArtworkByte { get; set; }

        public TYPE Type { get; set; } = TYPE.NONE;
                
        public FRAME Frame { get; set; }

        public List<ABILITY> Abilities = new List<ABILITY>();

        public string Description { get; set; }

        public double ATK { get; set; } = double.NaN;

        public double DEF { get; set; } = double.NaN;
    }

    enum ATTRIBUTE
    {
        NONE,
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
        NONE, Flip, Toon, Union, Gemini, Spirit,
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
}
