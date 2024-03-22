using Gameboard.EventArgs;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Gameboard.Objects;
using Gameboard.Objects.OpenAI;
using Gameboard.Objects.StabilityAI;
using System.Linq;
using Gameboard.CardsPracticalExample;
using System;
using Gameboard.Objects.Buttons;
using static Gameboard.EventArgs.CompanionCardErrorResponse;

namespace Gameboard.Examples
{
    /// <summary>
    /// The AI Generation Controller allows connecting to the OpenAI chat capabilities.
    /// The AI Image Controller allows connecting to the stabilityAI image generation capabilities.
    /// </summary>
    public class AIControllerExample : MonoBehaviour
    {
        private AssetController assetController;
        private OpenAIController openAIController;
        private StabilityAIController stabilityAIController;
        private CompanionButtonController companionButtonController;
        private UserPresenceController userPresenceController;
        private CardController cardController;

        public Text results;
        public Image displayImage;

        private DeviceEventController deviceEventController;

        [Tooltip("Image sent to StabilityAI must have dimensions that are an increment of 64.")]
        public Texture2D sendImage;

        private const string BLANK_CARD_ID = "blank_card_AI";
        private const string IMAGE_CARD_ID = "image_card_AI";
        private const string GENERATE_ID = "generate_AI";

        private void Start()
        {
            GameObject gameboardObject = GameObject.FindWithTag("Gameboard");
            Gameboard gameboard = gameboardObject.GetComponent<Gameboard>();

            userPresenceController = gameboardObject.GetComponent<UserPresenceController>();
            companionButtonController = gameboardObject.GetComponent<CompanionButtonController>();
            cardController = gameboardObject.GetComponent<CardController>();
            assetController = gameboardObject.GetComponent<AssetController>();
            deviceEventController = gameboardObject.GetComponent<DeviceEventController>();

            gameboard.GameboardShutdownBegun += OnGameboardShutdown;
            cardController.CardButtonPressed += OnCardButtonPressed;
            userPresenceController.OnUserPresence += OnUserPresence;

            // AI Chat setup
            openAIController = gameboardObject.GetComponent<OpenAIController>();

            openAIController.SetOpenAICredentials("{your-open-ai-api-key}", "{your-org-id}");
            openAIController.OnChatDetailedResponseReceived += ReceivedChat;
            openAIController.OnImageDetailedResponseReceived += ReceivedImages;

            // AI Image setup
            stabilityAIController = gameboardObject.GetComponent<StabilityAIController>();

            stabilityAIController.SetStabilityAICredentials("{your-stability-ai-api-key}");

            stabilityAIController.OnImageDetailedResponseReceived += ReceivedImages;

            if (userPresenceController.IsInitialized)
            {
                OnUserPresenceControllerInitialized();
            }
            else
            {
                userPresenceController.UserPresenceControllerInitialized += OnUserPresenceControllerInitialized;
            }
        }

        private void OnCardButtonPressed(GameboardCompanionCardsButtonPressedEventArgs cardButtonEvent)
        {
            if (cardButtonEvent.buttonId == "GenerateBtn")
            {
                stabilityAIController.SendTextToImageRequest("forest resource card illustration", 896, 768, null, GENERATE_ID);
                openAIController.SendOpenAIChatConversation(new List<OpenAIMessage>() {
                    new OpenAIMessage() {role = MessageRoleEnum.SYSTEM, content = "You are a helpful assistant designed to come up with clever card prompts"},
                    new OpenAIMessage() {role = MessageRoleEnum.USER, content = "Generate a card prompt for a cards against humanity card without the prefix 'prompt'"}
                }, GENERATE_ID);
            }
        }

        private void OnDestroy()
        {
            OnGameboardShutdown();
        }

        private void OnGameboardShutdown()
        {
            openAIController.OnChatDetailedResponseReceived -= ReceivedChat;
            openAIController.OnImageDetailedResponseReceived -= ReceivedImages;
            stabilityAIController.OnImageDetailedResponseReceived -= ReceivedImages;
            cardController.CardButtonPressed -= OnCardButtonPressed;
            userPresenceController.OnUserPresence -= OnUserPresence;
            userPresenceController.UserPresenceControllerInitialized -= OnUserPresenceControllerInitialized;
        }

