using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WerewolfBackend.Models;
using WerewolfBackend.Utils;

namespace WerewolfBackend.Controllers
{
    public class GameController : ApiController
    {
        [HttpGet]
        public IHttpActionResult CreateGame(
            int villageNumber,
            int werewolfNumber,
            int prophet = 0,
            int witch = 0,
            int hunter = 0,
            int guard = 0,
            int idiot = 0,
            int cupid = 0,
            int demon = 0,
            int whiteWerewolf = 0,
            int thief = 0,
            int witchHealSelf = 1,
            bool witchTwoSkillInOneNight = false,
            bool healAndGuardIsDead = true
            )
        {
            int prophetNumber = prophet > 0 ? 1 : 0,
                witchNumber = witch > 0 ? 1 : 0,
                hunterNumber = hunter > 0 ? 1 : 0,
                guardNumber = guard > 0 ? 1 : 0,
                idiotNumber = idiot > 0 ? 1 : 0,
                cupidNumber = cupid > 0 ? 1 : 0,
                demonNumber = demon > 0 ? 1 : 0,
                whiteWerewolfNumber = whiteWerewolf > 0 ? 1 : 0,
                thiefNumber = thief > 0 ? 1 : 0;

            int playerNumber = villageNumber + werewolfNumber + prophetNumber + witchNumber + +hunterNumber + guardNumber + idiotNumber + cupidNumber + demonNumber + whiteWerewolfNumber - thiefNumber;

            if (playerNumber <= 3)
            {
                return BadRequest("Player number is not enough");
            }

            GameConfig config = new GameConfig
            {
                VillageNumber = villageNumber,
                WerewolfNumber = werewolfNumber,
                PlayerNumber = playerNumber,
                ProphetNumber = prophetNumber,
                WitchNumber = witchNumber,
                HunterNumber = hunterNumber,
                GuardNumber = guardNumber,
                IdiotNumber = idiotNumber,
                CupidNumber = cupidNumber,
                DemonNumber = demonNumber,
                WhiteWerewolfNumber = whiteWerewolfNumber,
                ThiefNumber = thiefNumber,
                WitchHealSelf = witchHealSelf,
                WitchTwoSkillInOneNight = witchTwoSkillInOneNight,
                HealAndGuardIsDead = healAndGuardIsDead
            };

            string roomId = GameDB.NewGame();

            Game game = new Game();
            game.RoomId = roomId;
            game.Config = config;
            game.InitGame();

            GameDB.SetGame(roomId, game);
            return Ok(roomId);
        }
        [HttpGet]
        public IHttpActionResult GetGameConfig(string roomId)
        {
            var game = GameDB.GetGame(roomId);
            if (game == null)
            {
                return BadRequest("Game not exist");
            }

            return Ok(JsonConvert.SerializeObject(game.Config));
        }

        [HttpGet]
        public IHttpActionResult GetCharacter(string roomId, int seatNumber)
        {
            var game = GameDB.GetGame(roomId);
            if (game == null)
            {
                return BadRequest("Game not exist");
            }

            if (seatNumber <= 0 || seatNumber > game.Config.PlayerNumber)
            {
                return BadRequest("Seat number is illegal");
            }

            return Ok(game.Characters[seatNumber].ToString());
        }

        [HttpGet]
        public IHttpActionResult GetGameStage(string roomId)
        {
            var game = GameDB.GetGame(roomId);
            if (game == null)
            {
                return BadRequest("Game not exist");
            }
            return Ok(game.Status.Stage.ToString());
        }

        [HttpGet]
        public IHttpActionResult NightFall(string roomId)
        {
            var game = GameDB.GetGame(roomId);
            if (game == null)
            {
                return BadRequest("Game not exist");
            }

            if (game.Status.Stage != GameStage.Prepare && game.Status.Stage != GameStage.DayTime)
            {
                return BadRequest("Not now");
            }

            int playerNumber = game.Config.PlayerNumber;
            for (int i = 1; i <= playerNumber; ++i)
            {
                var player = RoomDB.GetPlayer(roomId, i);
                if (player == null || player.State != PlayerState.Ready)
                {
                    return BadRequest("Player " + i.ToString() + " not ready");
                }
            }

            game.NextStage();

            return Ok();
        }

