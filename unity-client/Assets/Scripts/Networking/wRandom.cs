using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    public class wRandom
    {
        private uint _seed;
        private int _useAmount;
        public wRandom(uint seed) {
            _seed = seed;
        }
        public uint GetSeed() => _seed;
        public int GetUsage() => _useAmount;
        public void Drop(int count) {
            for (var i = 0; i < count; i++)
                Gen();
        }
        public uint NextIntRange(uint min, uint max) {
            return min == max ? min : min + Gen() % (max - min);
        }
        public double NextDouble() {
            return Gen() / 2147483647.0;
        }
        private uint Gen() {
            var lb = 16807 * (_seed & 0xFFFF);
            var hb = 16807 * (_seed >> 16);
            lb = lb + ((hb & 32767) << 16);
            lb = lb + (hb >> 15);
            if (lb > 2147483647) {
                lb = lb - 2147483647;
            }
            _useAmount++;

            return _seed = lb;
        }
    }
}
