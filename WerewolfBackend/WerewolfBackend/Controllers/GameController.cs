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
    }
}