        [HttpGet]
        public IHttpActionResult GetNightInfo(string roomId)
        {
            var game = GameDB.GetGame(roomId);
            if (game == null)
            {
                return BadRequest("Game not exist");
            }

            if (game.Status.Stage != GameStage.DayTime || game.Status.Date < 1)
            {
                return BadRequest("Not now");
            }

            var dead = game.Status.Trace[game.Status.Date - 1].Dead;

            return Ok(dead);
        }

        [HttpGet]
        public IHttpActionResult ThiefSkill(string roomId, int sourceSeatNumber, bool useSkill, int choice)
        {
            var game = GameDB.GetGame(roomId);
            if (game == null)
            {
                return BadRequest("Game not exist");
            }

            if (!game.Status.CanThiefChoose)
            {
                return BadRequest("Game has no thief");
            }

            if (game.Status.Stage != GameStage.ThiefNight)
            {
                return BadRequest("Not your turn");
            }
            
            if (sourceSeatNumber <= 0 || sourceSeatNumber > game.Config.PlayerNumber)
            {
                return BadRequest("Seat number is illegal");
            }

            if (game.Characters[sourceSeatNumber] != Character.Thief)
            {
                return BadRequest("You are not the thief");
            }

            if (useSkill && (choice < 0 || choice > 1))
            {
                return BadRequest("No this choice");
            }

            if (useSkill) game.ThiefChoose(choice);
            game.NextStage();
            GameDB.SetGame(roomId, game);

            return Ok();
        }
        [HttpGet]
        public IHttpActionResult CupidSkill(string roomId, int sourceSeatNumber, bool useSkill, int target1, int target2)
        {
            var game = GameDB.GetGame(roomId);
            if (game == null)
            {
                return BadRequest("Game not exist");
            }

            if (!game.Status.CanCupidMakeCouple)
            {
                return BadRequest("Game has no cupid");
            }

            if (game.Status.Stage != GameStage.CupidNight)
            {
                return BadRequest("Not your turn");
            }

            if (sourceSeatNumber <= 0 || sourceSeatNumber > game.Config.PlayerNumber)
            {
                return BadRequest("Seat number is illegal");
            }

            if (game.Characters[sourceSeatNumber] != Character.Cupid)
            {
                return BadRequest("You are not the cupid");
            }

            if (useSkill && (target1 <= 0 || target2 <= 0 || target1 > game.Config.PlayerNumber || target2 > game.Config.PlayerNumber || target1 == target2))
            {
                return BadRequest("Lovers illegal");
            }

            if (useSkill) game.CupidMakeCouple(target1, target2);
            game.NextStage();
            GameDB.SetGame(roomId, game);

            return Ok();
        }

        [HttpGet]
        public IHttpActionResult WerewolfSkill(string roomId, int sourceSeatNumber, bool useSkill, int target)
        {
            var game = GameDB.GetGame(roomId);
            if (game == null)
            {
                return BadRequest("Game not exist");
            }

            if (game.Status.Stage != GameStage.WerewolfNight)
            {
                return BadRequest("Not your turn");
            }

            if (sourceSeatNumber <= 0 || sourceSeatNumber > game.Config.PlayerNumber)
            {
                return BadRequest("Seat number is illegal");
            }

            if (game.Characters[sourceSeatNumber] != Character.Werewolf || game.Characters[sourceSeatNumber] != Character.Demon || game.Characters[sourceSeatNumber] != Character.WhiteWerewolf)
            {
                return BadRequest("You can not kill people");
            }

            if (useSkill && (target <= 0 || target > game.Config.PlayerNumber))
            {
                return BadRequest("People you kill is illegal");
            }

            if (useSkill) game.WerewolfKill(target);
            game.NextStage();
            GameDB.SetGame(roomId, game);

            return Ok();
        }

