using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WerewolfBackend.Utils;

namespace WerewolfBackend.Models
{
    public enum GameStage
    {
        Prepare = 0,
        DayTime,
        ThiefNight,
        CupidNight,
        LoversNight,
        WerewolfNight,
        WitchNight,
        ProphetNight,
        GuardNight,
        DemonNight,
    }
    public class Game
    {
        public string RoomId { get; set; }

        public GameConfig Config { get; set; }

        public GameStatus Status { get; set; }

        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public Character[] Characters { get; set; }
        public int[] Lovers { get; set; }
        public int ThiefSeatNumber { get; set; }
        public Character[] ThiefCandidates { get; set; }
        public Game()
        {
            Config = new GameConfig();
            Status = new GameStatus();
        }

        public void InitGame()
        {
            if (Config == null) return;
            // check thief and cupid 
            if (Config.ThiefNumber > 0 && ThiefCandidates == null) ThiefCandidates = new Character[2];
            else if (Config.ThiefNumber <= 0) { ThiefCandidates = null; ThiefSeatNumber = -1; }
            if (Config.CupidNumber > 0 && Lovers == null) Lovers = new int[2];
            else if (Config.CupidNumber <= 0) Lovers = null;

            // date and stage
            Status.Date = 0;
            Status.Stage = GameStage.Prepare;

            // character skills
            Status.CanProphetCheck = Config.ProphetNumber > 0;
            Status.CanWitchHeal = Config.WitchNumber > 0;
            Status.CanWitchPoison = Config.WitchNumber > 0;
            Status.CanHunterShoot = Config.HunterNumber > 0;
            Status.CanGuardGuard = Config.GuardNumber > 0;
            Status.CanDemonCheck = Config.DemonNumber > 0;

            // prepare all characters
            if (Characters == null || Characters.Length != Config.PlayerNumber) { Characters = new Character[Config.PlayerNumber]; }
            int characterNumber = Config.PlayerNumber + (Config.ThiefNumber > 0 ? 2 : 0);
            List<Character> characters = new List<Character>();
            for (int i = 0; i < Config.ThiefNumber; ++i) characters.Add(Character.Thief);
            for (int i = 0; i < Config.ProphetNumber; ++i) characters.Add(Character.Prophet);
            for (int i = 0; i < Config.ThiefNumber; ++i) characters.Add(Character.Witch);
            for (int i = 0; i < Config.ThiefNumber; ++i) characters.Add(Character.Hunter);
            for (int i = 0; i < Config.ThiefNumber; ++i) characters.Add(Character.Guard);
            for (int i = 0; i < Config.ThiefNumber; ++i) characters.Add(Character.Idiot);
            for (int i = 0; i < Config.ThiefNumber; ++i) characters.Add(Character.Demon);
            for (int i = 0; i < Config.ThiefNumber; ++i) characters.Add(Character.WhiteWerewolf);
            for (int i = 0; i < Config.ThiefNumber; ++i) characters.Add(Character.Village);
            for (int i = 0; i < Config.ThiefNumber; ++i) characters.Add(Character.Werewolf);

            // choose thief candidates
            if (Config.ThiefNumber > 0)
            {
                int rand = 0;
                do { rand = RandomUtil.GenRandomInt(characters.Count); }
                while (characters[rand] == Character.Thief);
                ThiefCandidates[0] = characters[rand];
                characters.RemoveAt(rand);

                do { rand = RandomUtil.GenRandomInt(characters.Count); }
                while (characters[rand] == Character.Thief);
                ThiefCandidates[1] = characters[rand];
                characters.RemoveAt(rand);
            }

            // choose character for each seat
            for (int i = 0; i < Characters.Length; ++i)
            {
                int rand = RandomUtil.GenRandomInt(characters.Count);
                Characters[i] = characters[rand];
                characters.RemoveAt(rand);
            }

        }
    }
    public class GameConfig
    {
        public int ProphetNumber { get; set; }
        public int WitchNumber { get; set; }
        public int HunterNumber { get; set; }
        public int GuardNumber { get; set; }
        public int IdiotNumber { get; set; }
        public int CupidNumber { get; set; }

        public int DemonNumber { get; set; }
        public int WhiteWerewolfNumber { get; set; }

        public int ThiefNumber { get; set; }

        public int PlayerNumber { get; set; }
        public int VillageNumber { get; set; }
        public int WerewolfNumber { get; set; }
    }
    public class GameStatus
    {
        public bool CanProphetCheck { get; set; }
        public bool CanWitchHeal { get; set; }
        public bool CanWitchPoison { get; set; }
        public bool CanHunterShoot { get; set; }
        public bool CanGuardGuard { get; set; }
        public bool CanDemonCheck { get; set; }


        public int Date { get; set; }
        public GameStage Stage { get; set; }
    }
}