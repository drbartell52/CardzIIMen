using Gameboard.EventArgs;
using UnityEngine;
using UnityEngine.UI;
using Gameboard.Objects;
using System.Collections.Generic;
using System;
using System.Linq;

// TODO: load player cards if they exist when we receive a companion event for a user joining
namespace Gameboard.Examples
{
    public class CardControllerExample : MonoBehaviour
    {
        private CompanionButtonController companionButtonController;
        private UserPresenceController userPresenceController;
        private AssetController assetController;
        private CardController cardController;
        private Gameboard gameboard;
        private int? activeHandIndex;

        // TODO we should ideally consolidate this into objects in the controller
        private string userId;
        private string UserId
        {
            get
            {
                if (string.IsNullOrEmpty(userId))
                {
                    var id = Utils.GetFirstCompanionUserId(userPresenceController);
                    if (id == null)
                    {
                        Results.text = "There are no companion users connected.";
                        GameboardLogging.Warning("Attempted to call a companion method when there were no companion users connected.");
                        return string.Empty;
                    }
                    else
                    {
                        userId = id;
                        return id;
                    }
                }

                return userId;
            }
            set
            {
                userId = value;
            }
        }

        /// <summary>
        /// list of hands for a user with userId as key
        /// </summary>
        /// <remarks>while it is possible to have multiple hands, we only use the first one to keep the example simpler</remarks>
        private Dictionary<string, List<string>> userHands = new Dictionary<string, List<string>>();

        /// <summary>
        /// list of cards in a hand with handId as key
        /// </summary>
        private Dictionary<string, List<Guid>> handCards = new Dictionary<string, List<Guid>>();

        /// <summary>
        /// card assets loaded from the Cards resource folder
        /// </summary>
        private List<CompanionTextureAsset> cardAssets = new List<CompanionTextureAsset>();

        /// <summary>
        /// list of card ids we created when randomly generating a card to add to a user's hand
        /// </summary>
        private List<string> createdCards = new List<string>();

        public Text Results;

        private void Start()
        {
            GameObject gameboardObject = GameObject.FindWithTag("Gameboard");
            gameboard = gameboardObject.GetComponent<Gameboard>();

            userPresenceController = gameboardObject.GetComponent<UserPresenceController>();
            companionButtonController = gameboardObject.GetComponent<CompanionButtonController>();
            assetController = gameboardObject.GetComponent<AssetController>();
            cardController = gameboardObject.GetComponent<CardController>();

            gameboard.GameboardShutdownBegun += OnGameboardShutdown;
            cardController.CardButtonPressed += OnCardButtonPressed;
            cardController.CardPlayed += OnCardEvent;
            cardController.CardTapped += OnCardEvent;
            cardController.CardsReordered += OnCardsReorderedEvent;
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
            cardController.CardPlayed -= OnCardEvent;
            cardController.CardTapped -= OnCardEvent;
            cardController.CardsReordered -= OnCardsReorderedEvent;
        }

        private void OnCardButtonPressed(GameboardCompanionCardsButtonPressedEventArgs companionButtonEvent)
        {
            Results.text = $"OnCardButtonPressed {companionButtonEvent}";
        }

        private void OnCardEvent(CompanionCardPlayedEventArgs cardPlayedEvent)
        {
            Results.text = $"OnCard {cardPlayedEvent.eventType} {cardPlayedEvent}";
        }

        private void OnCardsReorderedEvent(GameboardCardsReorderedEventArgs cardsReorderedEvent)
        {
            var userId = cardsReorderedEvent.ownerId;
            Results.text = $"OnCardsReordered {cardsReorderedEvent} Order: {cardController.playersCompanionData[userId].activeHand.cards}";
        }

        public async void LoadAssets()
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

            Results.text = $"Loaded {cardAssets.Count} card Assets.";
            return;
        }

