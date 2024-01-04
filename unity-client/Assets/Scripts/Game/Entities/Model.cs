using Data;
using Static;
using System;
using UnityEngine;

namespace Game.Entities {
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class Model : Entity {
        [SerializeField] private MeshFilter MeshFilter;
        [SerializeField] private MeshRenderer MeshRenderer;
        public override void Init(ObjectDesc descriptor, int id, Vec2 position, bool isMyPlayer = false) {
            Descriptor = descriptor;
            Name = Descriptor.DisplayId;
            Id = id;
            Type = GameObjectType.Model;
            if (MeshPreloader.Instance.TryGetMesh(descriptor.Type, out Mesh mesh))
                MeshFilter.sharedMesh = mesh;

            var pos = new Vector3(
                position.x, 
                position.y, 
                -0.5f);

            SetModelProperties(descriptor, pos);

            MeshRenderer.material = MeshPreloader.Instance.GetModelMaterial();
        }

        private void SetModelProperties(ObjectDesc descriptor, Vector3 position) {
            switch (descriptor.ModelType) {
                default:
                case ModelType.None: return;
                case ModelType.Wall: WallProperties(position); return;
                case ModelType.Tower: TowerProperties(position); return;
                case ModelType.BrokenPillar:
                case ModelType.Pillar: PillarProperties(position); return;
            }
        }

        private void PillarProperties(Vector3 position) {
            Transform.localScale = new Vector3(1, 1, 1);
            Transform.SetPositionAndRotation(position, Quaternion.Euler(0, 0, -90));
        }

        private void WallProperties(Vector3 position) {
            Transform.localScale = new Vector3(50, 50, 50);
            Transform.SetPositionAndRotation(position, Quaternion.Euler(0, 180, 0));

        }
        private void TowerProperties(Vector3 position) {
            position.z = 0;
            Transform.localScale = new Vector3(1, 1, 1);
            Transform.SetPositionAndRotation(position, Quaternion.Euler(0, 0, 0));
        }
    }
}
