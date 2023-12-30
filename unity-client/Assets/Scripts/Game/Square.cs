using Game.Entities;
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
        public StaticEntity StaticObject;
        public int X;
        public int Y;

        public TileDesc Descriptor;
        public void Init(TileDesc descriptor, int x, int y) {
            X = x;
            Y = y;
            Descriptor = descriptor;
            Type = descriptor.Type;

            if(descriptor.Sinking){
              SinkLevel = 12;
            }
            
            sprite = descriptor.TextureData.GetTexture(Hash(x,y));
            if (sprite == null)
                Utils.Error("Square {0} sprite is null!", descriptor.Id);
        }
        private static int Hash(int x, int y) {
            var l = LookUp[(x + y) % LookUp.Length];
            var val = (x << 16 | y) ^ 81397550L;
            val = val * l % 65535;
            return (int)val;
        }
    }
}
