using Static;
using UnityEngine;

namespace UI {
    public class SkinScreenController : MonoBehaviour, IScreen {
        public GameObject Object { get => gameObject; }
        public void Reset(object data = null) {
            ViewManager.SetBackgroundVisiblity(true);

            if (data == null) {
                Utils.Error("DataIsNull");
                ViewManager.ChangeView(View.Menu);
                return;
            }

            ViewManager.ChangeView(View.Game, (GameInitData)data);
        }
        public void Hide() {

        }
    }
}

