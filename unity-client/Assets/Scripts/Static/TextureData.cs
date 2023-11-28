using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Static {
    public class TextureData {
        public CharacterAnimation Animation { get; private set; }
        public Sprite Texture { get; private set; }
        public TextureData[] RandomTextureData { get; private set; }
        public Dictionary<int, TextureData> AltTextures { get; private set; }
        public Texture Model_Texture;
        public string TextureName = "None";
        public bool Invisible = false;
        public float Chance;
        public TextureData(XElement xml) {
            if (xml.Element("Texture") != null) {
                Parse(xml.Element("Texture"));
            }
            else if (xml.Element("AnimatedTexture") != null) {
                Parse(xml.Element("AnimatedTexture"));
            }
            else if (xml.Element("RandomTexture") != null) {
                Parse(xml.Element("RandomTexture"));
            }
            else {
                Parse(xml);
            }

            Chance = xml.ParseFloat("@chance", 1f);

            if (xml.Element("AltTexture") != null) {
                AltTextures = GetAltTextures(xml);
                Texture = AltTextures.First().Value.Texture;
            }

            if (TextureName.Contains("invisible", StringComparison.InvariantCultureIgnoreCase))
                Invisible = true;
        }

        public Sprite GetTexture(int id = 0) {
            if (RandomTextureData == null)
                return Texture;

            //if the first object chance is 100% just choose a random one
            if (RandomTextureData[0].Chance == 1f){
                return RandomTextureData[id % RandomTextureData.Length].GetTexture(id);
            }

            for (int i = 0; i < RandomTextureData.Length; i++) {
                if (FPMathUtils.Chance(RandomTextureData[i].Chance)) {
                    return RandomTextureData[i].GetTexture(id);
                }
            }

            return RandomTextureData[0].GetTexture(id); //if we fail at all chances choose the first one
        }

        public TextureData GetAltTextureData(int id) {
            return AltTextures?[id];
        }
        private void Parse(XElement textureXml) {
            switch (textureXml.Name.ToString()) {
                case "Texture":
                    Texture = GetTexture(textureXml);
                    TextureName = GetTextureName(textureXml);
                    break;
                case "AnimatedTexture":
                    Animation = GetAnimatedTexture(textureXml);//, m_Type);
                    TextureName = GetTextureName(textureXml);
                    Texture = Animation.ImageFromAngle(0, Action.Stand, 0);
                    break;
                case "RandomTexture":
                    RandomTextureData = GetRandomTexture(textureXml);
                    Texture = RandomTextureData[0].Texture;
                    break;
            }
        }

        private static Sprite GetTexture(XElement textureXml) {
            var sheetName = textureXml.ParseString("File");
            var index = textureXml.ParseUshort("Index");
            return AssetLibrary.GetImage(sheetName, index);
        }

        private static string GetTextureName(XElement textureXml) {
            return textureXml.ParseString("File");
        }

        private static CharacterAnimation GetAnimatedTexture(XElement textureXml) {
            var sheetName = textureXml.ParseString("File");
            var index = textureXml.ParseUshort("Index");
            return AssetLibrary.GetAnimation(sheetName, index);
        }

        private static TextureData[] GetRandomTexture(XElement textureXml) {
            var textureData = new List<TextureData>();
            foreach (var child in textureXml.Elements()) {
                textureData.Add(new TextureData(child));
            }

            return textureData.ToArray();
        }

        private static Dictionary<int, TextureData> GetAltTextures(XElement objectXml) {
            var altTextures = new Dictionary<int, TextureData>();
            foreach (var textureXml in objectXml.Elements("AltTexture")) {
                altTextures[textureXml.ParseInt("@id")] = new TextureData(textureXml);
            }

            return altTextures;
        }
    }

    public sealed class AnimationsData {
        public List<AnimationData> Animations;
        public AnimationsData(XElement xml) {
            Animations = new();
            foreach (var animation in xml.Elements("Animation")) {
                Animations.Add(new AnimationData(animation));
            }
        }
    }

    public sealed class AnimationData {
        public int Period;
        public int PeriodJitter;
        public float Probablity;
        public bool Sync;
        public List<FrameData> Frames;
        public AnimationData(XElement xml) {
            Frames = new List<FrameData>();

            Probablity = xml.ParseFloat("@prob", 1);
            Period = (int)xml.ParseFloat("@period", 0) * 1000;
            PeriodJitter = (int)xml.ParseFloat("@periodJitter", 0) * 1000;
            Sync = xml.ParseString("@sync", "true") == "true";

            foreach (var frame in xml.Elements("Frame")) {
                Frames.Add(new FrameData(frame));
            }

            if (Period <= 0) {
                var amount = 0;
                foreach (var frame in Frames) {
                    amount += frame.Time;
                }
                Period = amount;
            }
        }

        private int GetPeriod() {
            if (PeriodJitter == 0) {
                return Period;
            }
            return Period - PeriodJitter + 2 * (int)(Random.value * PeriodJitter);
        }

        public int GetLastRun(int time) {
            if (Sync) {
                return time / Period * Period;
            }
            return (int)(time + GetPeriod() + 200 * Random.value);
        }

        public int GetNextRun(int time) {
            if (Sync) {
                return (int)(time / Period) * Period + Period;
            }
            return time + GetPeriod();
        }
    }
    public sealed class FrameData {
        public int Time;
        public TextureData TextureData;
        public FrameData(XElement xml) {
            Time = (int)(xml.ParseFloat("@time", 0.3f) * 1000);
            TextureData = new TextureData(xml);
        }
    }
}
