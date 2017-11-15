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

            // status clear
            Status.Date = 0;
            Status.Stage = GameStage.DayTime;
            Status.CanProphetCheck = false;
            Status.CanWitchHeal = false;
            Status.CanWitchPoison = false;
            Status.CanHunterShoot = false;
            Status.CanGuardGuard = false;
            Status.CanDemonCheck = false;
            Status.CanThiefChoose = false;
            Status.CanCupidMakeCouple = false;

            // prepare all characters
            if (Characters == null || Characters.Length != Config.PlayerNumber + 1) { Characters = new Character[Config.PlayerNumber + 1]; }
            int characterNumber = Config.PlayerNumber + (Config.ThiefNumber > 0 ? 2 : 0);
            List<Character> characters = new List<Character>();
            for (int i = 0; i < Config.ThiefNumber; ++i) characters.Add(Character.Thief);
            for (int i = 0; i < Config.ProphetNumber; ++i) characters.Add(Character.Prophet);
            for (int i = 0; i < Config.WitchNumber; ++i) characters.Add(Character.Witch);
            for (int i = 0; i < Config.HunterNumber; ++i) characters.Add(Character.Hunter);
            for (int i = 0; i < Config.GuardNumber; ++i) characters.Add(Character.Guard);
            for (int i = 0; i < Config.IdiotNumber; ++i) characters.Add(Character.Idiot);
            for (int i = 0; i < Config.DemonNumber; ++i) characters.Add(Character.Demon);
            for (int i = 0; i < Config.WhiteWerewolfNumber; ++i) characters.Add(Character.WhiteWerewolf);
            for (int i = 0; i < Config.VillageNumber; ++i) characters.Add(Character.Village);
            for (int i = 0; i < Config.WerewolfNumber; ++i) characters.Add(Character.Werewolf);

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
            Characters[0] = Character.None;
            for (int i = 1; i <= Characters.Length; ++i)
            {
                int rand = RandomUtil.GenRandomInt(characters.Count);
                Characters[i] = characters[rand];
                switch (Characters[i])
                {
                    case Character.Prophet:
                        Status.CanProphetCheck = true;
                        break;
                    case Character.Witch:
                        Status.CanWitchHeal = true;
                        Status.CanWitchPoison = true;
                        break;
                    case Character.Hunter:
                        Status.CanHunterShoot = true;
                        break;
                    case Character.Guard:
                        Status.CanGuardGuard = true;
                        break;
                    case Character.Thief:
                        Status.CanThiefChoose = true;
                        break;
                    case Character.Cupid:
                        Status.CanCupidMakeCouple = true;
                        break;
                    case Character.Demon:
                        Status.CanDemonCheck = true;
                        break;
                }
                characters.RemoveAt(rand);
            }
        }

        public void NextStage()
        {
            if (Status.Stage == GameStage.DayTime)
            {
                Status.Stage =
                    (Status.Date == 0 && Config.ThiefNumber > 0) ? GameStage.ThiefNight :
                    (Status.Date == 0 && Config.CupidNumber > 0) ? GameStage.CupidNight :
                    GameStage.WerewolfNight;
            }
            else if (Status.Stage == GameStage.ThiefNight)
            {
                Status.Stage =
                    Config.CupidNumber > 0 ? GameStage.CupidNight :
                    GameStage.WerewolfNight;
            }
            else if (Status.Stage == GameStage.CupidNight)
            {
                Status.Stage = GameStage.LoversNight;
            }
            else if (Status.Stage == GameStage.LoversNight)
            {
                Status.Stage = GameStage.WerewolfNight;
            }
            else if (Status.Stage == GameStage.WerewolfNight)
            {
                Status.Stage =
                    Config.WitchNumber > 0 ? GameStage.WitchNight :
                    Config.ProphetNumber > 0 ? GameStage.ProphetNight :
                    Config.GuardNumber > 0 ? GameStage.GuardNight :
                    Config.DemonNumber > 0 ? GameStage.DemonNight :
                    GameStage.DayTime;
            }
            else if (Status.Stage == GameStage.WitchNight)
            {
                Status.Stage =
                    Config.ProphetNumber > 0 ? GameStage.ProphetNight :
                    Config.GuardNumber > 0 ? GameStage.GuardNight :
                    Config.DemonNumber > 0 ? GameStage.DemonNight :
                    GameStage.DayTime;
            }
            else if (Status.Stage == GameStage.ProphetNight)
            {
                Status.Stage =
                    Config.GuardNumber > 0 ? GameStage.GuardNight :
                    Config.DemonNumber > 0 ? GameStage.DemonNight :
                    GameStage.DayTime;
            }
            else if (Status.Stage == GameStage.GuardNight)
            {
                Status.Stage =
                    Config.DemonNumber > 0 ? GameStage.DemonNight :
                    GameStage.DayTime;
            }
            else if (Status.Stage == GameStage.DemonNight)
            {
                Status.Stage = GameStage.DayTime;
            }

            // update status if day time comes
            if (Status.Stage == GameStage.DayTime)
            {
                List<int> dead1 = new List<int>();
                if (Status.Trace[Status.Date].WerewolfKill > 0)
                {
                    bool heal = Status.Trace[Status.Date].WitchHeal, guard = Status.Trace[Status.Date].GuardGuard == Status.Trace[Status.Date].WerewolfKill;
                    // demon won't die in the night， no heal no guard or heal guard in a same time will die
                    if (Characters[Status.Trace[Status.Date].WitchPoison] != Character.Demon && ((heal && guard) || !(heal || guard)))
                    {
                        dead1.Add(Status.Trace[Status.Date].WerewolfKill);
                    }
                }
                if (Status.Trace[Status.Date].WitchPoison > 0)
                {
                    // demon won't die in the night
                    if (Characters[Status.Trace[Status.Date].WitchPoison] != Character.Demon)
                    {
                        dead1.Add(Status.Trace[Status.Date].WitchPoison);
                    }
                    // hunter can't shoot if poisoned
                    if (Characters[Status.Trace[Status.Date].WitchPoison] == Character.Hunter)
                    {
                        Status.CanHunterShoot = false;
                    }
                }
                // check dead lovers and distinct dead
                HashSet<int> dead2 = new HashSet<int>();
                foreach (var dead in dead1)
                {
                    dead2.Add(dead);
                    if (Lovers[0] == dead) dead2.Add(Lovers[1]);
                    else if (Lovers[1] == dead) dead2.Add(Lovers[0]);
                }
                Status.Trace[Status.Date].Dead = dead2.ToList();
                // check dead skill
                foreach (var dead in dead2)
                {
                    if (Characters[dead] == Character.Prophet) Status.CanProphetCheck = false;
                    else if (Characters[dead] == Character.Witch) Status.CanWitchHeal = Status.CanWitchPoison = false;
                    else if (Characters[dead] == Character.Guard) Status.CanGuardGuard = false;
                }
                ++Status.Date;
            }
        }
        public void WerewolfKill(int seatNumber)
        {
            if (Status.Trace.Count < Status.Date) Status.Trace.Add(new GameTrace());
            Status.Trace[Status.Date].WerewolfKill = seatNumber;
        }
        public void WitchHeal()
        {
            if (Status.Trace.Count < Status.Date) Status.Trace.Add(new GameTrace());
            Status.Trace[Status.Date].WitchHeal = true;
        }
        public void WitchPoison(int seatNumber)
        {
            if (Status.Trace.Count < Status.Date) Status.Trace.Add(new GameTrace());
            Status.Trace[Status.Date].WitchPoison = seatNumber;
        }
        public Camp ProphetCheck(int seatNumber)
        {
            if (Status.Trace.Count < Status.Date) Status.Trace.Add(new GameTrace());
            Status.Trace[Status.Date].ProphetCheck = seatNumber;
            return CharacterUtil.CheckCamp(Characters[seatNumber]);
        }
        public bool GuardGuard(int seatNumber)
        {
            if (Status.Trace.Count < Status.Date) Status.Trace.Add(new GameTrace());
            if (Status.Date > 0 && seatNumber == Status.Trace[Status.Date - 1].GuardGuard) return false;
            Status.Trace[Status.Date].GuardGuard = seatNumber;
            return true;
        }
        public Camp DemonCheck(int seatNumber)
        {
            if (Status.Trace.Count < Status.Date) Status.Trace.Add(new GameTrace());
            Status.Trace[Status.Date].DemonCheck = seatNumber;
            return CharacterUtil.CheckCamp(Characters[seatNumber]);
        }

        public void ThiefChoose(int choice)
        {
            Characters[ThiefSeatNumber] = ThiefCandidates[choice];
            switch (Characters[ThiefSeatNumber])
            {
                case Character.Prophet:
                    Status.CanProphetCheck = true;
                    break;
                case Character.Witch:
                    Status.CanWitchHeal = true;
                    Status.CanWitchPoison = true;
                    break;
                case Character.Hunter:
                    Status.CanHunterShoot = true;
                    break;
                case Character.Guard:
                    Status.CanGuardGuard = true;
                    break;
                case Character.Cupid:
                    Status.CanCupidMakeCouple = true;
                    break;
                case Character.Demon:
                    Status.CanDemonCheck = true;
                    break;
            }
        }

        public void CupidMakeCouple(int lover1, int lover2)
        {
            Lovers[0] = lover1;
            Lovers[1] = lover2;
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

        public bool WitchHealSelf { get; set; }
        public bool WitchTwoSkillInOneNight { get; set; }
        public bool HealAndGuardIsDead { get; set; }
    }
    public class GameStatus
    {
        public bool CanProphetCheck { get; set; }
        public bool CanWitchHeal { get; set; }
        public bool CanWitchPoison { get; set; }
        public bool CanHunterShoot { get; set; }
        public bool CanGuardGuard { get; set; }
        public bool CanDemonCheck { get; set; }
        public bool CanThiefChoose { get; set; }
        public bool CanCupidMakeCouple { get; set; }


        public int Date { get; set; }
        public GameStage Stage { get; set; }
        public List<GameTrace> Trace { get; set; }
        public GameStatus()
        {
            Trace = new List<GameTrace>();
        }
    }
    public class GameTrace
    {
        public int WerewolfKill;
        public int WitchPoison;
        public bool WitchHeal;
        public int ProphetCheck;
        public int GuardGuard;
        public int DemonCheck;

        public List<int> Dead;
    }
}