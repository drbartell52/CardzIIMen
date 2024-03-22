using Gameboard.EventArgs;
using UnityEngine;
using Gameboard.Objects;
using System.Collections.Generic;
using System;
using System.Linq;
using Gameboard.Objects.DeviceEvent;
using Gameboard.Objects.Buttons;
using System.Threading.Tasks;
using static Gameboard.EventArgs.CompanionCardErrorResponse;

namespace Gameboard.CardsPracticalExample
{
    public class CardPracticalExample : MonoBehaviour
    {
        private CompanionButtonController companionButtonController;
        private UserPresenceController userPresenceController;
        private GameboardAssetLoader assetLoader;
        private CardController cardController;
        private CompanionTabNavController tabNavController;
        private DeviceEventController deviceEventController;
        private Gameboard gameboard;
        private Dictionary<string, PlayerCompanionData> playersData = new Dictionary<string, PlayerCompanionData>();

        private enum ButtonId
        {
            returnFromHidden,
            drawNewCard,
            sendCenterCard,
            exchangeSelected
        }

        private void Start()
        {
            GameObject gameboardObject = GameObject.FindWithTag("Gameboard");
            gameboard = gameboardObject.GetComponent<Gameboard>();

            //Manages assets
            assetLoader = GameObject.FindWithTag("AssetLoader").GetComponent<GameboardAssetLoader>();

            userPresenceController = gameboardObject.GetComponent<UserPresenceController>();
            companionButtonController = gameboardObject.GetComponent<CompanionButtonController>();
            cardController = gameboardObject.GetComponent<CardController>();
            tabNavController = gameboardObject.GetComponent<CompanionTabNavController>();
            deviceEventController = gameboardObject.GetComponent<DeviceEventController>();

            gameboard.GameboardShutdownBegun += OnGameboardShutdown;
            cardController.CardButtonPressed += OnCardButtonPressed;
            cardController.CardPlayed += OnCardPlayed;
            cardController.CardTapped += OnCardTapped;
            userPresenceController.OnUserPresence += OnUserPresence;
            tabNavController.CompanionTabNavPressed += OnCompanionTabNavPressed;

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
            this.EventCleanup();
        }

        private void OnGameboardShutdown()
        {
            this.EventCleanup();
        }

        private void EventCleanup()
        {
            gameboard.GameboardShutdownBegun -= OnGameboardShutdown;
            cardController.CardButtonPressed -= OnCardButtonPressed;
            cardController.CardPlayed -= OnCardPlayed;
            cardController.CardTapped -= OnCardTapped;
            userPresenceController.OnUserPresence -= OnUserPresence;
            tabNavController.CompanionTabNavPressed -= OnCompanionTabNavPressed;
            userPresenceController.UserPresenceControllerInitialized -= OnUserPresenceControllerInitialized;
        }

        /// <summary>
        /// Update tracked players + setup companion if needed when users are added
        /// Remove tracked players when users are removed
        /// </summary>
        private void OnUserPresence(GameboardUserPresenceEventArgs userPresence)
        {
            switch (userPresence.changeValue)
            {
                case DataTypes.UserPresenceChangeTypes.ADD:
                    addPlayerFromPresence(userPresence, true);
                    break;
                case DataTypes.UserPresenceChangeTypes.REMOVE:
                    playersData.Remove(userPresence.userId);
                    break;
            }
        }

        /// <summary>
        /// Different actions based on the type of card button pressed
        /// </summary>
        private async void OnCardButtonPressed(GameboardCompanionCardsButtonPressedEventArgs companionButtonEvent)
        {
            var playerData = playersData[companionButtonEvent.ownerId];
            switch (companionButtonEvent.buttonId)
            {
                case nameof(ButtonId.drawNewCard):
                    addNewCard(playerData);
                    break;
                case nameof(ButtonId.exchangeSelected):
                    exchangeSelected(playerData);
                    break;
                case nameof(ButtonId.returnFromHidden):
                    returnToMainHand(playerData);
                    break;
                case nameof(ButtonId.sendCenterCard):
                    await removeCard(playerData, companionButtonEvent.selectedCardId);
                    break;
            }
        }

