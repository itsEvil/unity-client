using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
