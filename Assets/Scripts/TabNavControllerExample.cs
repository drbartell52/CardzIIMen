using Gameboard.EventArgs;
using UnityEngine;
using UnityEngine.UI;
using Gameboard.Objects;
using System.Collections.Generic;
using System;
using Gameboard.Objects.DeviceEvent;

namespace Gameboard.Examples
{
    public class TabNavControllerExample : MonoBehaviour
    {
        private UserPresenceController userPresenceController;
        private AssetController assetController;
        private CompanionTabNavController tabNavController;
        private CardController cardController;
        private Gameboard gameboard;

        private string userId;
        private string UserId
        {
            get
            {
                if (string.IsNullOrEmpty(userId))
                {
                    Results.text = "There are no companion users connected.";
                    GameboardLogging.Warning("Attempted to call a companion method when there were no companion users connected.");
                    return string.Empty;
                }

                return userId;
            }
            set
            {
                userId = value;
            }
        }

        /// <summary>
        /// Assets loaded from the Cards resource folder
        /// </summary>
        private List<CompanionTextureAsset> cardAssets = new List<CompanionTextureAsset>();

        public Text Results;

        private void Start()
        {
            GameObject gameboardObject = GameObject.FindWithTag("Gameboard");
            gameboard = gameboardObject.GetComponent<Gameboard>();

            userPresenceController = gameboardObject.GetComponent<UserPresenceController>();
            assetController = gameboardObject.GetComponent<AssetController>();
            cardController = gameboardObject.GetComponent<CardController>();
            tabNavController = gameboardObject.GetComponent<CompanionTabNavController>();

            GameboardLogging.LogMessage($"TabNavController: {tabNavController}", GameboardLogging.MessageTypes.Warning);

            gameboard.GameboardShutdownBegun += OnGameboardShutdown;
            tabNavController.CompanionTabNavPressed += OnCompanionTabNavPressed;
        }

        private void OnDestroy()
        {
            gameboard.GameboardShutdownBegun -= OnGameboardShutdown;
            tabNavController.CompanionTabNavPressed -= OnCompanionTabNavPressed;
        }

        private void OnGameboardShutdown()
        {
            gameboard.GameboardShutdownBegun -= OnGameboardShutdown;
            tabNavController.CompanionTabNavPressed -= OnCompanionTabNavPressed;
        }

        private void OnCompanionTabNavPressed(GameboardTabNavPressedEventArgs companionTabEvent)
        {
            Results.text = $"OnTabNavPressed {JsonUtility.ToJson(companionTabEvent)}";
        }

        public async void SetupTabs()
        {
            if (userPresenceController.Users.Count == 0)
            {
                Results.text = "No users were found in userPresenceController.Users.";
                GameboardLogging.Error("No users were found in userPresenceController.Users.");
                return;
            }

            // Get the user id of the first companion user from the UserPresenceController
            UserId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (string.IsNullOrEmpty(UserId))
                return;
            Results.text = $"Loading Assets...";

            // Create texture assets to load into the companion app
            var textureDelegate = new AssetController.AddAsset<CompanionTextureAsset, Texture2D>(assetController.AddTextureToAssets);
            cardAssets = assetController.CreateCompanionAssetsFromPath("Cards", textureDelegate);

            await assetController.LoadAllAssetsOntoAllCompanions();

            if (cardAssets.Count <= 0)
            {
                GameboardLogging.Error("Failed to load button assets.");
                Results.text = "Failed to load button assets.";
                return;
            }

            await CreateHandWithCards();
            await CreateHandWithCards();

            Results.text = $"Created 2 hands with cards.";

            return;
        }

        public async void ShowTabs()
        {
            UserId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (string.IsNullOrEmpty(UserId))
                return;

            CompanionMessageResponseArgs response = await tabNavController.ShowTabNavigation(userId, new TabNavigationOptions()
            {
                display = true,
                includeDiceScreen = true,
            });
        }

        public async void HideTabs()
        {
            CompanionMessageResponseArgs response = await tabNavController.ShowTabNavigation(userId, new TabNavigationOptions()
            {
                display = false
            });
        }

        public async void ShowCustomTabs()
        {
            UserId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (string.IsNullOrEmpty(UserId))
                return;

            CompanionTextureAsset cardFront = cardAssets[UnityEngine.Random.Range(0, cardAssets.Count - 1)];

            CompanionMessageResponseArgs response = await tabNavController.ShowTabNavigation(userId, new TabNavigationOptions()
            {
                display = true,
                includeDiceScreen = true,
                font = "Courier New",
                fontColor = "blue",
                backgroundAssetId = cardFront.AssetGuid.ToString(),
            });
        }

        private async System.Threading.Tasks.Task<bool> CreateHandWithCards()
        {
            if (string.IsNullOrEmpty(UserId))
                return false;
            CompanionCreateObjectEventArgs response = await cardController.CreateCompanionHandDisplay(UserId, "HandName");

            if ((bool)!response?.wasSuccessful)
            {
                Results.text += $"\n error: {response?.errorResponse}";
                return false;
            }

            AddCardToHand(response.newObjectUid);
            AddCardToHand(response.newObjectUid);

            return true;
        }

        private async void AddCardToHand(string handId)
        {
            if (string.IsNullOrEmpty(UserId))
                return;

            // Get 2 random card assets from the list
            GameboardLogging.Log($"currently have {cardAssets.Count} card assets");
            CompanionTextureAsset cardFront = cardAssets[UnityEngine.Random.Range(0, cardAssets.Count - 1)];
            CompanionTextureAsset cardBack = cardAssets[UnityEngine.Random.Range(0, cardAssets.Count - 1)];

            // Create the random card
            CompanionCreateObjectEventArgs newCardResponse = await cardController.CreateCompanionCard(UserId, Guid.NewGuid().ToString(), cardFront.AssetGuid.ToString(), cardBack.AssetGuid.ToString());
            if (newCardResponse == null)
            {
                if ((bool)!newCardResponse?.wasSuccessful)
                    Results.text += $"\n error: {newCardResponse?.errorResponse}";

                return;
            }

            CompanionMessageResponseArgs response = await cardController.AddCardToHandDisplay(UserId, handId, newCardResponse.newObjectUid);

            if ((bool)!response?.wasSuccessful)
            {
                Results.text += $"\n error: {response?.errorResponse}";
                return;
            }
        }
    }
}