        /// <summary>
        /// If it is the main hand or hidden hand, highlight/unhighlight the tapped card and keep track of those cards.
        /// For the hidden hand, only one item can be highlighted, so remove all previous highlights when highlighting a new item.
        /// </summary>
        private async void OnCardTapped(CompanionCardPlayedEventArgs cardTappedEvent)
        {
            var playerData = playersData[cardTappedEvent.ownerId];

            if (playerData.activeHand.type == HandType.Resources) return;

            if (playerData.activeHand.currentlySelectedCards.Contains(cardTappedEvent.cardId))
            {
                playerData.activeHand.currentlySelectedCards.Remove(cardTappedEvent.cardId);
                await cardController.SetCardHighlights(cardTappedEvent.ownerId, new CardHighlights()
                {
                    cardIds = new string[] { cardTappedEvent.cardId },
                    color = $"#{ColorUtility.ToHtmlStringRGB(Color.green)}",
                    enabled = false,
                });
            }
            else
            {
                // Hidden hand can only select one card at a time
                if (playerData.activeHand.type == HandType.Hidden)
                {
                    await cardController.SetCardHighlights(cardTappedEvent.ownerId, new CardHighlights()
                    {
                        all = true,
                        enabled = false,
                    });
                    playerData.activeHand.currentlySelectedCards.Clear();
                }

                playerData.activeHand.currentlySelectedCards.Add(cardTappedEvent.cardId);
                await cardController.SetCardHighlights(cardTappedEvent.ownerId, new CardHighlights()
                {
                    cardIds = new string[] { cardTappedEvent.cardId },
                    color = $"#{ColorUtility.ToHtmlStringRGB(Color.green)}",
                    enabled = true,
                });
            }
        }

        /// <summary>
        /// When on the main hand, remove the card that is being played. Otherwise do nothing.
        /// </summary>
        private async void OnCardPlayed(CompanionCardPlayedEventArgs cardPlayedEvent)
        {
            var playerData = playersData[cardPlayedEvent.ownerId];

            if (playerData.activeHand.type != HandType.Main) return;

            await removeCard(playerData, cardPlayedEvent.cardId);
        }

        /// <summary>
        /// Swap out buttons and show/hide select asset and tab navigation based on which hand you are on.
        /// </summary>
        private void OnCompanionTabNavPressed(GameboardTabNavPressedEventArgs companionTabEvent)
        {
            var playerData = playersData[companionTabEvent.ownerId];
            switchTab(playerData, companionTabEvent.handId);
        }

        /// <summary>
        /// Button click event from hidden hand. Show the tab navigation and return to the main hand.
        /// </summary>
        private async void returnToMainHand(PlayerCompanionData playerData)
        {
            await tabNavController.ShowTabNavigation(playerData.userId, new TabNavigationOptions() { display = true });
            var mainHandId = playerData.hands.Values.First(hand => hand.type == HandType.Main).id;

            await cardController.ShowCompanionHandDisplay(playerData.userId, mainHandId);
            switchTab(playerData, mainHandId);
        }


        /// <summary>
        /// Button click event. Remove all the cards and add new ones for each one successfully removed.
        /// </summary>
        private async void exchangeSelected(PlayerCompanionData playerData)
        {
            var removeResponse = new List<bool>();
            foreach (var cardId in playerData.activeHand.currentlySelectedCards)
            {
                var wasSuccessful = await removeCard(playerData, cardId);
                removeResponse.Add(wasSuccessful);
            }

            foreach (var wasSuccessful in removeResponse)
            {
                if (wasSuccessful)
                {

                    addNewCard(playerData);
                }
            }

            playerData.activeHand.currentlySelectedCards.Clear();
        }