        private void OnUserPresence(GameboardUserPresenceEventArgs userPresence)
        {
            switch (userPresence.changeValue)
            {
                case DataTypes.UserPresenceChangeTypes.ADD:
                    addPlayerFromPresence(userPresence, true);
                    break;
            }
        }

        private async void OnUserPresenceControllerInitialized()
        {
            await assetController.LoadAllAssetsOntoAllCompanions();

            foreach (var user in userPresenceController.Users.Values)
            {
                addPlayerFromPresence(user);
            }
        }

        private void addPlayerFromPresence(GameboardUserPresenceEventArgs userPresence, bool loadAssets = false)
        {
            if (userPresence.presenceTypeValue == DataTypes.PresenceType.COMPANION)
            {
                initializeCompanionForUser(userPresence, loadAssets);
            }
        }

        private async void initializeCompanionForUser(GameboardUserPresenceEventArgs userPresence, bool loadAssets = false)
        {
            await deviceEventController.ResetPlayPanel(userPresence.userId);

            await deviceEventController.DisplaySystemPopup(userPresence.userId, "Loading...", 1.0f);

            if (loadAssets) //reload assets if needed
            {
                await assetController.LoadAllAssetsOntoOneCompanion(userPresence.userId); //Load card assets
            }

            //Create Hands
            var mainHandResponse = await cardController.CreateCompanionHandDisplay(userPresence.userId, "Main");
            await cardController.ShowCompanionHandDisplay(userPresence.userId, mainHandResponse.newObjectUid);

            //Add Button
            var buttons = new List<CompanionCardButton>(){
                new CompanionCardButton("GenerateBtn", "Generate", DataTypes.CardButtonPosition.Center)};
            await companionButtonController.SetAndShowMultipleCompanionCardButtons(userPresence.userId, buttons.ToArray());

            //Add cards
            var blankCardAsset = assetController.CompanionAssetsByName["blank_card"];
            var newBlankCardResponse = await cardController.CreateCompanionCard(userPresence.userId, BLANK_CARD_ID, blankCardAsset.AssetGuid.ToString(), blankCardAsset.AssetGuid.ToString());
            if (newBlankCardResponse.wasSuccessful)
            {
                await cardController.AddCardToHandDisplay(userPresence.userId, mainHandResponse.newObjectUid, newBlankCardResponse.newObjectUid);
            }

            var landscapeCardAsset = assetController.CompanionAssetsByName["desert"];
            var newCardResponse = await cardController.CreateCompanionCard(userPresence.userId, IMAGE_CARD_ID, landscapeCardAsset.AssetGuid.ToString(), landscapeCardAsset.AssetGuid.ToString());
            if (newCardResponse.wasSuccessful)
            {
                await cardController.AddCardToHandDisplay(userPresence.userId, mainHandResponse.newObjectUid, newCardResponse.newObjectUid);
            }
        }

        public void sendOpenAIImageRequest()
        {
            results.text = "Prompts: A cute baby sea otter";
            openAIController.SendOpenAIImageRequest("A cute baby sea otter");
        }

        public void sendChatRequest()
        {
            openAIController.SendOpenAIChatNextMessage("Say this is a test!"); // sends next message sent from user by default

            openAIController.SendOpenAIChatNextMessage("I will be your assistant.", MessageRoleEnum.ASSISTANT, "assistant"); //specifying message role

            results.text = string.Join("\n", openAIController.currentMessages.Select(message => $"{message.role}: {message.content}").ToList());
        }

        public void sendConversationRequest()
        {
            //Specify the whole chat stream. This will clear out the current messages send from SendOpenAIChatNextMessage and replace it with the list of specified messages 
            openAIController.SendOpenAIChatConversation(new List<OpenAIMessage>() {
                new OpenAIMessage() {role = MessageRoleEnum.SYSTEM, content = "You are a helpful assistant."},
                new OpenAIMessage() {role = MessageRoleEnum.USER, content = "Hello!"}
            });

            results.text = "system: You are a helpful assistant. \nuser: Hello!";
        }

