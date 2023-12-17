using Data;
using UnityEngine;

namespace Game {
    public partial class Map : MonoBehaviour {
        private void SpawnTilesTest() {
            short x = 0;
            short y = 0;
            Utils.Log("Map::TileTest::{0}", AssetLibrary.Type2TileDesc.Count);
            foreach(var (id, desc) in AssetLibrary.Type2TileDesc) {
                x++;
                if(x > 10) {
                    y++;
                    x = 0;
                }

                Utils.Log("Map::TileTest::SpawningTile::{0}::At-X:{1}-Y:{2}", desc.Id, x,y);

                var tile = ScriptableObject.CreateInstance<Square>();
                tile.Init(desc, x, y);
                _tileMap.SetTile(new Vector3Int(x,y), tile);
            }
        }

        private void SpawnEntityTest() {
            foreach (var (id, desc) in AssetLibrary.Type2ObjectDesc) {
                Utils.Log("Spawning in: {0}", desc.DisplayId);
                var entity = EntityPool.Get(desc.ObjectClass);
                entity.Init(desc);
                AddEntity(entity);
            }
        }
    }
}