        /// <summary>
        /// Draw a new card and add it to either the active hand or the hand specified.
        /// </summary>
        private async void addNewCard(PlayerCompanionData playerData, string handId = null, string frontAssetId = null, string backAssetId = null)
        {
            var randomNumberGenerator = new System.Random();

            string cardFront = frontAssetId ?? assetLoader.cardAssets[randomNumberGenerator.Next(0, assetLoader.cardAssets.Count - 1)].AssetGuid.ToString();
            string cardBack = backAssetId ?? assetLoader.cardAssets[randomNumberGenerator.Next(0, assetLoader.cardAssets.Count - 1)].AssetGuid.ToString();

            // Create the random card
            var newCardResponse = await cardController.CreateCompanionCard(playerData.userId, Guid.NewGuid().ToString(), cardFront, cardBack);

            //Try again after reloading assets
            if (newCardResponse.errorId == (int)CardErrorTypes.BackTextureIDNotFound || newCardResponse.errorId == (int)CardErrorTypes.FrontTextureIDNotFound)
            {
                await assetLoader.assetController.LoadAllAssetsOntoOneCompanion(playerData.userId);
                newCardResponse = await cardController.CreateCompanionCard(playerData.userId, Guid.NewGuid().ToString(), cardFront, cardBack);
            }

            if (newCardResponse.wasSuccessful)
            {
                var addToHandId = handId != null ? handId : playerData.activeHand.id;
                var addToHandResponse = await cardController.AddCardToHandDisplay(playerData.userId, addToHandId, newCardResponse.newObjectUid);

                if (addToHandResponse.wasSuccessful)
                {
                    playerData.activeHand.cards.Add(addToHandId);
                }
            }
        }

        /// <summary>
        /// Removes card from active hand
        /// </summary>
        private async Task<bool> removeCard(PlayerCompanionData playerData, string cardId)
        {
            var response = await cardController.RemoveCardFromHandDisplay(playerData.userId, playerData.activeHand.id, cardId);
            if (response.wasSuccessful)
            {
                playerData.activeHand.cards.Remove(cardId);
            }
            return response.wasSuccessful;
        }

        /// <summary>
        /// Player action. Takes the player to a hidden hand to select one card. 
        /// </summary>
        public async void StealFromPlayer()
        {
            //In theory would be the player whos turn it is, and then some mechanism for them to select a player to steal from
            var currentPlayer = playersData.Values.First();

            await tabNavController.ShowTabNavigation(currentPlayer.userId, new TabNavigationOptions() { display = false });

            var hiddenHandId = currentPlayer.hands.Values.First(hand => hand.type == HandType.Hidden).id;

            await cardController.ShowCompanionHandDisplay(currentPlayer.userId, hiddenHandId);
            switchTab(currentPlayer, hiddenHandId);
        }

        /// <summary>
        /// Swap out buttons and show/hide select asset and tab navigation based on which hand you are on.
        /// </summary>
        private async void switchTab(PlayerCompanionData playerData, string handId)
        {
            if (playerData.activeHand.buttons != null)
            {
                foreach (var button in playerData.activeHand.buttons)
                {
                    await companionButtonController.SetCompanionButtonVisiblity(playerData.userId, button.buttonId, false);
                }
            }

            playerData.activeHand = playerData.hands[handId];

            switch (playerData.activeHand.type)
            {
                case HandType.Hidden:
                    await cardController.SetCardTemplateType(playerData.userId, CompanionCardTemplateType.Playing);
                    await cardController.SetCardControlAsset(playerData.userId, ControlAssetType.CardSelectedAssetId, null);
                    break;
                case HandType.Main:
                    await cardController.SetCardTemplateType(playerData.userId, CompanionCardTemplateType.Card);
                    await cardController.SetCardControlAsset(playerData.userId, ControlAssetType.CardSelectedAssetId, assetLoader.selectAssetGuid);
                    break;
                case HandType.Resources:
                    await cardController.SetCardTemplateType(playerData.userId, CompanionCardTemplateType.Card);
                    await cardController.SetCardControlAsset(playerData.userId, ControlAssetType.CardSelectedAssetId, null);
                    break;
            }

            if (playerData.activeHand.buttons != null)
            {
                await companionButtonController.SetAndShowMultipleCompanionCardButtons(playerData.userId, playerData.activeHand.buttons.ToArray());
            }
        }

        /// <summary>
        /// Load assets on any existing connected companion devices + keep track of the players
        /// </summary>
        private async void OnUserPresenceControllerInitialized()
        {
            await assetLoader.assetController.LoadAllAssetsOntoAllCompanions();

            // Keep track of user and initialize companion if needed
            foreach (var user in userPresenceController.Users.Values)
            {
                addPlayerFromPresence(user);
            }
        }

