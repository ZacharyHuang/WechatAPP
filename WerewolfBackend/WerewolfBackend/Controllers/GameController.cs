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
        public IHttpActionResult StartGame(string roomId)
        {
            // check game
            var game = GameDB.GetGame(roomId);
            if (game == null)
            {
                return BadRequest("Game not exist");
            }

            // check player ready
            List<Player> players = new List<Player>();
            for (int i = 0; i < game.Config.PlayerNumber; ++i)
            {
                var player = RoomDB.GetPlayer(roomId, i);
                if (player == null || player.State != PlayerState.Ready)
                {
                    return BadRequest("Some players are not ready");
                }
                players.Add(player);
            }

            // set players
            foreach (var player in players)
            {
                player.State = PlayerState.Playing;
                RoomDB.SetPlayer(roomId, player);
            }
            // init and set game
            game.InitGame();
            GameDB.SetGame(roomId, game);

            return Ok();
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

            if (game.Status.Stage == GameStage.Prepare)
            {
                return BadRequest("Game not start");
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
        public IHttpActionResult ThiefSkill(string roomId, int sourceSeatNumber, int choice)
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

            if (choice < 0 || choice > 1)
            {
                return BadRequest("No this choice");
            }

            game.ThiefChoose(choice);
            game.NextStage();
            GameDB.SetGame(roomId, game);

            return Ok();
        }
        [HttpGet]
        public IHttpActionResult CupidSkill(string roomId, int sourceSeatNumber, int target1, int target2)
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

            if (target1 <= 0 || target2 <= 0 || target1 > game.Config.PlayerNumber || target2 > game.Config.PlayerNumber || target1 == target2)
            {
                return BadRequest("Lovers illegal");
            }

            game.CupidMakeCouple(target1, target2);
            game.NextStage();
            GameDB.SetGame(roomId, game);

            return Ok();
        }

        [HttpGet]
        public IHttpActionResult WerewolfSkill(string roomId, int sourceSeatNumber, bool kill, int target)
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

            if (kill && (target <= 0 || target > game.Config.PlayerNumber))
            {
                return BadRequest("People you kill is illegal");
            }

            if (kill) game.WerewolfKill(target);
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

            if (heal && !game.Config.WitchHealSelf && game.Status.Date != 0 && game.Characters[game.Status.Trace[game.Status.Date].WerewolfKill] == Character.Witch)
            {
                return BadRequest("In this game, witch can not heal herself except first night");
            }

            if (heal) game.WitchHeal();
            if (poison) game.WitchPoison(target);
            game.NextStage();
            GameDB.SetGame(roomId, game);

            return Ok();
        }

        [HttpGet]
        public IHttpActionResult ProphetSkill(string roomId, int sourceSeatNumber, bool check, int target)
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

            if (check && (target <= 0 || target > game.Config.PlayerNumber))
            {
                return BadRequest("People you check is illegal");
            }

            var result = check ? (game.ProphetCheck(target) == Camp.Werewolf ? "Werewolf" : "Non-werewolf") : "";
            game.NextStage();
            GameDB.SetGame(roomId, game);

            return Ok(result);
        }

        [HttpGet]
        public IHttpActionResult GuardSkill(string roomId, int sourceSeatNumber, bool guard, int target)
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

            if (guard && (target <= 0 || target > game.Config.PlayerNumber))
            {
                return BadRequest("People you guard is illegal");
            }
            
            if (guard && (game.Status.Date > 0 && game.Status.Trace[game.Status.Date - 1].GuardGuard == target))
            {
                return BadRequest("Can not guard the same people as last night");
            }

            game.GuardGuard(target);
            game.NextStage();
            GameDB.SetGame(roomId, game);

            return Ok();
        }

        [HttpGet]
        public IHttpActionResult DemonSkill(string roomId, int sourceSeatNumber, bool check, int target)
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

            if (check && (target <= 0 || target > game.Config.PlayerNumber))
            {
                return BadRequest("People you check is illegal");
            }

            var result = check ? (game.DemonCheck(target) == Camp.God ? "God" : "Non-god") : "";
            game.NextStage();
            GameDB.SetGame(roomId, game);

            return Ok(result);
        }

    }
}