        public async void SetRandomCardControlAssets()
        {
            // Get the user id of the first companion user from the UserPresenceController
            UserId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (string.IsNullOrEmpty(UserId))
                return;

            // Iterate through all of the valid ControlAssetTypes and set a random asset
            foreach (var assetType in Enum.GetNames(typeof(ControlAssetType)))
            {
                Enum.TryParse(assetType, out ControlAssetType controlAssetType);
                CompanionTextureAsset randomAsset = cardAssets[UnityEngine.Random.Range(0, cardAssets.Count - 1)];
                await cardController.SetCardControlAsset(UserId, controlAssetType, randomAsset.AssetGuid.ToString());
            }
        }

        public async void SetCardTemplate()
        {
            if (string.IsNullOrEmpty(UserId))
                return;
            await cardController.SetCardTemplateType(UserId, CompanionCardTemplateType.Card);
        }

        public async void SetTileTemplate()
        {
            if (string.IsNullOrEmpty(UserId))
                return;
            await cardController.SetCardTemplateType(UserId, CompanionCardTemplateType.Tile);
        }


        public async void SetPlayingTemplate()
        {
            if (string.IsNullOrEmpty(UserId))
                return;
            await cardController.SetCardTemplateType(UserId, CompanionCardTemplateType.Playing);
        }

        public async void CreateHand()
        {
            if (string.IsNullOrEmpty(UserId))
                return;
            CompanionCreateObjectEventArgs response = await cardController.CreateCompanionHandDisplay(UserId, "HandName");
            Results.text = $"response for cardController.CreateCompanionHandDisplay: {response}";
            GameboardLogging.Verbose($"response for cardController.CreateCompanionHandDisplay: {response}");

            if ((bool)!response?.wasSuccessful)
            {
                Results.text += $"\n error: {response?.errorResponse}";
                return;
            }

            if (userHands.TryGetValue(UserId, out var hands))
            {
                hands.Add(response.newObjectUid);
                activeHandIndex = hands.Count - 1;
                Results.text += $"\nnow have a total of {hands.Count} hands";
            }
            else //First hand for this user
            {
                userHands.Add(UserId, new List<string>() { response.newObjectUid });
                Results.text += $"\nnow have a total of 1 hand";
            }

            var handIds = cardController.playersCompanionData[UserId].hands.Values.Select(hand => hand.id).ToList();
            Results.text += $"\nHands: {String.Join(",", handIds)}";

            return;
        }

        public async void ShowHand()
        {
            if (string.IsNullOrEmpty(UserId))
                return;
            // Get a hand for the current user
            if (userHands.TryGetValue(UserId, out var hands))
            {
                activeHandIndex = activeHandIndex == null ? 0 : ((activeHandIndex + 1) % hands.Count);
                var index = activeHandIndex.GetValueOrDefault(0);
                GameboardLogging.Verbose($"Attempting to show hand number {index} with ID {hands[index]} from user {UserId}");

                // show the hand
                CompanionMessageResponseArgs response = await cardController.ShowCompanionHandDisplay(UserId, hands[index]);

                Results.text = $"(Hand index {index}) Response for cardController.ShowCompanionHandDisplay: {response}";
                Results.text += $"\nActiveHand: {JsonUtility.ToJson(cardController.playersCompanionData[UserId].activeHand)}";

                if ((bool)!response?.wasSuccessful)
                    Results.text += $"\n error: {response?.errorResponse}";

                GameboardLogging.Verbose($"response for cardController.ShowCompanionHandDisplay: {response}");

                return;
            }

            Results.text = $"failed to get user hand prior to calling cardController.ShowCompanionHandDisplay";
            GameboardLogging.Verbose($"failed to get user hand prior to calling cardController.ShowCompanionHandDisplay");
        }

