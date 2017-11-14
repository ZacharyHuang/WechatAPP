using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WerewolfBackend.Models
{
    public enum PlayerState
    {
        None,
        Sit,
        Ready,
        Playing
    }
    public class Player
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string AvatarUrl { get; set; }
        public int SeatNumber { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public PlayerState State { get; set; }
    }
}