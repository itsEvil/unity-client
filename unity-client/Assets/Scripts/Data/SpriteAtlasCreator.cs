using Static;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;

namespace Data {
    public class SpriteAtlasCreator {
        private const int AtlasSize = 4096;
        private static readonly Dictionary<int, Texture2D> _atlases = new();
        private static readonly Dictionary<string, Dictionary<int, Sprite>> Images = new();
        private static readonly Dictionary<string, Dictionary<int, CharacterAnimation>> Animations = new();
        private static readonly Dictionary<string, SpriteSheetData> IdToSheetData = new();
        private static int _atlasX, _atlasY;
        static SpriteAtlasCreator() {
            var atlas = new Texture2D(AtlasSize, AtlasSize) {
                filterMode = FilterMode.Point
            };

            for (int x = 0; x < AtlasSize; x++)
                for(int y = 0; y < AtlasSize; y++)
                    atlas.SetPixel(x, y, Color.clear);

            atlas.Apply();
            _atlases[0] = atlas;
        }
        /// <summary>
        /// Returns a sprite at the given index from the texture atlas
        /// </summary>
        public static Sprite GetSprite(string fileName, int index) {
            if(!Images.TryGetValue(fileName, out var dict)) {
                if (IdToSheetData.TryGetValue(fileName, out var data))
                    return GetSprite(data.SheetName, index);

                Utils.Error("Sprite not found in Image database {0}:{1}", fileName, index);
                return GetSprite("ErrorTexture", 0);
            }

            if(!dict.TryGetValue(index, out var sprite)) {
                Utils.Error("Index not found in Images database {0}:{1}", fileName, index);
                return GetSprite("ErrorTexture", 0);
            }

            return sprite;
        }
        /// <summary>
        /// Gets a certain amount of sprites in a row starting from the given index
        /// </summary>
        public static Sprite[] GetSprites(string fileName, int index, int amount = 6) {
            Sprite[] sprites = new Sprite[amount];
            for(int i = 0; i < sprites.Length; i++)
                sprites[i] = GetSprite(fileName, index + i);
            
            return sprites;
        }

        public static CharacterAnimation GetCharacterAnimation(string fileName, int index) {
            if (!Animations.TryGetValue(fileName, out var dict)) {
                if (IdToSheetData.TryGetValue(fileName, out var data))
                    return GetCharacterAnimation(data.SheetName, index);
                Utils.Error("Char anim not found in Animations database {0}", fileName);
                return GetCharacterAnimation("ErrorAnimation", 0);
            }

            if (!dict.TryGetValue(index, out var animation)) {
                Utils.Error("Index not found in Animations database {0}:{1}", fileName, index);
                return GetCharacterAnimation("ErrorAnimation", 0);
            }

            return animation;
        }

        public static void AddImages(Texture2D texture, SpriteSheetData sheetData) {
            //Utils.Log("Parsing {0} | Height: {1} | Width: {2} | X: {3} | Y: {4}", sheetData.Id, sheetData.ImageHeight, sheetData.ImageWidth, _atlasX, _atlasY);
            IdToSheetData[sheetData.Id] = sheetData;
            var index = 0;
            var atlas = _atlases[0];
            var imageHeight = sheetData.ImageHeight;
            var imageWidth = sheetData.ImageWidth;
            var targetRect = new Rect(0, texture.height, texture.width, texture.height);
            //loop over all sprites in the texture

            if (!Images.TryGetValue(sheetData.SheetName, out var sheetSprites))
                sheetSprites = new();

            for (var y = targetRect.y - imageHeight; y >= targetRect.y - targetRect.height; y -= imageHeight) {
                for (var x = targetRect.x; x < targetRect.x + targetRect.width; x += imageWidth) {
                    if (SpriteUtils.IsTransparent(texture, targetRect))
                        continue;

                    try {

                        var rect = CopyTexture(texture, atlas, imageHeight, imageWidth, (int)y, (int)x);
                        sheetSprites[index++] = Sprite.Create(atlas, rect, new Vector2(0.5f, 0), 8);
                        Images[sheetData.SheetName] = sheetSprites;

                    }
                    catch(Exception e) {
                        Utils.Error("{0}\n{1}", e.Message, e.StackTrace);
                    }
                }
            }

            atlas.Apply();
        }