        public async void AddTextToCard()
        {
            if (string.IsNullOrEmpty(UserId))
                return;

            if (userHands.TryGetValue(UserId, out var hands))
            {
                if (handCards.TryGetValue(hands[activeHandIndex.GetValueOrDefault(0)], out var cards))
                {
                    GameboardLogging.Verbose($"hand {hands[activeHandIndex.GetValueOrDefault(0)]} has {cards.Count} cards");

                    if (cards.Count <= 0)
                    {
                        Results.text = $"There are no cards to set text of!";
                        return;
                    }

                    // Try to set the first card's text
                    CompanionMessageResponseArgs response = await cardController.SetCardText(UserId, new CardTextProp
                    {
                        cardId = cards[activeHandIndex.GetValueOrDefault(0)].ToString(),
                        text = "Updating the card after the fact",
                        color = "red",
                        fontSize = 24,
                        horizontalPadding = 30,
                        verticalAdjust = 50
                    });

                    Results.text = $"response for cardController.SetCardHighlights: {response}";
                    GameboardLogging.Verbose($"response for cardController.SetCardHighlights: {response}");
                    if ((bool)!response?.wasSuccessful)
                        Results.text += $"\n error: {response?.errorResponse}";

                    return;
                }
                else
                {
                    Results.text = $"failed to get card prior to calling cardController.SetCardHighlights";
                    GameboardLogging.Verbose($"failed to get card prior to calling cardController.SetCardHighlights");
                    return;
                }
            }
        }

        public async void AddCardToHand()
        {
            if (string.IsNullOrEmpty(UserId))
                return;
            // Get 2 random card assets from the list
            GameboardLogging.Log($"currently have {cardAssets.Count} card assets");
            CompanionTextureAsset cardFront = cardAssets[UnityEngine.Random.Range(0, cardAssets.Count - 1)];
            CompanionTextureAsset cardBack = cardAssets[UnityEngine.Random.Range(0, cardAssets.Count - 1)];

            // Create the random card
            CompanionCreateObjectEventArgs newCardResponse = await cardController.CreateCompanionCard(
                UserId,
                Guid.NewGuid().ToString(),
                cardFront.AssetGuid.ToString(),
                cardBack.AssetGuid.ToString(),
                new CardTextProp
                {
                    text = "This is a test",
                    horizontalPadding = 45,
                    textAlignment = TextAlignment.Right
                });
            if (newCardResponse == null)
            {
                Results.text = $"cardController.AddCardToHandDisplay failed to create a card: {newCardResponse}";

                if ((bool)!newCardResponse?.wasSuccessful)
                    Results.text += $"\n error: {newCardResponse?.errorResponse}";

                GameboardLogging.Verbose($"cardController.AddCardToHandDisplay failed to create a card: {newCardResponse}");
                return;
            }

            if (userHands.TryGetValue(UserId, out var hands))
            {
                // Add the card to the list of card ids and add the card to the user's hand
                createdCards.Add(newCardResponse.newObjectUid);
                CompanionMessageResponseArgs response = await cardController.AddCardToHandDisplay(UserId, hands[activeHandIndex.GetValueOrDefault(0)], newCardResponse.newObjectUid);
                Results.text = $"response for cardController.AddCardToHandDisplay: {response}";
                GameboardLogging.Verbose($"response for cardController.AddCardToHandDisplay: {response}");

                if ((bool)!response?.wasSuccessful)
                {
                    Results.text += $"\n error: {response?.errorResponse}";
                    return;
                }

                // Keep track of the card that was added to the companion hand.
                if (handCards.TryGetValue(hands[activeHandIndex.GetValueOrDefault(0)], out var cards))
                    cards.Add(new Guid(newCardResponse.newObjectUid));
                else
                    handCards.Add(hands[activeHandIndex.GetValueOrDefault(0)], new List<Guid>() { new Guid(newCardResponse.newObjectUid) });

                GameboardLogging.Verbose($"added card {newCardResponse.newObjectUid} to hand {hands[activeHandIndex.GetValueOrDefault(0)]}");

                var cardIds = cardController.playersCompanionData[UserId].activeHand.cards;
                Results.text += $"\nCards: {String.Join(",", cardIds)}";

                return;
            }

            Results.text = $"failed to get hand prior to calling cardController.AddCardToHandDisplay";
            GameboardLogging.Verbose($"failed to get hand prior to calling cardController.AddCardToHandDisplay");
        }

