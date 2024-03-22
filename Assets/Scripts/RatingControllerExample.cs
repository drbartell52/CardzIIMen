using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Gameboard.Examples
{
    /// <summary>
    /// The rating controller handles sending and receiving ratings for this specific game.
    /// </summary>
    public class RatingControllerExample : MonoBehaviour
    {
        private UserPresenceController userPresenceController;
        private RatingController ratingController;

        public Text results;

        private void Start()
        {
            GameObject gameboardObject = GameObject.FindWithTag("Gameboard");

            userPresenceController = gameboardObject.GetComponent<UserPresenceController>();
            ratingController = gameboardObject.GetComponent<RatingController>();
        }

        public void RateGameForBoardUser()
        {
            ratingController.RateGame(7.5);
        }

        public void RateGameForSpecificUser()
        {
            if (userPresenceController.Users.Count > 0)
            {
                var userId = userPresenceController.Users.First().Key;
                if (userId != null)
                {
                    ratingController.RateGameForUser(6.5, userId);
                }
            }
            else
            {
                results.text = $"No user's currently found.";
            }
        }

        public void GetGameRatingForBoardUser()
        {
            var rating = ratingController.GetGameRating();
            results.text = $"Game's rating from board user: {rating}";
        }

        public void GetGameRatingForSpecificUser()
        {
            if (userPresenceController.Users.Count > 0)
            {
                var userId = userPresenceController.Users.First().Key;
                if (userId != null)
                {
                    var rating = ratingController.GetGameRatingForUser(userId);
                    results.text = $"Game's rating from user {userId}: {rating}";
                }
            }
            else
            {
                results.text = $"No user's currently found.";
            }
        }

        public void GetGamesAverageRating()
        {
            var rating = ratingController.GetGameAverageRating();
            results.text = $"Game's average rating: {rating}";
        }
    }
}
