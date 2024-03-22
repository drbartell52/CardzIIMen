using UnityEngine;
using Gameboard.EventArgs;
using Gameboard.Objects;
using Gameboard.Objects.DeviceEvent;
using System.Linq;
using UnityEngine.UI;

namespace Gameboard.Examples
{
    public class DeviceEventControllerExample : MonoBehaviour
    {
        public Text Results;
        private DeviceEventController deviceEventController;
        private UserPresenceController userPresenceController;
        private CompanionButtonController buttonController;
        private int labelSetCount = 0;

        private void Start()
        {
            GameboardLogging.Verbose("DeviceEventExample Start");
            GameObject gameboardObject = GameObject.FindWithTag("Gameboard");

            deviceEventController = gameboardObject.GetComponent<DeviceEventController>();
            userPresenceController = gameboardObject.GetComponent<UserPresenceController>();
            buttonController = gameboardObject.GetComponent<CompanionButtonController>();

            buttonController.CompanionButtonPressed += OnButtonPressed;
            GameboardLogging.Verbose("DeviceEventExample Start Success");
        }

        private void OnButtonPressed(GameboardCompanionButtonPressedEventArgs companionButtonEvent)
        {
            Results.text = $"OnCompanionButtonPressed {companionButtonEvent}";
        }

        /// <summary>
        /// Display a pop up message on Companion of the first user with the for 5 seconds. 
        /// </summary>
        public async void ShowDevicePopup()
        {
            var userId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (userId == string.Empty)
            {
                Results.text = "There are no companion users connected.";
                return;
            }

            CompanionMessageResponseArgs response = await deviceEventController.DisplaySystemPopup(userId, "This is a test message.\nThis is a continuation of the message.", 5);
            Results.text = $"Sent popup to user {userId}, Response: {response}";
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
                content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce eget risus nec sapien blandit gravida non at dui. Vivamus vehicula, tortor at consectetur auctor, erat ipsum imperdiet dolor, non venenatis quam erat et eros. Proin consequat libero turpis, vitae pellentesque ante vestibulum quis. Nulla porttitor gravida massa et rutrum. Ut ac ultricies enim. Sed semper pellentesque nisl, eu auctor arcu. Maecenas ac nisi sem. Praesent viverra mi a hendrerit dapibus. Nam urna nunc, tincidunt ullamcorper finibus sit amet, accumsan a nisl."
            });

