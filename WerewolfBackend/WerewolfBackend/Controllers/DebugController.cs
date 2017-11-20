using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WerewolfBackend.Models;
using WerewolfBackend.Utils;

namespace WerewolfBackend.Controllers
{
    public class DebugController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Infos(string roomId)
        {
            var game = GameDB.GetGame(roomId);

            int playerNumber = game == null ? 20 : game.Config.PlayerNumber;
            Player[] players = new Player[playerNumber + 1];
            for (int i = 1; i <= playerNumber; ++i)
            {
                var player = RoomDB.GetPlayer(roomId, i);
                players[i] = player;
            }

            return Ok(JsonConvert.SerializeObject(new { game = game, players = players }, Formatting.Indented));
        }

        [HttpGet]
        public IHttpActionResult GenBots(string roomId)
        {
            var game = GameDB.GetGame(roomId);

            if (game == null) return BadRequest("game not exist");
            int playerNumber = game.Config.PlayerNumber;
            int botNumber = 0;
            Player[] players = new Player[playerNumber + 1];
            for (int i = 1; i <= playerNumber; ++i)
            {
                var player = RoomDB.GetPlayer(roomId, i);
                if (player == null)
                {
                    player = new Player() { UserId = "bot" + botNumber, UserName = "bot" + botNumber, SeatNumber = i, AvatarUrl = "", State = PlayerState.Ready };
                    RoomDB.AddPlayer(roomId, player);
                    ++botNumber;
                }
            }

            return Ok();
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
        public IHttpActionResult ThiefSkill(string roomId, bool useSkill, int choice)
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
        public IHttpActionResult CupidSkill(string roomId, bool useSkill, int target1, int target2)
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
        public IHttpActionResult WerewolfSkill(string roomId, bool useSkill, int target)
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
        public IHttpActionResult WitchSkill(string roomId, bool heal, bool poison, int target)
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

            if (heal && !game.Status.CanWitchHeal)
            {
                return BadRequest("Witch can't heal now");
            }

            if (poison && !game.Status.CanWitchPoison)
            {
                return BadRequest("Witch can't poison now");
            }

            if (poison && (target <= 0 || target > game.Config.PlayerNumber))
            {
                return BadRequest("People you poison is illegal");
            }

            if (heal && poison && !game.Config.WitchTwoSkillInOneNight)
            {
                return BadRequest("In this game, witch can not use two skill in one night");
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
        public IHttpActionResult ProphetSkill(string roomId, bool useSkill, int target)
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
        public IHttpActionResult GuardSkill(string roomId, bool useSkill, int target)
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
        public IHttpActionResult DemonSkill(string roomId, bool useSkill, int target)
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