        public static void AddAnimations(Texture2D texture, SpriteSheetData sheetData) {
            Utils.Warn("Parsing {0}|{1} animation", sheetData.Id, sheetData.SheetName);
            IdToSheetData[sheetData.Id] = sheetData;
            var animationIndex = 0;
            var imageIndex = 0;
            var atlas = _atlases[0];
            var imageHeight = sheetData.ImageHeight;
            var imageWidth = sheetData.ImageWidth;
            bool isPlayers = sheetData.Id.Equals("players");

            if (!Images.TryGetValue(sheetData.SheetName, out var sheetSprites))
                sheetSprites = new();

            for(var y = texture.height - sheetData.ImageHeight; y >= 0; y -= sheetData.AnimationHeight) {
            //for(var y = 0; y < texture.height; y+= sheetData.AnimationHeight) {
                List<Sprite> frames = new();
                for(int i = 0; i < sheetData.RowCount; i++) {
                    for (var x = 0; x < sheetData.AnimationWidth; x += sheetData.ImageWidth) {
                        var rect = CopyTexture(texture, atlas, imageHeight, imageWidth, y - (i * sheetData.ImageHeight), x);
                        var sprite = Sprite.Create(atlas, rect, new Vector2(0.5f, 0), 8);
                        sheetSprites[imageIndex++] = sprite;
                        frames.Add(sprite);
                    }
                }

                var animation = new CharacterAnimation(frames, sheetData.StartFacing);
                if(!Animations.TryGetValue(sheetData.SheetName, out var dict))
                    dict = new();

                if (isPlayers)
                    Utils.Log("Added Players animation index {0}", animationIndex);

                dict[animationIndex++] = animation;
                Animations[sheetData.SheetName] = dict;
            }
            Images[sheetData.SheetName] = sheetSprites;
            atlas.Apply();
        }

        private static Rect CopyTexture(Texture2D texture, Texture2D atlas, int imageHeight, int imageWidth, int y, int x, int counter = 0) {
            if(counter > 8) {
                Utils.Error("Failed to find a spot to copy our texture. ");
                return Rect.zero;
            }
            
            if(_atlasX + imageWidth + 4 > AtlasSize) {
                _atlasX = 0;
                _atlasY += imageHeight + 4;
            }

            //Potentially infinite loop?
            if (!SpriteUtils.IsTransparent(atlas, new Rect(_atlasX, _atlasY, imageWidth + 4, imageHeight + 4))) {
                _atlasX += imageWidth + 4;
                return CopyTexture(texture, atlas, imageHeight, imageWidth, y, x, counter++);
            }
            Utils.Log("Getting pixels x:{0} y:{1} w:{2} h:{3} | texture w:{4} h:{5}", x,y,imageWidth, imageHeight, texture.width, texture.height);
            var colours = texture.GetPixels(x, y, imageWidth, imageHeight, 0);
            atlas.SetPixels(_atlasX + 2, _atlasY + 2, imageWidth, imageHeight, colours);

            var tempX = _atlasX;
            var tempY = _atlasY;

            _atlasX += imageWidth + 4;
            return new Rect(tempX, tempY, imageWidth + 4, imageHeight + 4);
        }
        public static void InitErrorTexture() {
            List<Sprite> sprites = new();
            var errorSprite = Images["ErrorTexture"][0];
            for (int i = 0; i < 6; i++)
                sprites.Add(errorSprite);

            var dict = Animations["ErrorAnimation"] = new Dictionary<int, CharacterAnimation>();
            dict[0] = new CharacterAnimation(sprites, Facing.Right);
            
            Utils.Log("Loaded Error Animation");
        }
        //Test stuff
        public static Texture2D GetAtlasTexture(int index = 0) => _atlases[index];
        public static Dictionary<string, Dictionary<int, Sprite>> GetImages() => Images;

    }
}