        public async void RemoveCardFromHand()
        {
            if (string.IsNullOrEmpty(UserId))
                return;
            if (userHands.TryGetValue(UserId, out var hands))
            {
                GameboardLogging.Verbose($"getting card from hand {hands[0]}");

                // Get a card from the first hand
                if (handCards.TryGetValue(hands[0], out var cards))
                {
                    GameboardLogging.Verbose($"hand {hands[0]} has {cards.Count} cards");

                    if (cards.Count <= 0)
                    {
                        Results.text = $"There are no cards to remove!";
                        return;
                    }

                    // Try to remove the first card from the first hand
                    CompanionMessageResponseArgs response = await cardController.RemoveCardFromHandDisplay(UserId, hands[0], cards[0].ToString());
                    Results.text = $"response for cardController.ShowCompanionHandDisplay: {response}";
                    GameboardLogging.Verbose($"response for cardController.ShowCompanionHandDisplay: {response}");

                    if ((bool)!response?.wasSuccessful)
                    {
                        Results.text += $"\n error: {response?.errorResponse}";
                        return;
                    }

                    // Remove the card from the hand's list of cards
                    GameboardLogging.Verbose($"removing card {cards[0]} from hand {hands[0]}");
                    handCards[hands[0]].Remove(cards[0]);

                    var cardIds = cardController.playersCompanionData[UserId].activeHand.cards;
                    Results.text += $"\nCards: {String.Join(",", cardIds)}";

                    return;
                }
                else
                {
                    Results.text = $"failed to get card prior to calling cardController.RemoveCardFromHandDisplay";
                    GameboardLogging.Verbose($"failed to get card prior to calling cardController.RemoveCardFromHandDisplay");

                    return;
                }
            }

            Results.text = $"failed to get hand prior to calling cardController.RemoveCardFromHandDisplay";
            GameboardLogging.Verbose($"failed to get hand prior to calling cardController.RemoveCardFromHandDisplay");
        }

        public async void RemoveAllCardsFromHand()
        {
            if (string.IsNullOrEmpty(UserId))
                return;
            if (userHands.TryGetValue(UserId, out var hands))
            {
                GameboardLogging.Verbose($"removing all cards from hand {hands[0]}, currently has {handCards[hands[0]].Count} cards");

                if (handCards[hands[0]].Count <= 0)
                {
                    Results.text = $"There are no cards to remove!";
                    return;
                }

                CompanionMessageResponseArgs response = await cardController.RemoveAllCardsFromHandDisplay(UserId, hands[0]);

                Results.text = $"response for companionButtonController.SetCompanionButtonValues: {response}";
                GameboardLogging.Verbose($"response for companionButtonController.SetCompanionButtonValues: {response}");

                if ((bool)!response?.wasSuccessful)
                {
                    Results.text += $"\n error: {response?.errorResponse}";
                    return;
                }

                handCards[hands[0]] = new List<Guid>();
                GameboardLogging.Verbose($"hand {hands[0]} now has {handCards[hands[0]].Count} cards");

                var cardIds = cardController.playersCompanionData[UserId].activeHand.cards;
                Results.text += $"\nCards: {String.Join(",", cardIds)}";

                return;
            }

            Results.text = $"failed to get hand prior to calling cardController.RemoveAllCardsFromHandDisplay";
            GameboardLogging.Verbose($"failed to get hand prior to calling cardController.RemoveAllCardsFromHandDisplay");
        }

        public async void SetCardButton()
        {
            if (string.IsNullOrEmpty(UserId))
                return;
            CompanionMessageResponseArgs response;

            if (userHands.TryGetValue(UserId, out var hands) && handCards[hands[0]].Count > 0)
            {
                response = await companionButtonController.SetCompanionCardButton(UserId, "1", "Hello World.", "HelloWorldCallback", DataTypes.CardButtonPosition.Center);
                GameboardLogging.Verbose($"response for companionButtonController.SetCompanionButtonValues: {response}");
                if ((bool)!response?.wasSuccessful)
                {
                    Results.text += $"\n error: {response?.errorResponse}";
                    return;
                }

                response = await companionButtonController.SetCompanionButtonVisiblity(UserId, "1", true);
                GameboardLogging.Verbose($"response for companionButtonController.SetCompanionButtonVisiblity: {response}");
                if ((bool)!response?.wasSuccessful)
                {
                    Results.text += $"\n error: {response?.errorResponse}";
                    return;
                }

                Results.text = $"response for SetCardButton: {response}";
                GameboardLogging.Verbose($"response for SetCardButton: {response}");
                return;
            }

            Results.text = "No cards were found, cannot create buttons ";
        }

