using Data;
using Static;
using System.Xml.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Tests
{

    public class AtlasCreatorTest : MonoBehaviour {
        [SerializeField] private AtlasPrefab Prefab;
        [SerializeField] private Texture2D AtlasTexture;
        [SerializeField] private SpriteRenderer Renderer;
        public void Awake()
        {
            //AssetLoader_LoadSpriteSheets();
            //AssetLoader_LoadXmls();

            //CreateObjects();
        }

        private void Start() {
            AtlasTexture = SpriteAtlasCreator.GetAtlasTexture(0);
            Renderer.sprite = Sprite.Create(AtlasTexture, new Rect(0, 0, AtlasTexture.width, AtlasTexture.height), Vector2.zero);
        }

        private void CreateObjects()
        {
            int counter = 0;
            int xOffset = 0;
            int yOffset = 0;
            foreach (var (_, images) in SpriteAtlasCreator.GetImages())
            {
                foreach (var (_, image) in images)
                {
                    var obj = Instantiate(Prefab, transform);
                    obj.Renderer.sprite = image;
                    obj.transform.position = new Vector3(xOffset++, yOffset, -5);
                    obj.transform.localScale = new Vector3(5, 5);
                    counter++;
                    if (xOffset > 100)
                    {
                        xOffset = 0;
                        yOffset++;
                    }
                }
            }
            Utils.Log("Created {0} objects", counter);
        }

        public void AssetLoader_LoadSpriteSheets() {
            var spritesXml = XElement.Parse(Resources.Load<TextAsset>("SpriteSheets/SpriteSheets").text);

            foreach (var sheetXml in spritesXml.Elements("Sheet")) {
                var sheetData = new SpriteSheetData(sheetXml);
                var texture = Resources.Load<Texture2D>($"SpriteSheets/{sheetData.SheetName}");
                if (sheetData.IsAnimation())
                    SpriteAtlasCreator.AddAnimations(texture, sheetData);
                else
                    SpriteAtlasCreator.AddImages(texture, sheetData);

                if (sheetData.Id == "ErrorTexture") {
                    SpriteAtlasCreator.InitErrorTexture();
                }
            }
            AtlasTexture = SpriteAtlasCreator.GetAtlasTexture(0);
            Renderer.sprite = Sprite.Create(AtlasTexture, new Rect(0,0, AtlasTexture.width, AtlasTexture.height), Vector2.zero);
        }

        private void AssetLoader_LoadXmls()
        {
            var xmlAssets = Resources.LoadAll<TextAsset>("Xmls");
            foreach (var xmlAsset in xmlAssets)
            {
                Utils.Log("Parsing {0} xml", xmlAsset.name);
                var xml = XElement.Parse(xmlAsset.text);
                AssetLibrary.ParseXml(xml);
            }

            Utils.Log($"Loaded {AssetLibrary.Type2ItemDesc.Count} items");
            Utils.Log($"Loaded {AssetLibrary.Type2ObjectDesc.Count} objects");
            Utils.Log($"Loaded {AssetLibrary.Type2TileDesc.Count} tiles");
        }
    }

}