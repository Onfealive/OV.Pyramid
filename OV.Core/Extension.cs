using Newtonsoft.Json;
using OV.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OV.Core
{

    static class Extension
    {
        #region Card
        /// <summary>
        /// Check the card whether it is this Frame or not
        /// </summary>
        /// <param name="card"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        public static bool IsFrame(this YgoCard card, FRAME frame)
        {
            return frame == card.Frame;
        }

        /// <summary>
        /// Check the card whether it is Spell/Trap, or not
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public static bool IsMagic(this YgoCard card)
        {
            if (card != null)
            {
                if (card.Frame == FRAME.Spell && card.Attribute == ATTRIBUTE.SPELL) { return true; }
                if (card.Frame == FRAME.Trap && card.Attribute == ATTRIBUTE.TRAP) { return true; }
            }
            //if (card.Attribute == ATTRIBUTE.UNKNOWN) { return false; }
            return false;
        }

        /// <summary>
        /// Check the card whether it is Monster or not
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public static bool IsMonster(this YgoCard card)
        {
            return card != null && !card.IsMagic();
        }

        /// <summary>
        /// Check the Property whether it is Property can only go with Spell Card
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool IsSpellPropertyOnly(this PROPERTY property)
        {
            if (property == PROPERTY.Equip) return true;
            if (property == PROPERTY.Field) return true;
            if (property == PROPERTY.QuickPlay) return true;
            if (property == PROPERTY.Ritual) return true;
            return false;
        }

        /// <summary>
        /// Check the Property whether it is Property can only go with Trap Card
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool IsTrapPropertyOnly(this PROPERTY property)
        {
            if (property == PROPERTY.Counter) return true;
            return false;
        }

        /// <summary>
        /// Check if Ability that must go with "Effect" Ability 
        /// </summary>
        /// <param name="ability"></param>
        /// <returns></returns>
        public static bool IsEffectAbility(this ABILITY ability)
        {
            if (ability != ABILITY.Effect && ability != ABILITY.Tuner) { return true; }
            return false;
        }

        /// <summary>
        /// Save the card data to "fileName" path (with extension)
        /// </summary>
        /// <param name="card"></param>
        /// <param name="fileName"></param>
        public static void SaveTo(this YgoCard card, string fileName)
        {
            string data = JsonConvert.SerializeObject(card, Formatting.Indented);
            File.WriteAllText(fileName, data);
        }

        public static string GetString(this PROPERTY property)
        {
            if (property == PROPERTY.QuickPlay)
            {
                return "Quick-Play";
            }
            return property.ToString();
        }

        public static string GetString(this TYPE type)
        {
            string result = type.ToString().Replace("WingedBeast", "Winged Beast");
            result = result.Replace("BeastWarrior", "Beast-Warrior");
            result = result.Replace("SeaSerpent", "Sea Serpent");
            result = result.Replace("DivineBeast", "Divine-Beast");
            result = result.Replace("CreatorGod", "Creator God");

            return result;
        }


        #endregion Card

        #region Set
        /// <summary>
        /// Save the set data to "fileName" path (with extension)
        /// </summary>
        /// <param name="card"></param>
        /// <param name="fileName"></param>
        public static void SaveTo(this YgoSet set, string fileName)
        {
            string data = JsonConvert.SerializeObject(set, Formatting.Indented);
            File.WriteAllText(fileName, data);
        }


        #endregion Set
    }
}
