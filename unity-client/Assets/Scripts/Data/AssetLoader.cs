using Static;
using System;
using System.Xml.Linq;
using UnityEngine;

namespace Data {
    internal class AssetLoader : MonoBehaviour {
        public void Awake() {
            try {
                LoadSpriteSheets();
            }
            catch(Exception e) {
                Utils.Error("Load sprite sheet exception {0}::{1}", e.Message, e.StackTrace);
            }
            try {
                LoadXmls();
            }
            catch(Exception e) {
                Utils.Error("Load xml data exception {0}::{1}", e.Message, e.StackTrace);
            }
            Destroy(gameObject);
        }

        private void LoadSpriteSheets() {
            var spritesXml = XElement.Parse(Resources.Load<TextAsset>("SpriteSheets/SpriteSheets").text);

            foreach (var sheetXml in spritesXml.Elements("Sheet")) {
                var sheetData = new SpriteSheetData(sheetXml);
                Utils.Log("Adding sheet {0}, {1}", sheetData.Id, sheetData.SheetName);

                var texture = Resources.Load<Texture2D>($"SpriteSheets/{sheetData.SheetName}");

                try {
                    if (sheetData.IsAnimation()) {
                        AssetLibrary.AddAnimations(texture, sheetData);
                    }
                    else {
                        AssetLibrary.AddImages(texture, sheetData);
                    }
                }
                catch (Exception e) {
                    Utils.Warn($"Unable to add {sheetData.Id}");
                    Utils.Error(e.Message+"\n"+e.StackTrace);
                }
            }
        }

        private void LoadXmls() {
            var xmlAssets = Resources.LoadAll<TextAsset>("Xmls");
            foreach (var xmlAsset in xmlAssets) {
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
