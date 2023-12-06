using UnityEngine;
namespace UI {
    public class DeathScreenController : MonoBehaviour, IScreen {
        public GameObject Object { get => gameObject; }
        public void Reset(object data = null) {
            ViewManager.SetBackgroundVisiblity(true);

        }
        public void Hide()
        {

        }
    }
}
