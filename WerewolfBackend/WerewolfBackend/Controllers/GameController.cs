using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WerewolfBackend.Models;
using WerewolfBackend.Utils;

namespace WerewolfBackend.Controllers
{
    class Card
    {
        public Character Character;
        public int Count;
        public Card(Character character, int count)
        {
            Character = character;
            Count = count;
        }
    }
    public class GameController : Controller
    {
        public void InitGame(Game game)
        {
            if (game.Config == null) return;
            if (game.Characters == null) game.Characters = new Character[game.Config.PlayerNumber];
            if (game.Status == null) game.Status = new GameStatus();

            // date and stage
            game.Status.Date = 0;
            game.Status.Stage = GameStage.Prepare;

            // character skills
            game.Status.CanProphetCheck = game.Config.ProphetNumber > 0;
            game.Status.CanWitchHeal = game.Config.WitchNumber > 0;
            game.Status.CanWitchPoison = game.Config.WitchNumber > 0;
            game.Status.CanHunterShoot = game.Config.HunterNumber > 0;
            game.Status.CanGuardGuard = game.Config.GuardNumber > 0;
            game.Status.CanDemonCheck = game.Config.DemonNumber > 0;

            // choose character for each seat
            game.Status.Lovers.Clear();
            game.Status.ThiefCandidates.Clear();
            int characterNumber = game.Config.PlayerNumber + (game.Config.ThiefNumber > 0 ? 2 : 0);
            var rand = RandomUtil.GenRandomArray(characterNumber);
            List<Card> cards = new List<Card>();
            cards.Add(new Card(Character.Thief, game.Config.ThiefNumber));
            cards.Add(new Card(Character.Prophet, game.Config.ProphetNumber));
            cards.Add(new Card(Character.Witch, game.Config.WitchNumber));
            cards.Add(new Card(Character.Hunter, game.Config.HunterNumber));
            cards.Add(new Card(Character.Guard, game.Config.GuardNumber));
            cards.Add(new Card(Character.Idiot, game.Config.IdiotNumber));
            cards.Add(new Card(Character.Demon, game.Config.DemonNumber));
            cards.Add(new Card(Character.WhiteWerewolf, game.Config.WhiteWerewolfNumber));
            cards.Add(new Card(Character.Village, game.Config.VillageNumber));
            cards.Add(new Card(Character.Werewolf, game.Config.WerewolfNumber));
            for (int i = 0; i < characterNumber; ++i)
            {
                for (int j = 0; j < cards.Count; ++j)
                {
                    if (cards[j].Count > 0)
                    {
                        if (rand[i] >= game.Config.PlayerNumber && cards[j].Character == Character.Thief) continue;
                        --cards[j].Count;
                        if (rand[i] < game.Config.PlayerNumber) { game.Characters[rand[i]] = cards[j].Character; }
                        else { game.Status.ThiefCandidates.Add(cards[j].Character); }
                    }
                }
            }

        }

        //public ActionResult StartGame(string roomId)
        //{
            
        //}
    }
}