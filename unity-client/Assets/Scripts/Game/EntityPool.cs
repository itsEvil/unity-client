using Static;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game {
    public class EntityPool : MonoBehaviour {
        [SerializeField] private Transform _wrapperParent;
        private Dictionary<GameObjectType, Queue<Entity>> _entityPool;
        private Dictionary<GameObjectType, Entity> _entityPrefabs;
        private void Awake() {
            DontDestroyOnLoad(this);
            var prefabs = new Dictionary<GameObjectType, Entity>();
            foreach (var entity in Resources.LoadAll<Entity>("Prefabs/Entities")) {
                Utils.Log("Loaded prefab: {0}", entity.name);
                prefabs[Enum.Parse<GameObjectType>(entity.name)] = entity;
            }
            _entityPool = new Dictionary<GameObjectType, Queue<Entity>>();
            _entityPrefabs = prefabs;
        }

        public Entity Get(GameObjectType type) {
            if (!_entityPrefabs.ContainsKey(type)) {
                Debug.LogWarning($"PREFAB NOT IMPLEMENTED [{type}]");
                return null;
            }

            if (!_entityPool.TryGetValue(type, out var queue))
                _entityPool[type] = queue = new Queue<Entity>();

            if (queue.Count > 0)
                return queue.Dequeue();

            //Debug.Log($"Instantiating object [{type}]");
            var entity = Instantiate(_entityPrefabs[type], _wrapperParent);
            return entity;
        }

        public void Return(Entity entity) {
            //Debug.Log($"Returning object [{entity.Type}] [{entity.Id}:{entity.Name}]");
            if (entity == null) {
                Debug.LogWarning("CAN NOT RETURN NULL WRAPPER");
                return;
            }

            entity.Dispose();

            _entityPool[entity.Type].Enqueue(entity);
        }

        public void Clear() {
            foreach (var queue in _entityPool.Values) {
                queue.Clear();
            }

            var childCount = _wrapperParent.childCount;
            for(int i = 0; i < childCount; i++)
                Destroy(_wrapperParent.GetChild(i).gameObject);
        }
    }
}
