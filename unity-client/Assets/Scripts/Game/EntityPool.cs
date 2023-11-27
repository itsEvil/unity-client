using Static;
using System.Collections.Generic;
using UnityEngine;

namespace Game {
    public class EntityPool {
        private readonly Dictionary<GameObjectType, Queue<Entity>> _entityPool;
        private readonly Dictionary<GameObjectType, Entity> _entityPrefabs;
        private readonly Transform _wrapperParent;

        public EntityPool(Dictionary<GameObjectType, Entity> prefabs, Transform wrapperParent) {
            _entityPool = new Dictionary<GameObjectType, Queue<Entity>>();
            _entityPrefabs = prefabs;
            _wrapperParent = wrapperParent;
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

            //Debug.Log($"Loading entity type [{type}]");
            var entity = Object.Instantiate(_entityPrefabs[type], _wrapperParent);
            return entity;
        }

        public void Return(Entity entity) {
            if (entity == null) {
                Debug.LogWarning("CAN NOT RETURN NULL WRAPPER");
                return;
            }

            entity.Dispose();

            _entityPool[entity.Type].Enqueue(entity);
        }

        public void Clean() {
            foreach (var queue in _entityPool.Values) {
                queue.Clear();
            }
        }
    }
}
