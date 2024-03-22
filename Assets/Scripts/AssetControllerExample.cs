using Gameboard.EventArgs;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Gameboard.Objects;

namespace Gameboard.Examples
{
    /// <summary>
    /// The asset controller handles data management for assets on the companion app.
    /// Assets are required to be loaded prior to being used.
    /// </summary>
    public class AssetControllerExample : MonoBehaviour
    {
        private UserPresenceController userPresenceController;
        private AssetController assetController;

        public Text Results;

        /// <summary>
        /// Texture2D assets defined in unity will be used to load into the companion app.
        /// </summary>
        public List<Texture2D> TextureAssets;

        private void Start()
        {
            GameObject gameboardObject = GameObject.FindWithTag("Gameboard");
            Gameboard gameboard = gameboardObject.GetComponent<Gameboard>();

            userPresenceController = gameboardObject.GetComponent<UserPresenceController>();
            assetController = gameboardObject.GetComponent<AssetController>();

            gameboard.GameboardShutdownBegun += OnGameboardShutdown;
        }

        public void TestCreateAssets()
        {
            AddFilesToCompanionAssets();
        }

        public async void TestLoadAssets()
        {
            await LoadAllAssets();
        }

        public async void TestDeleteAssets()
        {
            await DeleteAllAssetsFromCompanion();
        }

        private async void OnGameboardShutdown()
        {
            await DeleteAllAssetsFromCompanion();
        }

        /// <summary>
        /// Create ICompanionAsset objects to use when loading onto the companion app.
        /// </summary>
        private void AddFilesToCompanionAssets()
        {
            List<ICompanionAsset> newAssets = new List<ICompanionAsset>();
            TextureAssets.ForEach(t =>
            {
                CompanionTextureAsset asset = assetController.AddTextureToAssets(t);
                // If you don't want the asset to be loaded with the LoadAll methods, set the loadWithLoadAll flag to false
                // CompanionTextureAsset asset = assetController.AddTextureToAssets(t, false);
                newAssets.Add(asset);
            });


            var logMessage = $"Created {newAssets.Count} new assets, {assetController.CompanionAssets.Count} total.";
            Results.text = logMessage;
            GameboardLogging.Verbose(logMessage);
        }

        public void AddToAssetsFromPath()
        {
            var textureDelegate = new AssetController.AddAsset<CompanionTextureAsset, Texture2D>(assetController.AddTextureToAssets);
            var textureAssetsFromPath = assetController.CreateCompanionAssetsFromPath("Cards", textureDelegate);
            GameboardLogging.Verbose($"textureAssetsFromPath count {textureAssetsFromPath.Count}");

            GameboardLogging.Verbose($"assetController.CompanionAssets count: {assetController.CompanionAssets.Count}");
            GameboardLogging.Verbose($"assetController.LoadedAssets count: {assetController.LoadedAssets.Count}");

            Results.text = $"assetController.CompanionAssets count: {assetController.CompanionAssets.Count}";
            var userId = Utils.GetFirstCompanionUserId(userPresenceController);
            Results.text += $"/nassetController.LoadedAssets count: {assetController.LoadedAssets[userId].Count}";
        }

        /// <summary>
        /// Load all assets objects that were previously instantiated with CreateCompanionAssets 
        /// onto the companion app cor each connected user presence via the AssetController.
        /// </summary>
        /// <returns></returns>
        private async Task LoadAllAssets()
        {
            Results.text = "";
            var results = await assetController.LoadAllAssetsOntoAllCompanions();
            results.ForEach(r => Results.text += $"Loaded assetId, result={r}");
            var userId = Utils.GetFirstCompanionUserId(userPresenceController);
            Results.text += $"/n LoadedAssets count: {assetController.LoadedAssets[userId].Count}";
        }

        /// <summary>
        /// Delete all assets objects that were previously loaded via the LoadAssetsOntoCompanion.
        /// </summary>
        /// <returns></returns>
        private async Task DeleteAllAssetsFromCompanion()
        {
            Results.text = "";
            var loadedAssetCount = 0;
            foreach (var asset in assetController.LoadedAssets)
                loadedAssetCount += asset.Value.Count;

            GameboardLogging.Verbose($"Loaded Assets Count = {loadedAssetCount}");
            List<string> keys = new List<string>(assetController.LoadedAssets.Keys);

            foreach (var key in keys)
            {
                var assets = new List<ICompanionAsset>(assetController.LoadedAssets[key]);
                foreach (var asset in assets)
                {
                    CompanionMessageResponseArgs result = await asset.DeleteAssetFromCompanion(userPresenceController, key);
                    var resultMessage = $"Deleted assetId={asset.AssetGuid} for userId={key}, result={result}";
                    Results.text += resultMessage + "\n";
                    GameboardLogging.Verbose(resultMessage);
                }
            }

            loadedAssetCount = 0;
            foreach (var asset in assetController.LoadedAssets)
                loadedAssetCount += asset.Value.Count;

            GameboardLogging.Verbose($"Loaded Assets after Delete Count = {loadedAssetCount}");
        }

    }

}
