using Jobs;
using Networking;
using Networking.Tcp;
using Static;
using System;
using System.Collections.Generic;
using System.Globalization;
namespace Game.Entities {
    public sealed class Projectile : Entity {
        private float StartTime;
        private const float HITBOX_DISTANCE = 0.4f;
        public Entity Owner;
        public ProjectileDesc ProjectileDescriptor;
        public int BulletId;
        public float Angle;
        public Vec2 StartingPosition;
        public HashSet<int> Hits;
        public bool DamagesPlayers;

        public static Projectile Create(Entity owner, ProjectileDesc descriptor, int bulletId, float startTime, float angle,
            Vec2 startPos, int damage) {
            var proj = Map.Instance.EntityPool.Get(GameObjectType.Projectile) as Projectile;
            proj.Init(owner.Descriptor, bulletId, startPos, false);
            proj.SetValues(descriptor, owner, bulletId, angle, startTime, startPos, damage);
            return proj;
        }
        public override void Init(ObjectDesc descriptor, int id, Vec2 position, bool isMyPlayer = false) {
            base.Init(descriptor, id, position, isMyPlayer);
            Type = GameObjectType.Projectile;

            Hits ??= new();
            Hits.Clear();
        }

        public void SetValues(ProjectileDesc descriptor, Entity owner, int bulletId, float angle, float startTime, Vec2 startPos, int damage) {
            ProjectileDescriptor = descriptor;
            Owner = owner;
            BulletId = bulletId;
            Angle = angle;
            StartTime = startTime;
            StartingPosition = startPos;
            Position = startPos;
            DamagesPlayers = owner.Descriptor.Class != ObjectType.Player;
            Renderer.sprite = ProjectileDescriptor.TextureData.GetTexture(BulletId);
        }

        public bool CanHit(Entity en) {
            if (en.HasEffect(ConditionEffectIndex.Invincible) || en.HasEffect(ConditionEffectIndex.Stasis))
                return false;

            if (!Hits.Contains(en.Id)) {
                Hits.Add(en.Id);
                return true;
            }
            return false;
        }

        public Vec2 PositionAt(float elapsed, Vec2 p) {
            var speed = ProjectileDescriptor.Speed;
            if (ProjectileDescriptor.Accelerate) speed *= elapsed / ProjectileDescriptor.LifetimeMS;
            if (ProjectileDescriptor.Decelerate) speed *= 2 - elapsed / ProjectileDescriptor.LifetimeMS;
            var dist = elapsed * (speed / 10000f);
            var phase = Id % 2 == 0 ? 0 : MathF.PI;
            if (ProjectileDescriptor.Wavy)
            {
                var periodFactor = 6 * MathF.PI;
                var amplitudeFactor = MathF.PI / 64.0f;
                var theta = Angle + amplitudeFactor * MathF.Sin(phase + periodFactor * elapsed / 1000.0f);
                p.x = p.x + dist * MathF.Cos(theta);
                p.y = p.y + dist * MathF.Sin(theta);
            }
            else if (ProjectileDescriptor.Parametric)
            {
                var t = elapsed / ProjectileDescriptor.LifetimeMS * 2 * MathF.PI;
                var x = MathF.Sin(t) * (Id % 2 == 1 ? 1 : -1);
                var y = MathF.Sin(2 * t) * (Id % 4 < 2 ? 1 : -1);
                var sin = MathF.Sin(Angle);
                var cos = MathF.Cos(Angle);
                p.x = p.x + (x * cos - y * sin) * ProjectileDescriptor.Magnitude;
                p.y = p.y + (x * sin + y * cos) * ProjectileDescriptor.Magnitude;
            }
            else
            {
                if (ProjectileDescriptor.Boomerang) {
                    var halfway = ProjectileDescriptor.LifetimeMS * (ProjectileDescriptor.Speed / 10000) / 2;
                    if (dist > halfway)
                        dist = halfway - (dist - halfway);
                }
                p.x = p.x + dist * MathF.Cos(Angle);
                p.y = p.y + dist * MathF.Sin(Angle);
                if (ProjectileDescriptor.Amplitude != 0) {
                    var deflection = ProjectileDescriptor.Amplitude * MathF.Sin(phase + elapsed / ProjectileDescriptor.LifetimeMS * ProjectileDescriptor.Frequency * 2 * MathF.PI);
                    p.x = p.x + deflection * MathF.Cos(Angle + MathF.PI / 2);
                    p.y = p.y + deflection * MathF.Sin(Angle + MathF.PI / 2);
                }
            }

            return p;
        }
        public override bool MoveTo(Vec2 position) {
            var square = Map.Instance.GetTile((int)position.x, (int)position.y);
            if (square == null)
                return false;

            Position = position;
            Square = square;
            return true;
        }
        public override bool Tick() {
            var elapsed = GameTime.Time - StartTime;
            if(elapsed > ProjectileDescriptor.LifetimeMS)
                return false;

            if (!MoveTo(PositionAt(elapsed, StartingPosition)) || Square.Type == 255) {
                if (DamagesPlayers)
                    TcpTicker.Send(new SquareHit(GameTime.Time, BulletId));
                else if (Square.StaticObject != null) {
                    //Particles hit effect
                }

                return false;
            }

            if (Square.StaticObject != null && (!Square.StaticObject.Descriptor.Enemy || DamagesPlayers) &&
                (Square.StaticObject.Descriptor.EnemyOccupySquare || !ProjectileDescriptor.PassesCover && Square.StaticObject.Descriptor.OccupySquare)) {
                if (DamagesPlayers)
                    TcpTicker.Send(new SquareHit(GameTime.Time, BulletId));
                else {
                    //TODO square hit effect
                }
                return false;
            }

            Dictionary<int, Entity> dict = DamagesPlayers ? Map.Players : Map.Enemies;

            using var hitJob = new ProjectileHitJob(dict, Transform, HITBOX_DISTANCE);
            var result = hitJob.Execute();
            if (result.Closest == null)
                return false;
            
            var target = result.Closest;
            Utils.Log("Closest target is: {0} at {1}", target.Descriptor.DisplayId, result.Distance);
            if (CanHit(target)) {
                var dmg = 0;//ProjectileDescriptor.Damage;
                target.Damage(dmg, Array.Empty<ConditionEffectDesc>(), this);

                if (target.Id == Map.MyPlayer.Id)
                    TcpTicker.Send(new PlayerHit(BulletId));
                else if (target.Descriptor.Enemy)
                    TcpTicker.Send(new EnemyHit(GameTime.Time, BulletId, target.Id));

                if (ProjectileDescriptor.MultiHit) {
                    Hits.Add(target.Id);
                    return false;
                }
            }    

            return true;
        }
        public override void Dispose() {
            base.Dispose();
        }
    }
}