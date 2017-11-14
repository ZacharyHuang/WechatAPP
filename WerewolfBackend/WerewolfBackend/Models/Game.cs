using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WerewolfBackend.Models
{
    public enum GameStage
    {
        None,
        Prepare,
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

        public List<int> Lovers { get; set; }
        public int ThiefSeatNumber { get; set; }
        public List<Character> ThiefCandidates { get; set; }

        public int Date { get; set; }
        public GameStage Stage { get; set; }

        public GameStatus()
        {
            Lovers = new List<int>();
            ThiefCandidates = new List<Character>();
        }
    }
}