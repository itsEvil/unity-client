﻿using Static;
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
        public short X;
        public short Y;

        public TileDesc Desc;
        public void Init(TileDesc desc, short x, short y) {
            X = x;
            Y = y;
            Desc = desc;
            Type = desc.Type;

            if(desc.Sinking){
              SinkLevel = 12;
            }

            sprite = desc.TextureData.GetTexture(Hash(x, y));
        }
        private static int Hash(int x, int y) {
            var l = LookUp[(x + y) % LookUp.Length];
            var val = (x << 16 | y) ^ 81397550L;
            val = val * l % 65535;
            return (int)val;
        }
    }
}