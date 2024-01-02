using Game.Entities;
using Static;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Data {
    public class MeshPreloader : MonoBehaviour {
        public static MeshPreloader Instance { get; private set; }
        private static readonly Dictionary<ushort, Mesh> NameToMesh = new();
        [SerializeField] private Material Material;
        [SerializeField] private MeshFilter WallMeshFilter, TowerMeshFilter, PillarMeshFilter, BrokenPillarMeshFilter; //Example of a mesh which was loaded up via AssetLoader
        private void Awake() {
            Instance = this;
        }
        public void Start() {
            Material.SetTexture("_MainTex", SpriteAtlasCreator.GetAtlasTexture(0));

            foreach (var descriptor in AssetLibrary.ModelsToPreload)
                TryParseModel(descriptor);
        }
        public Material GetModelMaterial() => Material;
        /// <summary>
        /// Returns mesh for the ObjectType 
        /// </summary>
        public bool TryGetMesh(ushort objectType, out Mesh mesh) {
            if (NameToMesh.TryGetValue(objectType, out mesh)) 
                return true;

            mesh = null;
            return false;
        }
        private void TryParseModel(ObjectDesc descriptor) {
            switch (descriptor.ModelType) {
                case ModelType.BrokenPillar:
                case ModelType.Pillar:
                    ParseWallUVs(descriptor, PillarMeshFilter.sharedMesh); break;
                case ModelType.Tower:
                    ParseWallUVs(descriptor, TowerMeshFilter.sharedMesh); break;
                case ModelType.Wall: 
                    ParseWallUVs(descriptor, WallMeshFilter.sharedMesh); break;
                case ModelType.None:
                default: return;
            }
        }

        private void ParseWallUVs(ObjectDesc descriptor, Mesh meshToCopy) {
            Utils.Log("Parsing wall mesh for {0}", descriptor.Id);
            var meshCopy = MakeReadableMeshCopy(meshToCopy);
            var uv = meshCopy.uv;

            var sheetData = SpriteAtlasCreator.GetSheetData(descriptor.TextureData.TextureName);
            var topSheetData = sheetData;

            var sidePos = SpriteAtlasCreator.GetAtlasIndex(descriptor.TextureData.TextureName, descriptor.TextureData.Index);
            float sideX = sidePos.x + 2;
            float sideY = sidePos.y + 2;
            float topX = sideX;
            float topY = sideY;
            if (descriptor.TopTextureData != null) {
                var topPos = SpriteAtlasCreator.GetAtlasIndex(descriptor.TopTextureData.TextureName, descriptor.TopTextureData.Index);
                topSheetData = SpriteAtlasCreator.GetSheetData(descriptor.TopTextureData.TextureName);
                topX = topPos.x + 2;
                topY = topPos.y + 2;
            }


            var topUV = GetUVRectangleFromPixels(topX, topY, topSheetData.ImageWidth, topSheetData.ImageHeight, SpriteAtlasCreator.AtlasSize, SpriteAtlasCreator.AtlasSize);
            var upUV = GetUVRectangleFromPixels(sideX, sideY, sheetData.ImageWidth, sheetData.ImageHeight, SpriteAtlasCreator.AtlasSize, SpriteAtlasCreator.AtlasSize);
            var leftUV = GetUVRectangleFromPixels(sideX, sideY, sheetData.ImageWidth, sheetData.ImageHeight, SpriteAtlasCreator.AtlasSize, SpriteAtlasCreator.AtlasSize);
            var rightUV = GetUVRectangleFromPixels(sideX, sideY, sheetData.ImageWidth, sheetData.ImageHeight, SpriteAtlasCreator.AtlasSize, SpriteAtlasCreator.AtlasSize);
            var downUV = GetUVRectangleFromPixels(sideX, sideY, sheetData.ImageWidth, sheetData.ImageHeight, SpriteAtlasCreator.AtlasSize, SpriteAtlasCreator.AtlasSize);

            ApplyUVToUVArray(ref topUV, ref upUV, ref leftUV, ref rightUV, ref downUV, ref uv);
            meshCopy.uv = uv;
            NameToMesh[descriptor.Type] = meshCopy;
        }
        //Related to test only
        //private int counter = 0;
        //private void Update() {
        //    if (Input.GetKeyDown(KeyCode.Mouse0)) {
        //        var obj = Instantiate(ModelPrefab, transform);
        //        obj.SetMesh(Instantiate(NameToMesh["White Wall"]));
        //        obj.SetMaterial(Material);
        //        obj.transform.localScale = new Vector3(50, 50, 50);
        //        obj.transform.SetLocalPositionAndRotation(new Vector3(0, counter++), Quaternion.Euler(0, 180, 0));
        //    }
        //}
        private static Mesh MakeReadableMeshCopy(Mesh nonReadableMesh) {
            Mesh meshCopy = new Mesh();
            meshCopy.indexFormat = nonReadableMesh.indexFormat;

            // Handle vertices
            GraphicsBuffer verticesBuffer = nonReadableMesh.GetVertexBuffer(0);
            int totalSize = verticesBuffer.stride * verticesBuffer.count;
            byte[] data = new byte[totalSize];
            verticesBuffer.GetData(data);
            meshCopy.SetVertexBufferParams(nonReadableMesh.vertexCount, nonReadableMesh.GetVertexAttributes());
            meshCopy.SetVertexBufferData(data, 0, 0, totalSize);
            verticesBuffer.Release();

            // Handle triangles
            meshCopy.subMeshCount = nonReadableMesh.subMeshCount;
            GraphicsBuffer indexesBuffer = nonReadableMesh.GetIndexBuffer();
            int tot = indexesBuffer.stride * indexesBuffer.count;
            byte[] indexesData = new byte[tot];
            indexesBuffer.GetData(indexesData);
            meshCopy.SetIndexBufferParams(indexesBuffer.count, nonReadableMesh.indexFormat);
            meshCopy.SetIndexBufferData(indexesData, 0, 0, tot);
            indexesBuffer.Release();

            // Restore submesh structure
            uint currentIndexOffset = 0;
            for (int i = 0; i < meshCopy.subMeshCount; i++) {
                uint subMeshIndexCount = nonReadableMesh.GetIndexCount(i);
                meshCopy.SetSubMesh(i, new SubMeshDescriptor((int)currentIndexOffset, (int)subMeshIndexCount));
                currentIndexOffset += subMeshIndexCount;
            }

            // Recalculate normals and bounds
            meshCopy.RecalculateNormals();
            meshCopy.RecalculateBounds();

            return meshCopy;
        }
        private static Vector2 ConvertPixelsToUVCoords(float x, float y, int textureAtlasWidth, int textureAtlasHeight) {
            return new Vector2(x / textureAtlasWidth, y / textureAtlasHeight);
        }
        private static Vector2 ConvertPixelsToUVCoords(int x, int y, int textureAtlasWidth, int textureAtlasHeight) {
            return new Vector2((float)x / textureAtlasWidth, (float)y / textureAtlasHeight);
        }
        private static Vector2[] GetUVRectangleFromPixels(float x, float y, int width, int height, int textureWidth, int textureHeight) {
            return new Vector2[] {
                ConvertPixelsToUVCoords(x + width, y, textureWidth, textureHeight),
                ConvertPixelsToUVCoords(x, y, textureWidth, textureHeight),
                ConvertPixelsToUVCoords(x, y + height, textureWidth, textureHeight),
                ConvertPixelsToUVCoords(x + width, y + height, textureWidth, textureHeight),
            };
        }
        private static Vector2[] GetUVRectangleFromPixels(int x, int y, int width, int height, int textureWidth, int textureHeight) {
            return new Vector2[] {
                ConvertPixelsToUVCoords(x + width, y, textureWidth, textureHeight),
                ConvertPixelsToUVCoords(x, y, textureWidth, textureHeight),
                ConvertPixelsToUVCoords(x, y + height, textureWidth, textureHeight),
                ConvertPixelsToUVCoords(x + width, y + height, textureWidth, textureHeight),
            };
        }
        private static void ApplyUVToUVArray(ref Vector2[] top, ref Vector2[] upper, ref Vector2[] left, ref Vector2[] right, ref Vector2[] down, ref Vector2[] mainUV) {
            int length = top.Length + upper.Length + left.Length + right.Length + down.Length;
            if (mainUV.Length != length) {
                Debug.LogError($"Failed to set UV's. We set {length} uv's out of {mainUV.Length} the model requires");
                return;
            }

            mainUV[0] = top[0];
            mainUV[1] = top[1];
            mainUV[2] = top[2];
            mainUV[3] = top[3];

            mainUV[4] = upper[0];
            mainUV[5] = upper[1];
            mainUV[6] = upper[2];
            mainUV[7] = upper[3];

            mainUV[8] = left[0];
            mainUV[9] = left[1];
            mainUV[10] = left[2];
            mainUV[11] = left[3];

            mainUV[12] = right[0];
            mainUV[13] = right[1];
            mainUV[14] = right[2];
            mainUV[15] = right[3];

            mainUV[16] = down[0];
            mainUV[17] = down[1];
            mainUV[18] = down[2];
            mainUV[19] = down[3];
        }
    }
}