        public void ReverseOrderLastHand()
        {
            if (userHands.TryGetValue(UserId, out var hands))
            {
                var handId = hands.Last();
                this.ReverseOrderHandHelper(handId);
            }
        }

        public void ReverseOrderHand()
        {
            ReverseOrderHandHelper(null);
        }

        public async void ReverseOrderHandHelper(string handId = null)
        {
            if (string.IsNullOrEmpty(UserId))
                return;
            if (userHands.TryGetValue(UserId, out var hands))
            {
                var handIndex = handId != null ? hands.IndexOf(handId) : activeHandIndex.GetValueOrDefault(0);
                GameboardLogging.Verbose($"getting card from hand {hands[handIndex]}");

                // Get a cards from the first hand
                if (handCards.TryGetValue(hands[handIndex], out var cards))
                {
                    GameboardLogging.Verbose($"hand {hands[handIndex]} has {cards.Count} cards");

                    if (cards.Count <= 0)
                    {
                        Results.text = $"There are no cards to reorder!";
                        return;
                    }

                    cards.Reverse();

                    // Set new order on companion
                    CompanionMessageResponseArgs response = await cardController.ReorderHand(UserId, cards.Select(c => c.ToString()).ToArray(), handId);
                    Results.text = $"response for cardController.ReorderHand: {response}";

                    var cardIds = cardController.playersCompanionData[UserId].activeHand.cards;
                    Results.text += $"\nCardOrder: {String.Join(",", cardIds)}";

                    GameboardLogging.Verbose($"response for cardController.ReorderHand: {response}");

                    if ((bool)!response?.wasSuccessful)
                    {
                        Results.text += $"\n error: {response?.errorResponse}";

                        return;
                    }

                    //Update card order if successfull
                    handCards[hands[handIndex]] = cards;

                    return;
                }
                else
                {
                    Results.text = $"failed to get hand prior to calling cardController.ReorderHand";
                    GameboardLogging.Verbose($"failed to get hand prior to calling cardController.ReorderHand");

                    return;
                }
            }

            Results.text = $"failed to get hand prior to calling cardController.ReorderHand";
            GameboardLogging.Verbose($"failed to get hand prior to calling cardController.ReorderHand");
        }

        public async void SendCardToEndOfHand()
        {
            if (string.IsNullOrEmpty(UserId))
                return;
            if (userHands.TryGetValue(UserId, out var hands))
            {
                var handIndex = activeHandIndex.GetValueOrDefault(0);
                GameboardLogging.Verbose($"getting card from hand {hands[handIndex]}");

                // Get a cards from the first hand
                if (handCards.TryGetValue(hands[handIndex], out var cards))
                {
                    GameboardLogging.Verbose($"hand {hands[handIndex]} has {cards.Count} cards");

                    if (cards.Count <= 0)
                    {
                        Results.text = $"There are no cards to reorder!";
                        return;
                    }

                    // Set new order on companion
                    CompanionMessageResponseArgs response = await cardController.ReorderCard(UserId, cards[0].ToString(), cards.Count - 1);
                    Results.text = $"response for cardController.ReorderCard: {response}";

                    var cardIds = cardController.playersCompanionData[UserId].activeHand.cards;
                    Results.text += $"\nCardOrder: {String.Join(",", cardIds)}"; GameboardLogging.Verbose($"response for cardController.ReorderCard: {response}");

                    if ((bool)!response?.wasSuccessful)
                    {
                        Results.text += $"\n error: {response?.errorResponse}";

                        return;
                    }

                    //Update card order if successfull
                    Guid movedCard = cards[0];
                    handCards[hands[handIndex]].Remove(movedCard);
                    cards.Add(movedCard);

                    return;
                }
                else
                {
                    Results.text = $"failed to get hand prior to calling cardController.ReorderCard";
                    GameboardLogging.Verbose($"failed to get hand prior to calling cardController.ReorderCard");

                    return;
                }
            }

            Results.text = $"failed to get hand prior to calling cardController.ReorderCard";
            GameboardLogging.Verbose($"failed to get hand prior to calling cardController.ReorderCard");
        }