        /// <summary>
        /// Keeps track of players from user presences. If they are a companion player, initializes companion assets/setup
        /// </summary>
        private void addPlayerFromPresence(GameboardUserPresenceEventArgs userPresence, bool loadAssets = false)
        {
            var newPlayer = new PlayerCompanionData(userPresence.userId);
            playersData.Add(userPresence.userId, newPlayer);

            if (userPresence.presenceTypeValue == DataTypes.PresenceType.COMPANION)
            {
                newPlayer.companionPlayer = true;
                initializeCompanionForUser(newPlayer, loadAssets);
            }
        }

        /// <summary>
        /// Initializes companion assets/setup. Set selected asset, background, creates initial hands, and adds cards to them.
        /// Sets the main hand as the initial hand to display.
        /// </summary>
        private async void initializeCompanionForUser(PlayerCompanionData player, bool loadAssets = false)
        {
            await deviceEventController.ResetPlayPanel(player.userId);

            await deviceEventController.DisplaySystemPopup(player.userId, "Loading...", 6.0f);

            if (loadAssets) //reload assets if needed
            {
                await assetLoader.assetController.LoadAllAssetsOntoOneCompanion(player.userId); //Load card assets
            }

            //Set background
            await cardController.SetCardControlAsset(player.userId, ControlAssetType.CardBackgroundAssetId, assetLoader.backgroundGuid);

            //Set select asset
            await cardController.SetCardControlAsset(player.userId, ControlAssetType.CardSelectedAssetId, assetLoader.selectAssetGuid);

            //Create Hands
            var hiddenHandResponse = await cardController.CreateCompanionHandDisplay(player.userId, "Hidden", true, false);
            var mainHandResponse = await cardController.CreateCompanionHandDisplay(player.userId, "Main");
            var resourceHandResponse = await cardController.CreateCompanionHandDisplay(player.userId, "Resources", false, false);

            var hiddenHand = new Hand(hiddenHandResponse.newObjectUid)
            {
                type = HandType.Hidden
            };
            hiddenHand.buttons = new List<CompanionCardButton>(){
                new CompanionCardButton(nameof(ButtonId.returnFromHidden), "Go Back", DataTypes.CardButtonPosition.Center)
            };
            player.hands.Add(hiddenHandResponse.newObjectUid, hiddenHand);

            var mainHand = new Hand(mainHandResponse.newObjectUid)
            {
                type = HandType.Main
            };
            mainHand.buttons = new List<CompanionCardButton>(){
                new CompanionCardButton(nameof(ButtonId.drawNewCard), "Draw", DataTypes.CardButtonPosition.Left),
                new CompanionCardButton(nameof(ButtonId.sendCenterCard), "Send", DataTypes.CardButtonPosition.Center),
                new CompanionCardButton(nameof(ButtonId.exchangeSelected), "Exchange", DataTypes.CardButtonPosition.Right),
            };
            player.hands.Add(mainHandResponse.newObjectUid, mainHand);

            var resourceHand = new Hand(resourceHandResponse.newObjectUid)
            {
                type = HandType.Resources
            };
            player.hands.Add(resourceHandResponse.newObjectUid, resourceHand);

            //Add cards to hands 
            foreach (var resourceCard in assetLoader.resourceCardAssets)
            {
                addNewCard(player, resourceHand.id, resourceCard.AssetGuid.ToString(), resourceCard.AssetGuid.ToString());
            }

            for (var i = 0; i < 3; i++)
            {
                addNewCard(player, mainHand.id);
                addNewCard(player, hiddenHand.id);
            }

            //Set tab visibility
            await tabNavController.ShowTabNavigation(player.userId, new TabNavigationOptions()
            {
                display = true,
            });

            //Set buttons
            await companionButtonController.SetAndShowMultipleCompanionCardButtons(player.userId, mainHand.buttons.ToArray());

            //Set and show active hand
            player.activeHand = mainHand;
            await cardController.ShowCompanionHandDisplay(player.userId, mainHand.id);
        }
    }
}
