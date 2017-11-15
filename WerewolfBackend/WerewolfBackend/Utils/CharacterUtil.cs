using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WerewolfBackend.Models;

namespace WerewolfBackend.Utils
{
    public enum Camp
    {
        None,
        God,
        Village,
        Werewolf
    }
    public class CharacterUtil
    {
        public static Camp CheckCamp(Character character)
        {
            switch (character)
            {
                case Character.Prophet:
                case Character.Witch:
                case Character.Hunter:
                case Character.Guard:
                case Character.Idiot:
                case Character.Cupid:
                    return Camp.God;
                case Character.Village:
                    return Camp.Village;
                case Character.Werewolf:
                case Character.Demon:
                case Character.WhiteWerewolf:
                    return Camp.Werewolf;
                default:
                    return Camp.None;
            }
        }
    }
}