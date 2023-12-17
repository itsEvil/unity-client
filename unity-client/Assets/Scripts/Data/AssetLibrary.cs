using Static;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace Data {
    public static class AssetLibrary {

        private static readonly Dictionary<string, List<CharacterAnimation>> Animations = new();
        private static readonly Dictionary<string, List<Sprite>> Images = new();

        private static readonly Dictionary<ushort, Color> Type2Color = new();
        public static readonly Dictionary<ushort, ObjectDesc> Type2ObjectDesc = new();
        public static readonly Dictionary<string, ObjectDesc> Id2ObjectDesc = new();
        public static readonly Dictionary<ushort, TileDesc> Type2TileDesc = new();
        public static readonly Dictionary<ushort, ItemDesc> Type2ItemDesc = new();
        public static readonly Dictionary<ushort, PlayerDesc> Type2PlayerDesc = new();
        public static readonly Dictionary<ushort, List<SkinDesc>> ClassType2Skins = new();
        public static readonly Dictionary<ushort, ObjectDesc> Type2ProjectileDesc = new();
        public static readonly Dictionary<string, ObjectDesc> Id2ProjectileDesc = new();
        public static readonly Dictionary<ushort, SkinDesc> Type2SkinDesc = new();
        public static void AddAnimations(Texture2D texture, SpriteSheetData data) {
            if (!Animations.ContainsKey(data.Id))
                Animations[data.Id] = new List<CharacterAnimation>();

            try {
                for (var y = texture.height; y >= 0; y -= data.AnimationHeight) {
                    for (var x = 0; x < data.AnimationWidth; x += data.AnimationWidth) {
                        var rect = new Rect(x, y, data.AnimationWidth, data.AnimationHeight);
                        var frames = SpriteUtils.CreateSprites(texture, rect, data.ImageWidth, data.ImageHeight);
                        var animation = new CharacterAnimation(frames, data.StartFacing);
            
                        Animations[data.Id].Add(animation);
                    }
                }
            }
            catch(Exception e) {
                Utils.Error(e.Message + "\n" + e.StackTrace);
            }
        }
        public static void AddImages(Texture2D texture, SpriteSheetData data) {
            if (!Images.ContainsKey(data.Id))
                Images[data.Id] = new List<Sprite>();

            var rect = new Rect(0, texture.height, texture.width, texture.height);
            Images[data.Id] = SpriteUtils.CreateSprites(texture, rect, data.ImageWidth, data.ImageHeight);
        }
        public static void ParseXml(XElement xml) {
            foreach (var objectXml in xml.Elements("Object"))
            {
                var id = objectXml.ParseString("@id");
                var type = objectXml.ParseUshort("@type");
                switch (objectXml.ParseEnum("Class", ObjectType.GameObject)) {
                    case ObjectType.Skin:
                        var skinDesc = new SkinDesc(objectXml, id, type);
                        if (!ClassType2Skins.TryGetValue(skinDesc.Type, out var skinList))
                            skinList = new List<SkinDesc>();
                        skinList.Add(skinDesc);
                        ClassType2Skins[skinDesc.Type] = skinList;
                        break;
                    case ObjectType.Player:
                        SkinDesc defSkinDesc = new(objectXml, id, type);
                        if (!ClassType2Skins.TryGetValue(type, out var list))
                            list = new List<SkinDesc>();
                        list.Add(defSkinDesc);
                        ClassType2Skins[type] = list;
                        Type2PlayerDesc[type] = new PlayerDesc(objectXml, id, type);
                        break;
                    case ObjectType.Equipment:
                    case ObjectType.Dye:
                        Type2ItemDesc[type] = new ItemDesc(objectXml, id, type);
                        break;
                }
               Id2ObjectDesc[id] = Type2ObjectDesc[type] = new ObjectDesc(objectXml, id, type);
            }

            foreach (var skinList in ClassType2Skins.Values) {
                foreach (var skin in skinList) {
                    Type2SkinDesc[skin.Type] = skin;
                }
            }

            foreach (var groundXml in xml.Elements("Ground")) {
                var id = groundXml.ParseString("@id");
                var type = groundXml.ParseUshort("@type");
                Type2TileDesc[type] = new TileDesc(groundXml, id, type);
            }
        }

        public static Color GetTileColor(ushort type) {
            if (Type2Color.TryGetValue(type, out var ret))
                return ret;

            var desc = Type2TileDesc[type];
            Color color = SpriteUtils.MostCommonColor(desc.TextureData.GetTexture());
            
            Type2Color[type] = color;
            return color;
        }

        public static Sprite GetTileImage(ushort type) {
            return Type2TileDesc[type].TextureData.GetTexture();
        }

        public static List<Sprite> GetImageSet(string sheetName) {
            return Images[sheetName];
        }

        public static Sprite GetImage(string sheetName, int index) {
            if(!Images.TryGetValue(sheetName, out var list)) {
                Utils.Error("Sheet name {0} not found", sheetName);
                return Images["ErrorTexture"][0];
            }

            if(index > list.Count) {
                Utils.Error("{0} is out of bounds {1}", index, list.Count);
                return Images["ErrorTexture"][0];
            }

            return list[index];
        }

        public static CharacterAnimation GetAnimation(string sheetName, int index) {
            if(!Animations.TryGetValue(sheetName, out var list)) {
                Utils.Error("{0} not found", sheetName);
                return Animations["ErrorAnimation"][0];
            }

            if(index > list.Count) {
                Utils.Error("{0} is out of bounds {1}", index, list.Count);
                return Animations["ErrorAnimation"][0];
            }

            return list[index];
        }

        //public static GameObject GetModel(string modelName)
        //{
        //    if (Models.TryGetValue(modelName, out GameObject obj))
        //    {
        //        return obj;
        //    }
        //    Debug.LogWarning($"Model {modelName} not found in models!");
        //    return Models["Tower"];
        //}

        public static ObjectDesc GetObjectDesc(ushort type) {
            if (Type2ObjectDesc.TryGetValue(type, out ObjectDesc val)) {
                return val;
            }
            Debug.LogError($"Type {type} not found in gameData! Using Pirate!");
            return Id2ObjectDesc["Pirate"];
        }

        public static ObjectDesc GetObjectDesc(string id) {
            if (Id2ObjectDesc.TryGetValue(id, out ObjectDesc val)) {
                return val;
            }
            Debug.LogError($"Id {id} not found in gameData!\nUsing Pirate!");
            return Id2ObjectDesc["Pirate"];
        }

        public static TileDesc GetTileDesc(ushort type) {
            if (Type2TileDesc.TryGetValue(type, out TileDesc val)) {
                return val;
            }
            Debug.LogError($"Tile {type} not found in the gameData! Using grass tile!");
            return Type2TileDesc[0x46]; //Light grass
        }

        public static ItemDesc GetItemDesc(ushort type) {
            if (Type2ItemDesc.TryGetValue(type, out ItemDesc val)) {
                return val;
            }//
            Debug.LogError($"Item {type} type not found in the game data! Using sword!");
            return Type2ItemDesc[0xa00]; //basic sword
        }

        public static PlayerDesc GetPlayerDesc(ushort type) {
            if (Type2PlayerDesc.TryGetValue(type, out var desc)) {
                return desc;
            }
            return Type2PlayerDesc[0x0300]; //rogue
        }
        public static int GetMatchingSlotIndex(ItemType slotType, Entity entity) {
            //We only want specific items 
            if (slotType == ItemType.All) {
                return -1;
            }

            if (entity is not Player plr)
                return -1;

            var slots = plr.SlotTypes.AsSpan();

            for (int i = 0; i < slots.Length; i++) {
                if (slots[i] == slotType)
                    return i; //we found a slot that matches the item slotType
            }

            return -1; //not found
        }
    }
}