        public async void SetCardButtonWithAsset()
        {
            if (string.IsNullOrEmpty(UserId))
                return;

            if (cardAssets.Count <= 0)
            {
                Results.text += $"There are no assets loaded!";
                return;
            }

            CompanionMessageResponseArgs response;

            if (userHands.TryGetValue(UserId, out var hands) && handCards[hands[0]].Count > 0)
            {
                response = await companionButtonController.SetCompanionCardButton(UserId, "1", "CardButtonWithAsset", "CardButtonWithAssetCallback", DataTypes.CardButtonPosition.Center, cardAssets[0].AssetGuid.ToString());
                GameboardLogging.Verbose($"response for companionButtonController.SetCompanionButtonValues: {response}");
                if ((bool)!response?.wasSuccessful)
                {
                    Results.text += $"\n error: {response?.errorResponse}";
                    return;
                }

                response = await companionButtonController.SetCompanionButtonVisiblity(UserId, "1", true);
                GameboardLogging.Verbose($"response for companionButtonController.SetCompanionButtonVisiblity: {response}");
                if ((bool)!response?.wasSuccessful)
                {
                    Results.text += $"\n error: {response?.errorResponse}";
                    return;
                }

                Results.text = $"response for SetCardButton: {response}";
                GameboardLogging.Verbose($"response for SetCardButton: {response}");
                return;
            }

            Results.text = "No cards were found, cannot create buttons ";
        }

        public async void SetManyCardButtons()
        {
            if (string.IsNullOrEmpty(UserId))
                return;
            CompanionMessageResponseArgs response;
            Results.text = $"";
            int numButtonsCreated = 0;
            foreach (string buttonId in new List<string> { "testing0", "testing1", "testing2", "3", "1", "2", "testing3", "testing4", "testing5" })
            {
                response = await companionButtonController.SetCompanionCardButton(UserId, buttonId, $"id={buttonId}", $"Callback{buttonId}", DataTypes.CardButtonPosition.Center);
                GameboardLogging.Verbose($"response for buttonId {buttonId} companionButtonController.SetCompanionButtonValues: {response}");

                Results.text += response.wasSuccessful
                    ? $"\nresponse for buttonId {buttonId} companionButtonController.SetCompanionButtonValues: {response}"
                    : $"\nresponse was not successful for buttonId {buttonId} companionButtonController.SetCompanionButtonValues: {response} error: {response.errorResponse}";

                response = await companionButtonController.SetCompanionButtonVisiblity(UserId, buttonId, true);
                if ((bool)!response?.wasSuccessful)
                {
                    Results.text += $"\n error: {response?.errorResponse}";
                    return;
                }

                Results.text += $"\nresponse for buttonId {buttonId} companionButtonController.SetCompanionButtonVisiblity: {response}";
                numButtonsCreated++;
            }

            Results.text += $"\nCreated {numButtonsCreated} buttons.";
        }

