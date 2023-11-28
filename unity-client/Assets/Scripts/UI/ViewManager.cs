using UnityEngine;

namespace UI {
    public class ViewManager : MonoBehaviour {
        public static ViewManager Instance { get; private set; }

        [SerializeField] private MainScreenController MainScreen;
        [SerializeField] private CharacterScreenController CharacterScreen;
        [SerializeField] private NewCharacterController NewCharacterScreen;
        [SerializeField] private SkinScreenController SkinSelectScreen;
        [SerializeField] private GameScreenController GameScreen;
        [SerializeField] private DeathScreenController DeathScreen;

        private IScreen _activeScreen;
        private void Awake() {
            Instance = this;
            DontDestroyOnLoad(this);
            ChangeView(View.Menu, ignoreThisParam: true);
        }
        public static void ChangeView(View view, object data = null) {
            Instance.ChangeView(view, data);
        }
        private void ChangeView(View view, object data = null, bool ignoreThisParam = false) {
            IScreen newScreen = GetNewScreen(view);

            _activeScreen?.Hide();
            _activeScreen?.Object.SetActive(false);
            _activeScreen = newScreen;
            _activeScreen?.Object.SetActive(true);
            _activeScreen.Reset(data);
        }

        private IScreen GetNewScreen(View view) {
            IScreen newScreen = view switch {
                View.Menu => MainScreen,
                View.Character => CharacterScreen,
                View.NewCharacter => NewCharacterScreen,
                View.Skin => SkinSelectScreen,
                View.Game => GameScreen,
                View.Death => DeathScreen,
                _ => throw new System.Exception($"{view} not yet implemented"),
            };
            return newScreen;
        }
    }

    public enum View {
        Menu,
        Character,
        NewCharacter,
        Skin,
        Game,
        Death,
    }
}
