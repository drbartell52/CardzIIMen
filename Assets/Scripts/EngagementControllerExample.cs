using UnityEngine;
using Gameboard.Objects.Engagement;
using System.Collections.Generic;
using UnityEngine.UI;
using Gameboard.Objects.Ranking;
using System.Linq;

namespace Gameboard.Examples
{
    class EngagementControllerExample : MonoBehaviour
    {
        EngagementController engagementController;
        UserPresenceController userPresenceController;

        private int currentRound;
        private List<RankingEntry> rankingEntries;

        public Text Results;

        private void Start()
        {
            GameObject gameboardObject = GameObject.FindWithTag("Gameboard");
            engagementController = gameboardObject.GetComponent<EngagementController>();
            userPresenceController = gameboardObject.GetComponent<UserPresenceController>();
            rankingEntries = new List<RankingEntry>();
        }

        public void TestGameSessionStarted()
        {
            if (userPresenceController.Users == null || userPresenceController.Users.Count <= 0)
            {
                Results.text = "There are no users connected, no metric sent.";
                return;
            }

            engagementController.SendGameSessionStarted(new List<string>(userPresenceController.Users.Keys));
            Results.text = "Sent SendGameSessionStarted";
        }

        public void TestGameSessionEnded()
        {
            if (userPresenceController.Users == null || userPresenceController.Users.Count <= 0)
            {
                Results.text = "There are no users connected, no metric sent.";
                return;
            }

            engagementController.SendGameSessionEnded(new List<string>(userPresenceController.Users.Keys));
            Results.text = "Sent SendGameSessionEnded";
        }

        public void TestUserIdsInSession()
        {
            if (userPresenceController.Users == null || userPresenceController.Users.Count <= 0)
            {
                Results.text = "There are no users connected, no metric sent.";
                return;
            }

            engagementController.SendUserIdsInSession(new List<string>(userPresenceController.Users.Keys));
            Results.text = "Sent SendUserIdsInSession";

        }

        public void TestSessionRoundOutcome()
        {
            if (userPresenceController.Users == null || userPresenceController.Users.Count <= 0)
            {
                Results.text = "There are no users connected, no metric sent.";
                return;
            }

            string randomUserId = userPresenceController.Users.Keys.ToList()[UnityEngine.Random.Range(0, userPresenceController.Users.Count)];
            var winners = new List<string>() { randomUserId };
            RankingEntry ranking = new RankingEntry(winners, 1, UnityEngine.Random.Range(0, 10));
            rankingEntries.Add(ranking);

            RankingReport report = new RankingReport(rankingEntries, RankingReportType.RoundEnd, currentRound);
            RankingReportMetric rankingReportMetric = new RankingReportMetric(report);
            engagementController.SendRankingReport(rankingReportMetric);
            Results.text = $"Sent RankingReportMetric, type=RoundEnd, for round {currentRound}";
            currentRound++;
        }

        public void TestDefaultRankingReport()
        {
            if (userPresenceController.Users == null || userPresenceController.Users.Count <= 0)
            {
                Results.text = "There are no users connected, no metric sent.";
                return;
            }

            if (rankingEntries.Count <= 0)
            {
                // Add a random entry so we can report it

                string randomUserId = userPresenceController.Users.Keys.ToList()[UnityEngine.Random.Range(0, userPresenceController.Users.Count)];
                var teamMembers = new List<string>() { randomUserId };

                rankingEntries.Add(new RankingEntry(teamMembers, 1, UnityEngine.Random.Range(0, 10)));
            }

            RankingReport report = new RankingReport(rankingEntries, RankingReportType.GameFinished);
            RankingReportMetric rankingReportMetric = new RankingReportMetric(report);
            engagementController.SendRankingReport(rankingReportMetric);
            Results.text = "Sent RankingReportMetric, type=GameFinished";
        }
    }
}

