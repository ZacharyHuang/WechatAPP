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
    public class RoomController : ApiController
    {
        [HttpGet]
        public IHttpActionResult GetPlayers(string roomId)
        {
            var game = GameDB.GetGame(roomId);
            if (game == null || game.Config == null)
            {
                return BadRequest("Room not exist");
            }

            int playerNumber = game.Config.PlayerNumber;
            Player[] players = new Player[playerNumber + 1];
            for (int i = 1; i <= playerNumber; ++i)
            {
                var player = RoomDB.GetPlayer(roomId, i);
                players[i] = player;
            }

            return Ok(JsonConvert.SerializeObject(players));
        }

        [HttpGet]
        public IHttpActionResult TakeSeat(string roomId, int seatNumber, string userId, string userName, string avatarUrl)
        {

            // check whether palyer is already sit
            var p = RoomDB.GetPlayer(roomId, userId);
            if (p != null && p.State == PlayerState.Ready)
            {
                return BadRequest("Player is ready");
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
                return Ok();
            }
            else
            {
                return BadRequest("Seat is token");
            }
        }

        [HttpGet]
        public IHttpActionResult LeaveSeat(string roomId, string userId)
        {
            RoomDB.RemovePlayer(roomId, userId);
            return Ok();
        }

        [HttpGet]
        public IHttpActionResult Prepare(string roomId, string userId)
        {
            var player = RoomDB.GetPlayer(roomId, userId);
            if (player == null)
            {
                return BadRequest("Player has no seat");
            }
            player.State = PlayerState.Ready;
            RoomDB.SetPlayer(roomId, player);
            return Ok();
        }

        [HttpGet]
        public IHttpActionResult UnPrepare(string roomId, string userId)
        {
            var player = RoomDB.GetPlayer(roomId, userId);
            if (player == null)
            {
                return BadRequest("Player has no seat");
            }
            player.State = PlayerState.Sit;
            RoomDB.SetPlayer(roomId, player);
            return Ok();
        }

    }
}