            Results.text = $"Sent alert dialog to user {userId}, Response: {response}";
        }

        /// <summary>
        /// Hide alert dialog on Companion
        /// </summary>
        public async void HideAlertDialog()
        {
            var userId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (userId == string.Empty)
            {
                Results.text = "There are no companion users connected.";
                return;
            }

            CompanionMessageResponseArgs response = await deviceEventController.DisplayAlertDialog(userId, null);

            Results.text = $"Hide alert dialog for user {userId}, Response: {response}";
        }

        /// <summary>
        /// Display an alert dialog on Companion with buttons. 
        /// Closes once a button is pressed. 
        /// Clicking buttons will send a companionButton pressed message 
        /// </summary>
        public async void ShowAlertDialogCustomized()
        {
            var userId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (userId == string.Empty)
            {
                Results.text = "There are no companion users connected.";
                return;
            }

            CompanionMessageResponseArgs response = await deviceEventController.DisplayAlertDialog(userId, new DisplayAlertDialog()
            {
                title = "Alert Dialog Title",
                content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce eget risus nec sapien blandit gravida non at dui. Vivamus vehicula, tortor at consectetur auctor, erat ipsum imperdiet dolor, non venenatis quam erat et eros. Proin consequat libero turpis, vitae pellentesque ante vestibulum quis. Nulla porttitor gravida massa et rutrum. Ut ac ultricies enim. Sed semper pellentesque nisl, eu auctor arcu. Maecenas ac nisi sem. Praesent viverra mi a hendrerit dapibus. Nam urna nunc, tincidunt ullamcorper finibus sit amet, accumsan a nisl.",
                font = "Courier New",
                fontColor = "red",
                buttonFontColor = "#3252a8",
                // backgroundAssetId = "assetIdForBackground",
                buttons = new EventArgSetCompanionButton[] { new EventArgSetCompanionButton () {
                    buttonId = "noDialog",
                    buttonText = "No",
                }, new EventArgSetCompanionButton () {
                    buttonId = "yesDialog",
                    buttonText = "Yes",
                }}
            });

            Results.text = $"Sent alert dialog to user {userId}, Response: {response}";
        }

        /// <summary>
        /// Reset the first companion user's play panel
        /// </summary>
        public async void ResetUserPlayPanel()
        {
            var userId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (userId == string.Empty)
            {
                Results.text = "There are no companion users connected.";
                return;
            }

            CompanionMessageResponseArgs response = await deviceEventController.ResetPlayPanel(userId);
            Results.text = $"reset user play panel for {userId}, Response: {response}";
        }

        /// <summary>
        /// Soft Resets the first companion user's play panel (keeps cards, removes buttons)
        /// </summary>
        public async void SoftResetUserPlayPanelDefault()
        {
            var userId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (userId == string.Empty)
            {
                Results.text = "There are no companion users connected.";
                return;
            }

            CompanionMessageResponseArgs response = await deviceEventController.SoftResetPlayPanel(userId);
            Results.text = $"soft reset default user play panel for {userId}, Response: {response}";
        }

        /// <summary>
        /// Soft Resets the first companion user's play panel, destroys cards but keeps the buttons
        /// </summary>
        public async void SoftResetUserPlayPanelDestroyCards()
        {
            var userId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (userId == string.Empty)
            {
                Results.text = "There are no companion users connected.";
                return;
            }

            CompanionMessageResponseArgs response = await deviceEventController.SoftResetPlayPanel(userId, true, false);
            Results.text = $"Soft reset user play panel for {userId}, Response: {response}";
        }

        /// <summary>
        /// Vibrate the first companion user's device one time
        /// </summary>
        public async void VibrateCompanionDevice()
        {
            var userId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (userId == string.Empty)
            {
                Results.text = "There are no companion users connected.";
                return;
            }

            CompanionMessageResponseArgs response = await deviceEventController.VibrateCompanionDevice(userId);
            Results.text = $"companion device vibrated for {userId}, Response: {response}";
        }

        /// <summary>
        /// Vibrate the first companion user's device 6 times with increasing delay between each vibration
        /// </summary>
        public async void VibrateCompanionDeviceMultiple()
        {
            var userId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (userId == string.Empty)
            {
                Results.text = "There are no companion users connected.";
                return;
            }

            CompanionMessageResponseArgs response = await deviceEventController.VibrateCompanionDevice(userId, new uint[] { 0, 400, 800, 1200, 4000, 10000, 4294967295 });
            Results.text = $"companion device vibrated for {userId}, Response: {response}";
        }

        /// <summary>
        /// Sets the label text at the top of the target user's Companion device
        /// </summary>
        public async void SetTopLabelCounter()
        {
            var userId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (userId == string.Empty)
            {
                Results.text = "There are no companion users connected.";
                return;
            }

            labelSetCount++;
            CompanionMessageResponseArgs response = await deviceEventController.SetTopLabel(userId, $"Label Set {labelSetCount} times.");
            Results.text = $"top label was set for {userId}, Response: {response}";
        }

        /// <summary>
        /// Sets the label text at the top of the target user's Companion device with long text demonstrating the 18 character limit
        /// </summary>
        public async void SetTopLabelLong()
        {
            var userId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (userId == string.Empty)
            {
                Results.text = "There are no companion users connected.";
                return;
            }

            labelSetCount++;
            CompanionMessageResponseArgs response = await deviceEventController.SetTopLabel(userId, $"1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890");
            Results.text = $"top label was set for {userId}, Response: {response}";
        }

        /// <summary>
        /// Sets the label text at the top of the target user's Companion device with empty text demonstrating removal of the label
        /// </summary>
        public async void SetTopLabelEmpty()
        {
            var userId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (userId == string.Empty)
            {
                Results.text = "There are no companion users connected.";
                return;
            }

            labelSetCount++;
            CompanionMessageResponseArgs response = await deviceEventController.SetTopLabel(userId, $"");
            Results.text = $"top label was set for {userId}, Response: {response}";
        }
    }
}

