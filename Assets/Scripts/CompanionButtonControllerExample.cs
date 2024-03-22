using Gameboard.EventArgs;
using UnityEngine;
using UnityEngine.UI;
using Gameboard.Objects;
using System.Threading.Tasks;
using static Gameboard.DataTypes;
using Gameboard.Objects.Buttons;
using Gameboard.Objects.DeviceEvent;

namespace Gameboard.Examples
{
    public class CompanionButtonControllerExample : MonoBehaviour
    {
        private CompanionButtonController companionButtonController;
        private UserPresenceController userPresenceController;
        private AssetController assetController;
        private CardController cardController;
        private DeviceEventController deviceEventController;

        private string userId;
        private CompanionTextureAsset buttonIdle;
        private CompanionTextureAsset buttonDown;

        public Text Results;

        private void Start()
        {
            GameObject gameboardObject = GameObject.FindWithTag("Gameboard");
            Gameboard gameboard = gameboardObject.GetComponent<Gameboard>();

            userPresenceController = gameboardObject.GetComponent<UserPresenceController>();
            companionButtonController = gameboardObject.GetComponent<CompanionButtonController>();
            assetController = gameboardObject.GetComponent<AssetController>();
            cardController = gameboardObject.GetComponent<CardController>();
            deviceEventController = gameboardObject.GetComponent<DeviceEventController>();

            companionButtonController.CompanionButtonPressed += OnCompanionButtonPressed;
            cardController.CardButtonPressed += OnCardButtonPressed;
            gameboard.GameboardShutdownBegun += OnGameboardShutdown;
        }

        private void OnDestroy()
        {
            companionButtonController.CompanionButtonPressed -= OnCompanionButtonPressed;
        }

        private void OnGameboardShutdown()
        {
            companionButtonController.CompanionButtonPressed -= OnCompanionButtonPressed;
        }


        public async void SetButtons()
        {
            userId = Utils.GetFirstCompanionUserId(userPresenceController);

            CompanionMessageResponseArgs[] responses = await companionButtonController.SetAndShowMultipleCompanionCardButtons(userId,
            new CompanionCardButton[]
                {
                    new CompanionCardButton("LeftBtnId", "LeftBtn", CardButtonPosition.Left),
                    new CompanionCardButton("CenterBtnId", "CenterBtn", CardButtonPosition.Center),
                    new CompanionCardButton("RightBtnId", "RightBtn", CardButtonPosition.Right),
                }
            );
        }

        public async void SetCompanionButton(CardButtonPosition cardPosition)
        {
            userId = Utils.GetFirstCompanionUserId(userPresenceController);

            CompanionMessageResponseArgs response = await companionButtonController.SetCompanionCardButton(
                userId,
                $"{cardPosition}Id",
                $"{cardPosition}.",
                $"{cardPosition}Callback",
                cardPosition);
            Results.text = $"response for companionButtonController.SetCardButton: {response}";
            GameboardLogging.Verbose($"response for companionButtonController.SetCardButton: {response}");

            CompanionMessageResponseArgs visibleResponse = await companionButtonController.SetCompanionButtonVisiblity(userId, $"{cardPosition}Id", true);
            Results.text = $"response for companionButtonController.SetCompanionButtonVisiblity: {visibleResponse}";
            GameboardLogging.Verbose($"response for companionButtonController.SetCompanionButtonVisiblity: {visibleResponse}");
        }

        public void SetRightCompanionButton()
        {
            this.SetCompanionButton(CardButtonPosition.Right);
        }

        public void SetCenterCompanionButton()
        {
            this.SetCompanionButton(CardButtonPosition.Center);
        }

        public void SetLeftCompanionButton()
        {
            this.SetCompanionButton(CardButtonPosition.Left);
        }

        /// <summary>
        /// Display an alert dialog on Companion with buttons. 
        /// Closes once a button is pressed. 
        /// Clicking buttons will send a companionButton pressed message 
        /// </summary>
        public async void ShowAlertDialog()
        {
            var userId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (userId == string.Empty)
            {
                Results.text = "There are no companion users connected.";
                return;
            }

            CompanionMessageResponseArgs response = await deviceEventController.DisplayAlertDialog(userId, new DisplayAlertDialog
            {
                title = "Alert Dialog Title",
                content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce eget risus nec sapien blandit gravida non at dui. Vivamus vehicula, tortor at consectetur auctor, erat ipsum imperdiet dolor, non venenatis quam erat et eros. Proin consequat libero turpis, vitae pellentesque ante vestibulum quis. Nulla porttitor gravida massa et rutrum. Ut ac ultricies enim. Sed semper pellentesque nisl, eu auctor arcu. Maecenas ac nisi sem. Praesent viverra mi a hendrerit dapibus. Nam urna nunc, tincidunt ullamcorper finibus sit amet, accumsan a nisl.",
                buttons = new EventArgSetCompanionButton[] {
                    new EventArgSetCompanionButton () {
                        buttonId = "noDialog",
                        buttonText = "No",
                    }, new EventArgSetCompanionButton () {
                        buttonId = "yesDialog",
                        buttonText = "Yes",
                    }
                }
            });

            Results.text = $"Sent alert dialog to user {userId}, Response: {response}";
        }

        private void OnCompanionButtonPressed(GameboardCompanionButtonPressedEventArgs companionButtonEvent)
        {
            Results.text = $"OnCompanionButtonPressed {companionButtonEvent}";
        }

        private void OnCardButtonPressed(GameboardCompanionCardsButtonPressedEventArgs companionButtonEvent)
        {
            Results.text = $"OnCardButtonPressed {companionButtonEvent}";
        }
    }
}
