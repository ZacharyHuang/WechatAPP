using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WerewolfBackend.Models;
using WerewolfBackend.Utils;

namespace WerewolfBackend.Controllers
{
    public class RoomController : Controller
    {
        public ActionResult CreateRoom(
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
            int thief = 0
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
                ThiefNumber = thiefNumber
            };

            string roomId = GameDB.NewGame();

            Game game = new Game()
            {
                RoomId = roomId,
                Config = config
            };

            GameDB.SetGame(roomId, game);
            return Json("Success");
        }
        public ActionResult TakeSeat(string roomId, int seatNumber, string userId, string userName, string avatarUrl)
        {

            // check whether palyer is already sit
            var p = RoomDB.GetPlayer(roomId, userId);
            if (p != null && p.State == PlayerState.Ready)
            {
                return Json("Player is ready");
            }

            string seatKey = roomId + "_" + seatNumber.ToString();
            Player player = new Player()
            {
                UserId = userId,
                UserName = userName,
                AvatarUrl = avatarUrl,
                SeatNumber = seatNumber,
                State = PlayerState.Sit
            };

            // try to take the seat
            if (RoomDB.AddPlayer(roomId, player))
            {
                if (p != null)
                {
                    RoomDB.RemovePlayer(roomId, p.SeatNumber);
                }
                return Json("Success");
            }
            else
            {
                return Json("Seat is token");
            }
        }

        public ActionResult LeaveSeat(string roomId, string userId)
        {
            RoomDB.RemovePlayer(roomId, userId);
            return Json("Success");
        }

        public ActionResult Prepare(string roomId, string userId)
        {
            var player = RoomDB.GetPlayer(roomId, userId);
            if (player == null)
            {
                return Json("Player has no seat");
            }
            player.State = PlayerState.Ready;
            RoomDB.SetPlayer(roomId, player);
            return Json("Success");
        }

        public ActionResult UnPrepare(string roomId, string userId)
        {
            var player = RoomDB.GetPlayer(roomId, userId);
            if (player == null)
            {
                return Json("Player has no seat");
            }
            player.State = PlayerState.Sit;
            RoomDB.SetPlayer(roomId, player);
            return Json("Success");
        }

    }
}