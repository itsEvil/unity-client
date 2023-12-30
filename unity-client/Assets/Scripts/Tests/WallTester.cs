using Data;
using Game.Models;
using UnityEngine;

namespace Tests {
    /// <summary>
    /// Initializes an array of Walls and Ticks them via FixedUpdate
    /// 
    /// When walls get ticked they check if nearby walls exist 
    /// (UP/DOWN/LEFT/RIGHT). 
    /// 
    /// If a wall exists the wall will remove 
    /// the corresponding sprite by de-activating its game object
    /// 
    /// This is a minor optimization but it will reduce 
    /// how many game objects get ticked if there is lots of walls
    /// next to each other.
    /// 
    /// On the real map script Walls will use GetTile(x,y); and get the 
    /// StaticObject reference from that tile.
    /// 
    /// This will only be called when new tiles have been acquired 
    /// and will not use any Unity method calls.
    /// 
    /// </summary>

    public class WallTester : MonoBehaviour {
        [SerializeField] private Wall[] Walls;
        private void Start() {
            var whiteWallDescriptor = AssetLibrary.GetObjectDesc("White Wall");
            for(int i = 0; i <  Walls.Length; i++) {
                Wall w = Walls[i];
                w.Init(whiteWallDescriptor);
            }
        }
        public Wall GetWall(int x, int y) {
            foreach (var wall in Walls)
                if (wall.transform.position.x == x && wall.transform.position.y == y)
                    return wall;

            return null;
        }
        private void FixedUpdate() {
            foreach (var wall in Walls)
                wall.CheckNearbyWalls_Test(this);
        }
    }
}