        public async void HighlightFirstCard()
        {
            if (string.IsNullOrEmpty(UserId))
                return;
            if (userHands.TryGetValue(UserId, out var hands))
            {
                GameboardLogging.Verbose($"getting card from hand {hands[activeHandIndex.GetValueOrDefault(0)]}");

                // Get a card from the first hand
                if (handCards.TryGetValue(hands[activeHandIndex.GetValueOrDefault(0)], out var cards))
                {
                    GameboardLogging.Verbose($"hand {hands[activeHandIndex.GetValueOrDefault(0)]} has {cards.Count} cards");

                    if (cards.Count <= 0)
                    {
                        Results.text = $"There are no cards to highlight!";
                        return;
                    }

                    var cardHighlights = new CardHighlights()
                    {
                        cardIds = new string[] { cards[activeHandIndex.GetValueOrDefault(0)].ToString() },
                        all = false,
                        color = $"#{ColorUtility.ToHtmlStringRGB(Color.green)}",
                        enabled = true,
                    };

                    // Try to set the first card's highlight
                    CompanionMessageResponseArgs response = await cardController.SetCardHighlights(UserId, cardHighlights);
                    Results.text = $"response for cardController.SetCardHighlights: {response}";
                    GameboardLogging.Verbose($"response for cardController.SetCardHighlights: {response}");
                    if ((bool)!response?.wasSuccessful)
                        Results.text += $"\n error: {response?.errorResponse}";

                    return;
                }
                else
                {
                    Results.text = $"failed to get card prior to calling cardController.SetCardHighlights";
                    GameboardLogging.Verbose($"failed to get card prior to calling cardController.SetCardHighlights");
                    return;
                }
            }

            Results.text = $"failed to get hand prior to calling cardController.RemoveCardFromHandDisplay";
            GameboardLogging.Verbose($"failed to get hand prior to calling cardController.RemoveCardFromHandDisplay");
        }

        public async void SetSameCardAssets()
        {
            if (string.IsNullOrEmpty(UserId))
                return;
            if (userHands.TryGetValue(UserId, out var hands))
            {
                GameboardLogging.Verbose($"getting card from hand {hands[0]}");

                // Get a card from the first hand
                if (handCards.TryGetValue(hands[0], out var cards))
                {
                    GameboardLogging.Verbose($"hand {hands[0]} has {cards.Count} cards");

                    if (cards.Count <= 1)
                    {
                        Results.text = $"There are not enough cards to set assets for! Add more cards!";
                        return;
                    }

                    // Get the asset for the first card in the hand
                    CompanionTextureAsset asset = cardAssets.First();
                    if (asset == null)
                    {
                        Results.text = $"Failed to find an asset.";
                        return;
                    }

                    Results.text = "";
                    foreach (var curCardId in cards)
                    {
                        var assetIdToSet = new CardAssetId()
                        {
                            assetId = asset.AssetGuid.ToString(),
                            cardId = curCardId.ToString(),
                        };

                        // Try to set the front asset
                        CompanionMessageResponseArgs frontResponse = await cardController.SetCardFrontAssetId(UserId, assetIdToSet);
                        Results.text += $"response for cardController.SetCardFrontAssetId: {frontResponse}\n";
                        GameboardLogging.Verbose($"response for cardController.SetCardHighlights: {frontResponse}");
                        if ((bool)!frontResponse?.wasSuccessful)
                            Results.text += $"\n error: {frontResponse?.errorResponse}";

                        // Try to set the back asset
                        CompanionMessageResponseArgs backResponse = await cardController.SetCardBackAssetId(UserId, assetIdToSet);
                        Results.text += $"response for cardController.SetCardBackAssetId: {backResponse}\n";
                        Results.text += $"\nCard data: {JsonUtility.ToJson(cardController.playersCompanionData[UserId].createdCards[curCardId.ToString()])}";
                        GameboardLogging.Verbose($"response for cardController.SetCardBackAssetId: {backResponse}");
                        if ((bool)!backResponse?.wasSuccessful)
                            Results.text += $"\n error: {backResponse?.errorResponse}";
                    }
                    return;
                }
                else
                {
                    Results.text = $"failed to get card prior to calling cardController.SetCardHighlights";
                    GameboardLogging.Verbose($"failed to get card prior to calling cardController.SetCardHighlights");
                    return;
                }
            }

            Results.text = $"failed to get hand prior to calling cardController.RemoveCardFromHandDisplay";
            GameboardLogging.Verbose($"failed to get hand prior to calling cardController.RemoveCardFromHandDisplay");
        }

        // TODO: SetFacingDirectionOfCard - not implented in companion API yet
    }


}
