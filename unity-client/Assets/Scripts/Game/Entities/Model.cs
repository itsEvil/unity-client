using Data;
using Static;
using UnityEngine;

namespace Game.Entities {
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class Model : Entity {
        [SerializeField] private MeshFilter MeshFilter;
        [SerializeField] private MeshRenderer MeshRenderer;
        public override void Init(ObjectDesc descriptor, ObjectDefinition definition) {
            Descriptor = descriptor;
            Name = Descriptor.DisplayId;
            Id = definition.ObjectStatus.Id;
            Type = GameObjectType.Model;
            if (MeshPreloader.Instance.TryGetMesh(descriptor.Type, out Mesh mesh))
                MeshFilter.sharedMesh = mesh;

            MeshRenderer.material = MeshPreloader.Instance.GetModelMaterial();
        }
    }
}
