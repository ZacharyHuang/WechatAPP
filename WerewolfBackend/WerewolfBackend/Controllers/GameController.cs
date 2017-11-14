using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WerewolfBackend.Models;
using WerewolfBackend.Utils;

namespace WerewolfBackend.Controllers
{
    public class GameController : Controller
    {
        public ActionResult StartGame(string roomId)
        {
            // check game
            var game = GameDB.GetGame(roomId);
            if (game == null)
            {
                return Json("Game not exist", JsonRequestBehavior.AllowGet);
            }

            // check player ready
            List<Player> players = new List<Player>();
            for (int i = 0; i < game.Config.PlayerNumber; ++i)
            {
                var player = RoomDB.GetPlayer(roomId, i);
                if (player == null || player.State != PlayerState.Ready)
                {
                    return Json("Some player are not ready", JsonRequestBehavior.AllowGet);
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

            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetGame(string roomId)
        {
            var game = GameDB.GetGame(roomId);
            if (game == null)
            {
                return Json("Game not exist", JsonRequestBehavior.AllowGet);
            }

            return Json(game, JsonRequestBehavior.AllowGet);
        }
    }
}