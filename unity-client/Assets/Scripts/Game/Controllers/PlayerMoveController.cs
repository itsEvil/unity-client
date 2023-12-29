using Static;
using System;
using UI;
using UnityEngine;

namespace Game.Controllers {
    public class PlayerMoveController : IMoveController {
        private const float _MOVE_THRESHOLD = 0.4f;
        public Vec2 Direction { get; private set; }

        private readonly Player _player;
        private static Map _map => Map.Instance;
        public PlayerMoveController(Player player) {
            _player = player;
        }

        public void Tick(float deltaTime) {
            var rotate = 0;
            var xVelocity = 0;
            var yVelocity = 0;
            if (PlayerInputController.InputEnabled) {
                rotate = KeyToInt(Settings.RotateRight) - KeyToInt(Settings.RotateLeft);
                xVelocity = KeyToInt(Settings.MoveRight) - KeyToInt(Settings.MoveLeft);
                yVelocity = KeyToInt(Settings.MoveUp) - KeyToInt(Settings.MoveDown);
            }

            //Todo Add
            //if (_player.HasEffect(Confused))
            //{
            //    var temp = xVelocity;
            //    xVelocity = -yVelocity;
            //    yVelocity = -temp;
            //    rotate = -rotate;
            //}

            var cameraAngle = Settings.CameraAngle;
            if (rotate != 0) {
                cameraAngle += Time.deltaTime * Settings.PLAYER_ROTATE_SPEED * rotate;
                Settings.CameraAngle = cameraAngle;
            }

            var direction = Vec2.zero;
            if (xVelocity != 0 || yVelocity != 0) {
                var moveSpeed = _player.GetMovementSpeed();
                var moveVecAngle = Mathf.Atan2(yVelocity, xVelocity);
                direction.x = moveSpeed * Mathf.Cos(cameraAngle + moveVecAngle);
                direction.y = moveSpeed * Mathf.Sin(cameraAngle + moveVecAngle);
            }

            //if (_player.PushX != 0 || _player.PushY != 0)
            //{
            //    direction.x -= _player.PushX;
            //    direction.y -= _player.PushY;
            //}

            Direction = direction;
            ValidateAndMove(_player.Position + deltaTime * direction);
        }

        private void ValidateAndMove(Vec2 pos) {
            pos = ResolveNewLocation(pos);
            _player.MoveTo(pos);
        }

        private Vec2 ResolveNewLocation(Vec2 pos) {
            //Debug.Log($"Player has paralyze? {_player.HasConditionEffect(ConditionEffect.Paralyzed)}");

            var plrPos = _player.Position;

            var dx = pos.x - plrPos.x;
            var dy = pos.y - plrPos.y;

            if (dx < _MOVE_THRESHOLD &&
                dx > -_MOVE_THRESHOLD &&
                dy < _MOVE_THRESHOLD &&
                dy > -_MOVE_THRESHOLD) {
                return CalcNewLocation(pos);
            }

            var ds = _MOVE_THRESHOLD / Math.Max(Math.Abs(dx), Math.Abs(dy));
            var tds = 0f;

            pos = _player.Position;
            var done = false;
            while (!done) {
                if (tds + ds >= 1) {
                    ds = 1 - tds;
                    done = true;
                }

                pos = CalcNewLocation(new Vec2(pos.x + dx * ds, pos.y + dy * ds));
                tds += ds;
            }

            return pos;
        }

        private Vec2 CalcNewLocation(Vec2 pos) {
            var plrPos = _player.Position;

            var fx = 0f;
            var fy = 0f;

            var isFarX = plrPos.x % .5f == 0 && pos.x != plrPos.x || (int)(plrPos.x / .5f) != (int)(pos.x / .5f);
            var isFarY = plrPos.y % .5f == 0 && pos.y != plrPos.y || (int)(plrPos.y / .5f) != (int)(pos.y / .5f);

            if (!isFarX && !isFarY || _map.RegionUnblocked(pos.x, pos.y)) {
                return pos;
            }

            if (isFarX) {
                fx = pos.x > plrPos.x ? (int)(pos.x * 2) / 2f : (int)(plrPos.x * 2) / 2f;
                if ((int)fx > (int)plrPos.x)
                    fx -= 0.01f;
            }

            if (isFarY) {
                fy = pos.y > plrPos.y ? (int)(pos.y * 2) / 2f : (int)(plrPos.y * 2) / 2f;
                if ((int)fy > (int)plrPos.y)
                    fy -= 0.01f;
            }

            if (!isFarX) {
                pos.y = fy;
                return pos;
            }

            if (!isFarY) {
                pos.x = fx;
                return pos;
            }

            var ax = pos.x > plrPos.x ? pos.x - fx : fx - pos.x;
            var ay = pos.y > plrPos.y ? pos.y - fy : fy - pos.y;
            if (ax > ay) {
                if (_map.RegionUnblocked(pos.x, fy)) {
                    pos.y = fy;
                    return pos;
                }

                if (_map.RegionUnblocked(fx, pos.y)) {
                    pos.x = fx;
                    return pos;
                }
            }
            else {
                if (_map.RegionUnblocked(fx, pos.y)) {
                    pos.x = fx;
                    return pos;
                }

                if (_map.RegionUnblocked(pos.x, fy)) {
                    pos.y = fy;
                    return pos;
                }
            }

            pos.x = fx;
            pos.y = fy;
            return pos;
        }

        private int KeyToInt(KeyCode keyCode) {
            return Input.GetKey(keyCode) ? 1 : 0;
        }
    }
}