        [HttpGet]
        public IHttpActionResult WitchSkill(string roomId, int sourceSeatNumber, bool heal, bool poison, int target)
        {
            var game = GameDB.GetGame(roomId);
            if (game == null)
            {
                return BadRequest("Game not exist");
            }

            if (game.Status.Stage != GameStage.WitchNight)
            {
                return BadRequest("Not your turn");
            }

            if (sourceSeatNumber <= 0 || sourceSeatNumber > game.Config.PlayerNumber)
            {
                return BadRequest("Seat number is illegal");
            }

            if (game.Characters[sourceSeatNumber] != Character.Witch)
            {
                return BadRequest("You are not the witch");
            }

            if (heal && !game.Status.CanWitchHeal)
            {
                return BadRequest("Witch can't heal now");
            }

            if (poison && !game.Status.CanWitchPoison)
            {
                return BadRequest("Witch can't poison now");
            }

            if (heal && poison && !game.Config.WitchTwoSkillInOneNight)
            {
                return BadRequest("In this game, witch can not use two skill in one night");
            }

            if (poison && (target <= 0 || target > game.Config.PlayerNumber))
            {
                return BadRequest("Poisoned poeple is illegal");
            }

            if (heal && game.Status.Trace[game.Status.Date].WerewolfKill == 0)
            {
                return BadRequest("No poeple died");
            }

            if (heal && game.Characters[game.Status.Trace[game.Status.Date].WerewolfKill] == Character.Witch && (game.Config.WitchHealSelf == 0 || (game.Config.WitchHealSelf == 1 && game.Status.Date != 0)))
            {
                return BadRequest("In this game, witch can not heal herself now");
            }

            if (heal) game.WitchHeal();
            if (poison) game.WitchPoison(target);
            game.NextStage();
            GameDB.SetGame(roomId, game);

            return Ok();
        }

        [HttpGet]
        public IHttpActionResult ProphetSkill(string roomId, int sourceSeatNumber, bool useSkill, int target)
        {
            var game = GameDB.GetGame(roomId);
            if (game == null)
            {
                return BadRequest("Game not exist");
            }

            if (game.Status.Stage != GameStage.ProphetNight)
            {
                return BadRequest("Not your turn");
            }

            if (sourceSeatNumber <= 0 || sourceSeatNumber > game.Config.PlayerNumber)
            {
                return BadRequest("Seat number is illegal");
            }

            if (game.Characters[sourceSeatNumber] != Character.Prophet)
            {
                return BadRequest("You are not the prophet");
            }

            if (useSkill && (target <= 0 || target > game.Config.PlayerNumber))
            {
                return BadRequest("People you check is illegal");
            }

            var result = useSkill ? (game.ProphetCheck(target) == Camp.Werewolf ? "Werewolf" : "Non-werewolf") : "";
            game.NextStage();
            GameDB.SetGame(roomId, game);

            return Ok(result);
        }

        [HttpGet]
        public IHttpActionResult GuardSkill(string roomId, int sourceSeatNumber, bool useSkill, int target)
        {
            var game = GameDB.GetGame(roomId);
            if (game == null)
            {
                return BadRequest("Game not exist");
            }

            if (game.Status.Stage != GameStage.GuardNight)
            {
                return BadRequest("Not your turn");
            }

            if (sourceSeatNumber <= 0 || sourceSeatNumber > game.Config.PlayerNumber)
            {
                return BadRequest("Seat number is illegal");
            }

            if (game.Characters[sourceSeatNumber] != Character.Guard)
            {
                return BadRequest("You are not the guard");
            }

            if (useSkill && (target <= 0 || target > game.Config.PlayerNumber))
            {
                return BadRequest("People you guard is illegal");
            }
            
            if (useSkill && (game.Status.Date > 0 && game.Status.Trace[game.Status.Date - 1].GuardGuard == target))
            {
                return BadRequest("Can not guard the same people as last night");
            }

            game.GuardGuard(target);
            game.NextStage();
            GameDB.SetGame(roomId, game);

            return Ok();
        }

        [HttpGet]
        public IHttpActionResult DemonSkill(string roomId, int sourceSeatNumber, bool useSkill, int target)
        {
            var game = GameDB.GetGame(roomId);
            if (game == null)
            {
                return BadRequest("Game not exist");
            }

            if (game.Status.Stage != GameStage.DemonNight)
            {
                return BadRequest("Not your turn");
            }

            if (sourceSeatNumber <= 0 || sourceSeatNumber > game.Config.PlayerNumber)
            {
                return BadRequest("Seat number is illegal");
            }

            if (game.Characters[sourceSeatNumber] != Character.Demon)
            {
                return BadRequest("You are not the demon");
            }

            if (useSkill && (target <= 0 || target > game.Config.PlayerNumber))
            {
                return BadRequest("People you check is illegal");
            }

            var result = useSkill ? (game.DemonCheck(target) == Camp.God ? "God" : "Non-god") : "";
            game.NextStage();
            GameDB.SetGame(roomId, game);

            return Ok(result);
        }

    }
}