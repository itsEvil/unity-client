using UI;
using UnityEngine;

namespace Game.Controllers {
    public class PlayerInputController {
        public static bool InputEnabled = true;
        public void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                ViewManager.ChangeView(View.Menu);
                return;
            }

            if (Input.GetKeyDown(Settings.Interact)) {
                Map.Instance.InteractWithNearby();
            }
        }
    }
}
