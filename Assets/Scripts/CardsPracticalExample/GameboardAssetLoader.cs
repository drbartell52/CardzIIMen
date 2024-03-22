using System.Collections.Generic;
using Gameboard.Objects;
using UnityEngine;
namespace Gameboard.CardsPracticalExample
{
    public class GameboardAssetLoader : MonoBehaviour
    {
        // Assets set in editor
        [SerializeField] private List<Texture2D> TextureAssets;

        [HideInInspector] public string backgroundGuid, selectAssetGuid;
        [HideInInspector] public List<CompanionTextureAsset> cardAssets = new List<CompanionTextureAsset>();
        [HideInInspector] public List<CompanionTextureAsset> resourceCardAssets = new List<CompanionTextureAsset>();
        [HideInInspector] public AssetController assetController;

        void Start()
        {
            GameObject gameboardObject = GameObject.FindWithTag("Gameboard");

            assetController = gameboardObject.GetComponent<AssetController>();

            CreateCompanionAssets();
        }

        /// <summary>
        /// Loads and keeps track of companion assets
        /// </summary>
        public void CreateCompanionAssets()
        {
            TextureAssets.ForEach(t =>
            {
                byte[] imageBytes = t.EncodeToPNG();

                CompanionTextureAsset asset = new CompanionTextureAsset(imageBytes, assetController);

                if (t.name == "background") backgroundGuid = asset.AssetGuid.ToString();

                if (t.name == "CardSelectAsset") selectAssetGuid = asset.AssetGuid.ToString();
            });

            // Create texture assets to load into the companion app
            var textureDelegate = new AssetController.AddAsset<CompanionTextureAsset, Texture2D>(assetController.AddTextureToAssets);
            cardAssets = assetController.CreateCompanionAssetsFromPath("Cards", textureDelegate);

            var resourceTextureDelegate = new AssetController.AddAsset<CompanionTextureAsset, Texture2D>(assetController.AddTextureToAssets);
            resourceCardAssets = assetController.CreateCompanionAssetsFromPath("ResourceCards", resourceTextureDelegate);
        }

    }
}
