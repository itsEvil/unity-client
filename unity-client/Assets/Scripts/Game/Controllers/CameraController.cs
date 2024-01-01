using Game.Entities;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Controllers {
    internal class CameraController : MonoBehaviour {
        public static CameraController Instance { get; private set; }
        [SerializeField] private float Angle = 180.0f;
        [SerializeField] private float ZScale = 1;
        [SerializeField] private float ZOffset = -10;
        public Camera Camera { get; private set; }

        private GameObject _focus;

        private Player MyPlayer => Map.MyPlayer;
        private HashSet<Entity> _rotatingEntities;

        private void Awake() {
            Instance = this;
            Camera = Camera.main;
            _rotatingEntities = new HashSet<Entity>();
        }

        private void LateUpdate() {
            if (_focus == null)
                return;

            CheckForInputs();

            transform.rotation = Quaternion.Euler(0, 0, Settings.CameraAngle * Mathf.Rad2Deg);
            if (_focus != null) {
                //var yOffset = 0 * Camera.orthographicSize / 6f;
                transform.position = new Vector3(_focus.transform.position.x, _focus.transform.position.y, ZOffset);
                transform.Translate(0, 0.5f, 0, Space.Self);
            }

            Camera.orthographic = true;
            var orthoHeight = Camera.orthographicSize;
            var orthoWidth = Camera.aspect * orthoHeight;
            var m = Matrix4x4.Ortho(-orthoWidth, orthoWidth, -orthoHeight, orthoHeight, Camera.nearClipPlane, Camera.farClipPlane);
            var s = ZScale / orthoHeight;
            m[0, 2] = +s * Mathf.Sin(Mathf.Deg2Rad * -Angle);
            m[1, 2] = -s * Mathf.Cos(Mathf.Deg2Rad * -Angle);
            m[0, 3] = -ZOffset * m[0, 2];
            m[1, 3] = -ZOffset * m[1, 2];

            Camera.projectionMatrix = m;
            Camera.transparencySortMode = TransparencySortMode.CustomAxis;
            Camera.transparencySortAxis = transform.up;

            foreach (var entity in _rotatingEntities) {
                if (Map.TokenSource.IsCancellationRequested)
                    return;
                //entity.Rotation = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;

                entity.Transform.rotation = Quaternion.LookRotation(Camera.main.transform.up, -Camera.main.transform.forward);
            }
        }

        public float GetCurrentRotation() {
            return transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        }

        private void CheckForInputs() {
            //If movement is disabled so are all inputs
            if (MyPlayer == null || !PlayerInputController.InputEnabled)
                return;

            if (Input.GetKeyDown(Settings.ResetRotation))
                Settings.CameraAngle = 180 * Mathf.Deg2Rad;
        }

        public void SetFocus(GameObject focus) {
            _focus = focus;
        }

        public void AddRotatingEntity(Entity entity) {
            _rotatingEntities.Add(entity);
        }

        public void Clear() {
            _rotatingEntities.Clear();
        }

        public void RemoveRotatingEntity(Entity entity) {
            _rotatingEntities.Remove(entity);
        }
    }
}