        private void ReceivedChat(OpenAIChatResponse response)
        {
            var textResponse = response.response;

            if (response.id == GENERATE_ID)
            {
                var prompt = textResponse.TrimStart('"').TrimEnd('"');

                foreach (var user in userPresenceController.Users.Values)
                {
                    cardController.SetCardText(user.userId, new CardTextProp
                    {
                        cardId = BLANK_CARD_ID,
                        text = prompt,
                        fontSize = 24,
                        horizontalPadding = 40,
                        verticalAdjust = 75
                    });
                }
            }
            else if (response.id != "assistant")
            {
                results.text += $"\n Received chat: {textResponse}";
            }
        }

        public void sendTextToImageRequestSimple()
        {
            results.text = "Prompt: A lighthouse on a cliff";
            stabilityAIController.SendTextToImageRequest("A lighthouse on a cliff"); //simple request
        }

        public void sendTextToImageRequestAnime()
        {
            results.text = "Prompt: A lighthouse on a cliff - Style: Anime";
            stabilityAIController.SendTextToImageRequest("A lighthouse on a cliff", 704, 512, StyleEnum.ANIME); //extra params
        }

        public void sendTextToImageRequestMultiplePrompts()
        {
            results.text = "Prompts: Dog - Weight: 50%\nCat - Weight 50%";

            //Multiple text prompts with weight of prompt
            stabilityAIController.SendTextToImageRequest(new List<TextPrompt>() {
                new TextPrompt() {weight = 0.5f, text = "Dog"},
                new TextPrompt() {weight = 0.5f, text = "Cat"}
            });
        }
        public void sendTextToImageRequestMultiplePromptsComic()
        {
            results.text = "Prompts: Dog - Weight: 20%\nCat - Weight 80%\n Style: Comic Book";

            //Extra params
            stabilityAIController.SendTextToImageRequest(new List<TextPrompt>() {
                new TextPrompt() {weight = 0.2f, text = "Dog"},
                new TextPrompt() {weight = 0.8f, text = "Cat"}
            }, 512, 704, StyleEnum.COMIC_BOOK);
        }

        public void SendImageToImageRequest()
        {
            results.text = "Sending following image: Prompt: A lighthouse on a cliff";
            displayImage.sprite = Sprite.Create(sendImage, new Rect(0, 0, sendImage.width, sendImage.height), new Vector2(.5f, .5f), 100);

            stabilityAIController.SendImageToImageRequest(sendImage, "A lighthouse on a cliff"); //simple request
        }

        public void SendImageToImageMultipleRequest()
        {
            results.text = "Sending following image with Prompts: battle scene (weight 50%) - fairies dancing (weight 50%)";
            displayImage.sprite = Sprite.Create(sendImage, new Rect(0, 0, sendImage.width, sendImage.height), new Vector2(.5f, .5f), 100);

            //Multiple text prompts with weight of prompt
            stabilityAIController.SendImageToImageRequest(sendImage, new List<TextPrompt>() {
                new TextPrompt() {weight = 0.5f, text = "battle scene"},
                new TextPrompt() {weight = 0.5f, text = "fairies dancing"}
            });
        }

        private void displayReceivedImage(Texture2D image)
        {
            results.text = "Received image:";
            displayImage.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), new Vector2(.5f, .5f), 100);
        }

        private void ReceivedImages(OpenAIImageResponse response)
        {
            var texture = response.images.First();
            displayReceivedImage(texture);
        }

        private void ReceivedImages(StabilityAIResponse response)
        {
            var texture = response.images.First();
            displayReceivedImage(texture);

            if (response.id == GENERATE_ID)
            {
                var companionTexture = assetController.AddTextureToAssets(texture);

                foreach (var user in userPresenceController.Users.Values)
                {
                    assetController.LoadAssetToCompanion(companionTexture, user.userId);

                    cardController.SetCardFrontAssetId(user.userId, new CardAssetId
                    {
                        cardId = IMAGE_CARD_ID,
                        assetId = companionTexture.AssetGuid.ToString()
                    });
                }
            }
        }
    }
}
