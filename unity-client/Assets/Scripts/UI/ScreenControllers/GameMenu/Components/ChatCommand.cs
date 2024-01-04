using System;
using UnityEngine.Events;

namespace UI {
    public class ChatCommand {
        public string Name;
        public string Description;
        public string UseCase;
        public int MandatoryLength;
        public UnityAction<string[]> Action;
        public ChatCommand(string name, string description, string useCase, int mandatoryLength, UnityAction<string[]> action) {
            Name = name;
            Description = description;
            UseCase = useCase;
            MandatoryLength = mandatoryLength;
            Action = action;
        }
        public override string ToString() {
            return Name + " - " + Description;
        }
    }
}
