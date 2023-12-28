using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Static
{
    public sealed class CharacterAnimation
    {
        private readonly Dictionary<Facing, Dictionary<Action, Sprite[]>> _directionToAnimation = new();

        private readonly Facing[][] _sec2Dirs = new Facing[][] {
            new Facing[]{
                Facing.Left, Facing.Up, Facing.Down
            },
            new Facing[]{
                Facing.Up, Facing.Left, Facing.Down
            },
            new Facing[]{
                Facing.Up, Facing.Right, Facing.Down
            },
            new Facing[]{
                Facing.Right, Facing.Up, Facing.Down
            },
            new Facing[]{
                Facing.Right, Facing.Down
            },
            new Facing[]{
                Facing.Down, Facing.Right
            },
            new Facing[]{
                Facing.Down, Facing.Left
            },
            new Facing[]{
                Facing.Left, Facing.Down
            },
        };        

        public CharacterAnimation(List<Sprite> frames, Facing startFacing) {
            if (startFacing == Facing.Right) {
                _directionToAnimation[Facing.Right] = GetDirection(frames, 0, false);
                _directionToAnimation[Facing.Left] = GetDirection(frames, 0, true);
                if (frames.Count >= 14) {
                    _directionToAnimation[Facing.Down] = GetDirection(frames, 7, false);
                    if (frames.Count >= 21) {
                        _directionToAnimation[Facing.Up] = GetDirection(frames, 14, false);
                    }
                }
            }
            else
            {
                _directionToAnimation[Facing.Down] = GetDirection(frames, 0, false);
                if (frames.Count >= 14)
                {
                    _directionToAnimation[Facing.Right] = GetDirection(frames, 7, false);
                    _directionToAnimation[Facing.Left] = GetDirection(frames, 7, true);
                    if (frames.Count >= 21)
                    {
                        _directionToAnimation[Facing.Up] = GetDirection(frames, 14, false);
                    }
                }
            }
        }

        public Sprite ImageFromDir(Facing facing, Action action, int frame)
        {
            var frames = _directionToAnimation[facing][action];
            frame %= frames.Length;
            return frames[frame];
        }

        public Sprite ImageFromAngle(float angle, Action action, float p)
        {
            var sec = (int)(Mathf.Round(angle / (Mathf.PI / 4) + 4)) % 8;
            var dirs = _sec2Dirs[sec];
            if (!_directionToAnimation.TryGetValue(dirs[0], out var actions))
                if (!_directionToAnimation.TryGetValue(dirs[1], out actions))
                    actions = _directionToAnimation[dirs[2]];

            var images = actions[action];
            p = Mathf.Max(0, Mathf.Min(0.99999f, p));
            var i = (int)(p * images.Length);
            return images[i];
        }

        public Sprite ImageFromFacing(float facing, Action action, float p)
        {
            var cameraAngle = FPMathUtils.BoundToPI(facing - Settings.CameraAngle) * -1;
            return ImageFromAngle(cameraAngle, action, p);
        }

        private static Dictionary<Action, Sprite[]> GetDirection(List<Sprite> frames, int offset, bool mirror)
        {
            var ret = new Dictionary<Action, Sprite[]>();

            var stand = mirror ? frames[offset].Mirror() : frames[offset];
            var walk1 = mirror ? frames[offset + 1].Mirror() : frames[offset + 1];
            var walk2 = mirror ? frames[offset + 2].Mirror() : frames[offset + 2];
            var attack1 = mirror ? frames[offset + 4].Mirror() : frames[offset + 4];
            var attack2 = frames[offset + 5];

            var standAnim = new Sprite[]
            {
                stand
            };
            ret[Action.Stand] = standAnim;

            var walkAnim = new Sprite[]
            {
                walk1,
                walk2.IsTransparent() ? stand : walk2
            };
            ret[Action.Walk] = walkAnim;
            Sprite[] attacks = new Sprite[2];

            if (attack1.IsTransparent() && attack2.IsTransparent()) {
                attacks = walkAnim;
            }
            else {
                attacks[0] = attack1;

                //Todo Fix
                //if (!attackBit.IsTransparent()) {
                //    attack2 = SpriteUtils.MergeSprites(attack2, attackBit);
                //}

                attacks[1] = mirror ? attack2.Mirror() : attack2;
            }
            ret[Action.Attack] = attacks;

            return ret;
        }
    }
}
