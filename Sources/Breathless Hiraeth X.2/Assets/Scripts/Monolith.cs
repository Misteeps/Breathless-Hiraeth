using System;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

using Simplex;


namespace Game
{
    public class Monolith : MonoBehaviour
    {
        #region References
        [Serializable]
        public class References
        {
            public UniversalRenderPipelineAsset quality;
            public VolumeProfile volumeProfile;
            public AudioMixer audioMixer;
            public UIDocument uiDocument;
            public StyleSheet uiStyle;
        }
        #endregion References


        [SerializeField] private References refs;
        public static References Refs => Instance.refs;

        public static GameObject InstanceObject { get; private set; }
        public static Monolith Instance { get; private set; }

        public static GameObject CameraObject { get; private set; }
        public static UnityEngine.Camera Camera { get; private set; }

        public static GameObject PlayerObject { get; private set; }
        public static Player Player { get; private set; }

        private float pressure;
        public static float Pressure
        {
            get => Instance.pressure;
            set
            {
                Instance.pressure = Mathf.Clamp(value, 0, 100);
                float factor = Mathf.InverseLerp(20, 100, Pressure);

                Game.Camera.FOV = Mathf.Lerp(90, 60, factor);
                Game.Camera.Shake = (Settings.cameraShake) ? Mathf.Lerp(0, 4, factor) : 0;
                Game.Camera.Vignette.intensity.value = Mathf.Lerp(0.2f, 0.7f, factor);
                Game.Camera.ChromaticAberration.intensity.value = factor;
                Game.Camera.ShadowsMidtonesHighlights.shadows.value = new Vector4(1, Mathf.Lerp(1, 0.4f, factor), Mathf.Lerp(1, 0.4f, factor), 0);

                UI.Hud.Instance.SetPressure(Instance.pressure);
            }
        }

        private static float lastMouseX;


        private void Awake()
        {
#if UNITY_EDITOR
            // References validator
            int nullRefs = 0;
            Validate(refs);

            if (nullRefs > 0)
            {
                ConsoleUtilities.Error($"Missing {nullRefs:info} references");
                UnityEditor.EditorApplication.isPlaying = false;
            }

            void Validate<T>(T references) where T : class
            {
                foreach (var field in references.GetType().GetFields())
                {
                    object value = field.GetValue(references);
                    if (value == null || value.ToString() == "null")
                    {
                        ConsoleUtilities.Warn($"Reference {field.FieldType:type} {field.Name:info} missing");
                        nullRefs += 1;
                    }
                }
            }
#endif

            InstanceObject = GameObject.FindWithTag("GameController");
            Instance = InstanceObject.GetComponent<Monolith>();
            DontDestroyOnLoad(InstanceObject);

            CameraObject = GameObject.FindWithTag("MainCamera");
            Camera = CameraObject.GetComponent<UnityEngine.Camera>();
            DontDestroyOnLoad(CameraObject);

            PlayerObject = GameObject.FindWithTag("Player");
            Player = PlayerObject.GetComponent<Player>();
            DontDestroyOnLoad(PlayerObject);
        }
        private void Start()
        {
            Progress.Load();
            Settings.Load();

            UI.Hud.Show();
            UI.Menu.Hide();
            UI.Settings.Hide();
            UI.Overlay.Show();
            UI.Splash.Show();

            UI.Root.Instance.Add(UI.Overlay.Instance);
            Pressure = 0;
        }

        private void Update()
        {
            if ((Inputs.ScrollUp || Inputs.ZoomIn.Down) && UI.Root.Layer == null) Settings.zoom.Set(Settings.zoom + 1);
            if ((Inputs.ScrollDown || Inputs.ZoomOut.Down) && UI.Root.Layer == null) Settings.zoom.Set(Settings.zoom - 1);

            if (Inputs.RotateCamera.Down && UI.Root.Layer == null) lastMouseX = Inputs.MouseX;
            if (Inputs.RotateCamera.Held && UI.Root.Layer == null)
            {
                Game.Camera.Rotation += (Inputs.MouseX - lastMouseX) / 2;
                lastMouseX = Inputs.MouseX;
            }

            if (Inputs.Escape.Down)
            {
                if (UI.Root.Layer == null) UI.Menu.Show();
                else UI.Root.Layer.Hide();
            }

            // Debug
            if (Input.GetKeyDown(KeyCode.Keypad0)) Pressure = 0;
            if (Input.GetKeyDown(KeyCode.Keypad1)) Pressure = 10;
            if (Input.GetKeyDown(KeyCode.Keypad2)) Pressure = 20;
            if (Input.GetKeyDown(KeyCode.Keypad3)) Pressure = 30;
            if (Input.GetKeyDown(KeyCode.Keypad4)) Pressure = 40;
            if (Input.GetKeyDown(KeyCode.Keypad5)) Pressure = 50;
            if (Input.GetKeyDown(KeyCode.Keypad6)) Pressure = 60;
            if (Input.GetKeyDown(KeyCode.Keypad7)) Pressure = 70;
            if (Input.GetKeyDown(KeyCode.Keypad8)) Pressure = 80;
            if (Input.GetKeyDown(KeyCode.Keypad9)) Pressure = 90;
            if (Input.GetKeyDown(KeyCode.KeypadPlus)) Pressure = 100;
            if (Input.GetKeyDown(KeyCode.KeypadMinus)) Player.speedModifier = Mathf.Clamp(Player.speedModifier - 1, 1, 20);
            if (Input.GetKeyDown(KeyCode.KeypadMultiply)) Player.speedModifier = Mathf.Clamp(Player.speedModifier + 1, 1, 20);
        }

        public static async Task Load(string scene) => await Load(scene, new Vector3(0, 100, 0));
        public static async Task Load(string scene, Vector3 playerPosition)
        {
            ConsoleUtilities.Log($"Loading scene {scene:info}");

            UI.Overlay.Instance.Transition(VisualElementField.BackgroundColor, Unit.A, 0, 1).Curve(Function.Quadratic, Direction.In, 400).Start();
            UI.Overlay.Instance.loadingBar.style.width = new UnityEngine.UIElements.Length(0, LengthUnit.Percent);
            UI.Overlay.Instance.loading.AddToClassList("show");

            await GeneralUtilities.DelayMS(440);
            Player.Enable(false, Vector3.zero);

            AsyncOperation operation = SceneManager.LoadSceneAsync(scene, new LoadSceneParameters(LoadSceneMode.Single, LocalPhysicsMode.None));
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                UI.Overlay.Instance.loadingBar.style.width = new UnityEngine.UIElements.Length(operation.progress * 100, LengthUnit.Percent);
                await GeneralUtilities.DelayMS(10);
            }

            UI.Overlay.Instance.loadingBar.style.width = new UnityEngine.UIElements.Length(operation.progress * 100, LengthUnit.Percent);
            await GeneralUtilities.DelayMS(400);

            UI.Overlay.Instance.Transition(VisualElementField.BackgroundColor, Unit.A, 1, 0).Curve(Function.Quadratic, Direction.Out, 600).Start();
            UI.Overlay.Instance.loadingBar.style.width = new UnityEngine.UIElements.Length(90, LengthUnit.Percent);
            UI.Overlay.Instance.loading.RemoveFromClassList("show");

            Game.Camera.Rotation = 0;
            Game.Camera.ReleaseHijack();
            Player.Enable(true, playerPosition);
            operation.allowSceneActivation = true;

            ConsoleUtilities.Log($"Loaded scene {scene:info}");
        }
    }
}