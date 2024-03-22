using UnityEngine;
using Gameboard.EventArgs;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Gameboard.Examples
{
    public class UserPresenceControllerExample : MonoBehaviour
    {
        public Text Results;

        private UserPresenceController userPresenceController;
        private PlayerArmingController playerArmingController;

        private void Start()
        {
            GameboardLogging.Verbose("UserPresenceExample Start");
            GameObject gameboardObject = GameObject.FindWithTag("Gameboard");
            userPresenceController = gameboardObject.GetComponent<UserPresenceController>();
            playerArmingController = gameboardObject.GetComponent<PlayerArmingController>();

            userPresenceController.OnUserPresence += OnUserPresence;

            if (userPresenceController.IsInitialized)
            {
                OnUserPresenceControllerInitialized();
            }
            else
            {
                userPresenceController.UserPresenceControllerInitialized += OnUserPresenceControllerInitialized;
            }
        }

        private void OnDestroy()
        {
            GameboardLogging.Verbose("UserPresenceExample OnDestroy");
            userPresenceController.UserPresenceControllerInitialized -= OnUserPresenceControllerInitialized;
            userPresenceController.OnUserPresence -= OnUserPresence;
        }

        /// <summary>
        /// Update the text on the screen with the list of users when the user presence is done initializing
        /// </summary>
        private void OnUserPresenceControllerInitialized()
        {
            Results.text = "OnUserPresenceControllerInitialized - Users:\n";
            foreach (var user in userPresenceController.Users.Values)
            {
                Results.text += user.display + "\n";
            }
            GameboardLogging.Verbose(Results.text);
        }

        /// <summary>
        /// Handle receiving a user presence event
        /// This is an event from the gameboard when any user connects, disconnects, or changes position
        /// </summary>
        /// <param name="userPresence"></param>
        private void OnUserPresence(GameboardUserPresenceEventArgs userPresence)
        {
            GameboardLogging.Verbose($"user presence display: {userPresence}");
            GameboardLogging.Verbose($"User Count: {userPresenceController.Users.Count()}");
            Results.text = $"user presence changed for {userPresence?.display} -- {userPresence?.change}";
            GameboardLogging.Verbose(Results.text);
        }

        /// <summary>
        /// Fetch the list of companion users on demand
        /// </summary>
        public async void FetchCompanionUsers()
        {
            if (userPresenceController == null)
            {
                Results.text = "userPresenceController was null, the gameboard may have failed to initialize.";
                GameboardLogging.Verbose(Results.text);
                return;
            }

            var companionUserPresence = await userPresenceController.GetCompanionUserPresence();
            if (companionUserPresence == null)
            {
                Results.text = "Failed to FetchCompanionUsers.";
                GameboardLogging.Verbose(Results.text);
                return;
            }

            if (companionUserPresence.errorResponse != null)
            {
                Results.text = $"ERROR: {companionUserPresence.errorId} - {companionUserPresence.errorResponse.Message}";
                GameboardLogging.Verbose(Results.text);
                return;
            }

            var UserPresence = companionUserPresence.playerPresenceList;
            Results.text = "playerPresenceList:\n";
            showUserPresenceInResult(UserPresence);
        }

        public void FetchArmedUsers()
        {
            Results.text = "armedPlayers:\n";
            showUserPresenceInResult(playerArmingController.GetArmedPlayers());
        }

        private void showUserPresenceInResult(List<GameboardUserPresenceEventArgs> userPresence)
        {

            userPresence.ForEach(user => Results.text += $"{user}\n");
            GameboardLogging.Verbose(Results.text);
            GameboardLogging.Verbose("UserPresence: " + userPresence);
        }
    }
}

