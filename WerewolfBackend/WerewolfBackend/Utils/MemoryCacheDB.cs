using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using WerewolfBackend.Models;

namespace WerewolfBackend.Utils
{
    public class GameDB
    {
        static MemoryCache cache = new MemoryCache("Game");
        static CacheItemPolicy policy = new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromHours(1) };

        public static string NewGame()
        {
            string roomId = RandomUtil.GenRandomInt(999999).ToString("D6");
            Game game = new Game();
            while (!cache.Add(roomId, game, policy))
            {
                roomId = RandomUtil.GenRandomInt(999999).ToString("D6");
            }
            return roomId;
        }
        public static Game GetGame(string roomId)
        {
            return cache[roomId] as Game;
        }
        public static void SetGame(string roomId, Game game)
        {
            cache[roomId] = game;
        }
        public static void RemoveGame(string roomId)
        {
            cache.Remove(roomId);
        }
    }
    public class RoomDB
    {
        static MemoryCache cache = new MemoryCache("Room");
        static CacheItemPolicy policy = new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromHours(1) };


        public static bool AddPlayer(string roomId, Player player)
        {
            string seatKey = roomId + "_" + player.SeatNumber.ToString();
            return cache.Add(seatKey, player, policy);
        }

        public static Player GetPlayer(string roomId, string userId)
        {
            for (int i = 0; i < 20; ++i)
            {
                string sk = roomId + "_" + i.ToString();
                var player = cache[sk] as Player;
                if (player != null && player.UserId == userId)
                {
                    return player;
                }
            }
            return null;
        }

        public static bool SetPlayer(string roomId, Player player)
        {
            if (GetPlayer(roomId, player.UserId) != null)
            {
                string seatKey = roomId + "_" + player.SeatNumber.ToString();
                cache[seatKey] = player;
                return true;
            }
            return false;
        }

        public static bool RemovePlayer(string roomId, string userId)
        {
            var player = GetPlayer(roomId, userId);
            if (player != null)
            {
                string seatKey = roomId + "_" + player.SeatNumber.ToString();
                cache.Remove(seatKey);
                return true;
            }
            return false;
        }

        public static void EmptySeat(string roomId, int seatNumber)
        {
            string seatKey = roomId + "_" + seatNumber.ToString();
            cache.Remove(seatKey);
        }
    }
}