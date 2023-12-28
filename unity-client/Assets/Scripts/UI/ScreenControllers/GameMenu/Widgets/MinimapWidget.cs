using Game;
using Networking.Tcp;
using Static;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class MinimapWidget : MonoBehaviour {
        [SerializeField] private Image _playerIcon;
        [SerializeField] private MiniMapChunk _miniMapChunk;
        [SerializeField] private RectTransform _chunkLayer;
        
        //[SerializeField] private CameraController _mainCamera;

        private const int _MAP_CHUNK_SIZE = 256;

        private Map _map => Map.Instance;
        private MiniMapChunk[,] _chunks;

        private HashSet<MiniMapChunk> _needUpdateChunks;

        private List<float> _zoomLevels;
        private int _zoomIndex;

        public void Init() {
            _needUpdateChunks = new HashSet<MiniMapChunk>();
            foreach (Transform child in _chunkLayer) 
                Destroy(child.gameObject);

            var w = ((RectTransform)transform).sizeDelta.x;
            var h = ((RectTransform)transform).sizeDelta.y;

            _zoomIndex = 0;
            _zoomLevels = new List<float>();
            var maxZoom = Mathf.Max(w / Map.MapInfo.Width, h / Map.MapInfo.Height);
            _zoomLevels.Add(maxZoom);
            for (var zoom = 1f; zoom < 1 / maxZoom; zoom *= 2)
                _zoomLevels.Add(zoom);
            
            OnZoomChange();

            var xChunks = Convert(Map.MapInfo.Width - 1) + 1;
            var yChunks = Convert(Map.MapInfo.Height - 1) + 1;
            _chunks = new MiniMapChunk[xChunks, yChunks];

            var width = Map.MapInfo.Width;
            var height = Map.MapInfo.Height;
            for (var y = 0; y < xChunks; y++) {
                for (var x = 0; x < yChunks; x++) {
                    var chunkWidth = Mathf.Min(_MAP_CHUNK_SIZE, width);
                    var chunkHeight = Mathf.Min(_MAP_CHUNK_SIZE, height);
                    var texture = new Texture2D(chunkWidth, chunkHeight);
                    texture.filterMode = FilterMode.Point;
                    var pixelArray = new Color32[chunkWidth * chunkHeight];

                    for (var i = 0; i < pixelArray.Length; i++)
                        pixelArray[i] = Color.black;
                    
                    texture.SetPixels32(pixelArray);
                    texture.Apply();

                    var chunk = Instantiate(_miniMapChunk, _chunkLayer);
                    var sprite = Sprite.Create(texture, new Rect(0, 0, chunkWidth, chunkHeight), Vector2.zero, 1);
                    chunk.Sprite = sprite;
                    chunk.RectTransform.anchoredPosition = new(x * _MAP_CHUNK_SIZE, y * _MAP_CHUNK_SIZE);
                    chunk.RectTransform.sizeDelta = new(chunkWidth, chunkHeight);

                    _chunks[x, y] = chunk;
                    width -= _MAP_CHUNK_SIZE;
                }

                width = Map.MapInfo.Width;
                height -= _MAP_CHUNK_SIZE;
            }
        }
        public void Tick() {
            if (!Input.GetKey(KeyCode.LeftControl) && Input.mouseScrollDelta.y > 0) {
                if (_zoomIndex < _zoomLevels.Count - 1) {
                    _zoomIndex++;
                    OnZoomChange();
                }
            }

            if (!Input.GetKey(KeyCode.LeftControl) && Input.mouseScrollDelta.y < 0) {
                if (_zoomIndex > 0) {
                    _zoomIndex--;
                    OnZoomChange();
                }
            }

            if (!_playerIcon.gameObject.activeSelf)
                _playerIcon.gameObject.SetActive(true);

            //TODO
            //_playerIcon.transform.rotation = _mainCamera.Camera.transform.rotation;

            if (_zoomIndex == 0)
            {
                var arrowPos = Map.MyPlayer.Position;
                arrowPos *= _zoomLevels[_zoomIndex];
                arrowPos += new Vec2(-96, -96);
                _playerIcon.rectTransform.anchoredPosition = arrowPos.ToVector2();
                _chunkLayer.anchoredPosition = Vector2.zero;
                return;
            }

            _playerIcon.rectTransform.anchoredPosition = Vector2.zero;
            var pos = -Map.MyPlayer.Position;
            pos *= _zoomLevels[_zoomIndex];
            pos += new Vec2(96, -96);
            _chunkLayer.localPosition = pos.ToVector2();
        }
        private static int Convert(float value) => (int)Math.Floor(value / _MAP_CHUNK_SIZE);
        private void OnZoomChange() => _chunkLayer.localScale = Vector3.one * _zoomLevels[_zoomIndex];
    }
}