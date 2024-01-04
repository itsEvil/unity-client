using Account;
using Game;
using Game.Controllers;
using Networking;
using Networking.Tcp;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI {
    public class ChatInput : MonoBehaviour {
        [SerializeField] private TMP_InputField InputField;
        private const int ChatCooldownMS = 200;
        private static int LastMessageTime;
        private bool IsSelected;
        public void Deselect() {
            PlayerInputController.InputEnabled = true;
            InputField.DeactivateInputField();
            IsSelected = false;
            EventSystem.current.SetSelectedGameObject(null);
        }

        public void Update() {
            if (Input.GetKeyDown(Settings.Menu)) {
                Deselect();
            }

            if (Input.GetKeyDown(Settings.EnterChat)) {
                if (IsSelected) {
                    OnSubmit(InputField.text);
                    Deselect();
                }
                else {
                    PlayerInputController.InputEnabled = false;
                    InputField.Select();
                    IsSelected = true;
                }
            }
        }

        public void OnSubmit(string message) {
            InputField.text = "";
            if (message.Length < 0)
                return;

            if (message.Length > 128) {
                SendSystemMessage("Message too long.");
                return;
            }

            var validText = Regex.Replace(message, @"[^a-zA-Z0-9`!@#$%^&* ()_+|\-=\\{}\[\]:"";'<>?,./]", "");
            if (validText.Length <= 0) {
                return;
            }

            if (LastMessageTime > GameTime.Time) {
                SendSystemMessage("Message sent too soon after previous one.");
                return;
            }

            LastMessageTime = GameTime.Time + ChatCooldownMS;

            if (validText[0] == '.') {
                ChatCommands.ParseChatCommand(validText);
                return;
            }

            if (AccountData.IsMuted) {
                SendSystemMessage("You are muted.");
                return;
            }

            TcpTicker.Send(new PlayerText(validText));
        }
        private void SendSystemMessage(string message) {
            ChatWidget.Instance.AddMessage(new Text("*System*", -1, -1, 0, "", message));
        }
    }
}
