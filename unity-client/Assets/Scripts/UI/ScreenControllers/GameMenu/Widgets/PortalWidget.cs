using Game.Entities;
using Networking;
using Networking.Tcp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class PortalWidget : MonoBehaviour {
        [SerializeField] private TMP_Text PortalText;
        [SerializeField] private Button EntryButton;
        private Interactive Parent;
        public void Init(Interactive entity) {
            Parent = entity;
            PortalText.text = entity.Name;
        }
        public void OnEnable() {
            EntryButton.onClick.AddListener(OnEntry);
        }
        public void OnDisable() {
            EntryButton.onClick.RemoveAllListeners();
        }
        private void OnEntry() {
            TcpTicker.Send(new UsePortal(Parent.Id));
        }
    }

}