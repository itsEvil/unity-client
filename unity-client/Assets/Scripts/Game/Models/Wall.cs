using Game.Entities;
using Static;
using System.Collections;
using System.Collections.Generic;
using Tests;
using UnityEngine;

namespace Game.Models {

    public class Wall : MonoBehaviour {
        [SerializeField] private SpriteRenderer[] Renderers; //Always length of 5
        public bool Satisfied = false;
        public void Init(ObjectDesc descriptor) {
            for (int i = 0; i < Renderers.Length; i++)
                Renderers[i].gameObject.SetActive(true);

            Renderers[0].sprite = descriptor.TopTextureData.GetTexture(0);
            Renderers[1].sprite = descriptor.TextureData.GetTexture(0);
            Renderers[2].sprite = descriptor.TextureData.GetTexture(0);
            Renderers[3].sprite = descriptor.TextureData.GetTexture(0);
            Renderers[4].sprite = descriptor.TextureData.GetTexture(0);
        }
        public void CheckNearbyWalls(Vec2 position) {
            if (Satisfied)
                return;

            Utils.Log("Checking nearby walls");
            var pos = new Vec2(position.x - 0.5f, position.y - 0.5f);
            Utils.Log("At position x:{0} y:{1}", pos.x, pos.y);

            var map = Map.Instance;

            var tileUp = map.GetTile((int)pos.x, (int)pos.y + 1);
            var tileDown = map.GetTile((int)pos.x, (int)pos.y - 1);
            var tileLeft = map.GetTile((int)pos.x - 1, (int)pos.y);
            var tileRight = map.GetTile((int)pos.x + 1, (int)pos.y);

            bool up = tileUp != null;
            bool down = tileDown != null;
            bool left = tileLeft != null;
            bool right = tileRight != null;

            if (tileUp != null && tileUp.StaticObject != null) {
                Renderers[3].gameObject.SetActive(false);
            }

            if (tileRight != null && tileRight.StaticObject != null) {
                Renderers[1].gameObject.SetActive(false);
            }

            if (tileLeft != null && tileLeft.StaticObject != null) {
                Renderers[2].gameObject.SetActive(false);
            }

            if (tileDown != null && tileDown.StaticObject != null) {
                Renderers[4].gameObject.SetActive(false);
            }

            if (up && down && left && right)
                Satisfied = true;
        }
        public void CheckNearbyWalls_Test(WallTester tester) {
            if(!Satisfied) {
                var map = tester;
                var pos = transform.position;

                var wallUp = map.GetWall((int)pos.x + 0, (int)pos.y + 1);
                var wallRight = map.GetWall((int)pos.x + 1, (int)pos.y);
                var wallLeft = map.GetWall((int)pos.x - 1, (int)pos.y);
                var wallDown = map.GetWall((int)pos.x + 0, (int)pos.y - 1);

                if(wallUp != null)
                    Renderers[3].gameObject.SetActive(false);

                if (wallRight != null)
                    Renderers[1].gameObject.SetActive(false);

                if (wallLeft != null)
                    Renderers[2].gameObject.SetActive(false);

                if (wallDown != null)
                    Renderers[4].gameObject.SetActive(false);

                Satisfied = true;
            }
        }
    }
}