using UnityEngine;

namespace UI
{
    public interface IScreen
    {
        public GameObject Object { get; }
        public void Reset(object data = null);
        public void Hide();
    }
}
