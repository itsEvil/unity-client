using Static;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game
{
    public class Square : Tile
    {
        private static readonly int[] LookUp = new int[] {
            26171, 44789, 20333, 70429, 98257, 59393, 33961
        };

        public ushort Type;
        public int SinkLevel;
        public Entity StaticObject;
        public Vector3Int Position;
        public void Init(ushort type, int x, int y) {
            Position = new Vector3Int(x, y);
            Type = type;
            //var desc = AssetLibrary.Type2Tile[Type];
            //if(desc.Sink){
            //  SinkLevel = 12;
            //}

            //sprite = desc.TextureData.GetTexture(Hash(x, y));
        }
        private static int Hash(int x, int y) {
            var l = LookUp[(x + y) % LookUp.Length];
            var val = (x << 16 | y) ^ 81397550L;
            val = val * l % 65535;
            return (int)val;
        }
    }
}
