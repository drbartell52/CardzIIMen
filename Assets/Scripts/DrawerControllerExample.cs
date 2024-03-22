using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Gameboard.Examples
{
    public class DrawerControllerExample : MonoBehaviour
    {
        public Text Results;
        private DrawerController drawerController;

        private void Start()
        {
            GameObject gameboardObject = GameObject.FindWithTag("Gameboard");
            drawerController = gameboardObject.GetComponent<DrawerController>();
            StartCoroutine(FlashDrawers());
        }

        /// <summary>
        /// Set the visibility of the drawer containing player presence items on the gameboard device.
        /// </summary>
        public void ToggleDrawers()
        {
            var desiredState = !(drawerController.DrawersVisible ?? false);
            Results.text = $"Setting drawer visibility to {desiredState}";
            drawerController.SetDrawerVisibility(desiredState);
        }

        public void ShowDrawerIndicators()
        {
            drawerController.StartDrawerIndicator(1000);
        }

        private IEnumerator FlashDrawers()
        {
            Results.text = "Flashing Drawers. Showing...";
            drawerController.ShowDrawers();
            yield return new WaitForSeconds(1);
            Results.text = "Flashing Drawers. Hiding...";
            drawerController.HideDrawers();
            yield return new WaitForSeconds(1);
            Results.text = "Flashing Drawers. Showing...";
            drawerController.ShowDrawers();
            yield return new WaitForSeconds(1);
            Results.text = "Flashing Drawers. Hiding...";
            drawerController.HideDrawers();
            yield return new WaitForSeconds(1);
            Results.text = "Flashing Drawers. Showing...";
            drawerController.ShowDrawers();
            yield return new WaitForSeconds(1);
            Results.text = "Flashing Drawers. Hiding...";
            drawerController.HideDrawers();
            yield return new WaitForSeconds(1);
            Results.text = "Flashing Drawers. Showing...";
            drawerController.ShowDrawers();
            yield return new WaitForSeconds(1);

            Results.text = "Flashing Drawers complete.";
        }
    }
}

