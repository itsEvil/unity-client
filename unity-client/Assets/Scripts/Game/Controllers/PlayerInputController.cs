using Networking;
using Networking.Tcp;
using UI;
using UnityEngine;

namespace Game.Controllers {
    public class PlayerInputController {
        public static bool InputEnabled = true;
        public void Tick() {
            if (Input.GetKeyDown(KeyCode.G)) {
                TcpTicker.Send(new PlayerText("/spawn 1 Abyss of demons Portal"));
            }

            if (Input.GetKeyDown(Settings.EscapeToNexus)) {
                TcpTicker.Send(new Escape());
            }

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
