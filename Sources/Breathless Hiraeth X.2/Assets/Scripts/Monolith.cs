using System;
using System.Collections.Generic;

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
            public AudioSource pressureRumble;
            public AudioSource pressureLFE;
            public AudioSource pressureTinnitus;
            public UIDocument uiDocument;
            public StyleSheet uiStyle;
            public AudioClip uiClick;
            public AudioClip uiError;
            public AudioClip memorySparkle1;
            public AudioClip memorySparkle2;
            public AudioClip upgrade1;
            public AudioClip upgrade2;
            public AudioClip songStart;
            public AudioClip songEnd;
            public AudioClip songCredits;
            public AudioClip song1;
            public AudioClip song2;
            public AudioClip song3;
            public AudioClip ambiance;
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

        public static Fairy fairy;
        public static Encounter[] encounters;

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
                Refs.pressureRumble.volume = Mathf.Lerp(0, 0.75f, factor);
                Refs.pressureLFE.volume = Mathf.Lerp(0, 0.75f, factor);
                Refs.pressureTinnitus.volume = Mathf.Lerp(0, 0.1f, factor);

                UI.Hud.Instance.SetPressure(Instance.pressure);
            }
        }
        public static float PressureScale => fairy.PressureScale;
        public static int PressureStage => (Pressure < 30) ? 0 : (Pressure < 60) ? 1 : (Pressure < 90) ? 2 : 3;

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

            encounters = new Encounter[0];
            Player.Enable(false);

            UI.Hud.Show();
            UI.Menu.Hide();
            UI.Settings.Hide();
            UI.Overlay.Show();
            UI.Splash.Show();

            UI.Root.Instance.Add(UI.Overlay.Instance);
        }

        private void Update()
        {
            if (Inputs.Escape.Down)
            {
                if (UI.Root.Layer == null) UI.Menu.Show();
                else UI.Root.Layer.Hide();
            }

            if ((Inputs.ScrollUp || Inputs.ZoomIn.Down) && UI.Root.Layer == null) Settings.zoom.Set(Settings.zoom + 1);
            if ((Inputs.ScrollDown || Inputs.ZoomOut.Down) && UI.Root.Layer == null) Settings.zoom.Set(Settings.zoom - 1);

            if (Inputs.RotateCamera.Down && UI.Root.Layer == null) lastMouseX = Inputs.MouseX;
            if (Inputs.RotateCamera.Held && UI.Root.Layer == null)
            {
                Game.Camera.Rotation += (Inputs.MouseX - lastMouseX) / 2;
                lastMouseX = Inputs.MouseX;
            }

            switch (PressureStage)
            {
                default: break;
                case 0: Pressure = Mathf.Max(0, Pressure - Time.deltaTime); break;
                case 1: Pressure = Mathf.Max(30, Pressure - Time.deltaTime); break;
                case 2: Pressure = Mathf.Max(60, Pressure - Time.deltaTime); break;
                case 3: Pressure = Mathf.Max(90, Pressure - Time.deltaTime); break;
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

        public static void ScanEncounters()
        {
            GameObject root = GameObject.FindWithTag("Encounters");
            if (root == null)
            {
                ConsoleUtilities.Warn($"No encounters found");
                encounters = new Encounter[0];
                return;
            }

            List<Encounter> list = new List<Encounter>();
            for (int i = 0; i < root.transform.childCount; i++)
                try
                {
                    Encounter encounter = root.transform.GetChild(i).GetComponent<Encounter>();
                    list.Add(encounter);
                    encounter.Restart();
                }
                catch (Exception exception) { exception.Error($"Failed finding encounter in child"); }

            encounters = list.ToArray();
        }
        public static void ScanMemories()
        {
            GameObject root = GameObject.FindWithTag("Memories");
            if (root == null) { ConsoleUtilities.Warn($"No memories found"); return; }

            for (int i = 0; i < root.transform.childCount; i++)
                try
                {
                    GameObject gameObject = root.transform.GetChild(i).gameObject;
                    gameObject.SetActive(!Progress.guids.Contains(gameObject.name));
                    if (!gameObject.activeSelf) continue;
                    AudioSource audio = gameObject.GetComponent<AudioSource>();
                    audio.clip = (RNG.Generic.Bool()) ? Refs.memorySparkle1 : Refs.memorySparkle2;
                    audio.Play();
                }
                catch (Exception exception) { exception.Error($"Failed finding encounter in child"); }
        }
        public static void InitializeScene()
        {
            fairy = GameObject.FindWithTag("Fairy")?.GetComponent<Fairy>();
            fairy.GoToPosition(Progress.position);

            ScanEncounters();
            ScanMemories();

            Pressure = 0;
            Game.Camera.ColorAdjustments.saturation.value = 0;
            UI.Hud.Instance.Banner(fairy.CurrentScene);

            if (!Audio.Ambiance.global.isPlaying)
            {
                Audio.Ambiance.global.clip = Refs.ambiance;
                Audio.Ambiance.global.loop = true;
                Audio.Ambiance.global.Play();
            }

            if (!Audio.Music.global.isPlaying)
            {
                Audio.Music.global.clip = Refs.songStart;
                Audio.Music.global.Play();
            }
        }

        public static bool Load(string scene, bool respawn = false) => Load(scene, new Vector3(0, 100, 0), respawn);
        public static bool Load(string scene, Vector3 playerPosition, bool respawn = false)
        {
            if (!Application.CanStreamedLevelBeLoaded(scene))
            {
                ConsoleUtilities.Error($"Scene {scene:info} not found");
                return false;
            }
            else
            {
                LoadSequence(scene, playerPosition, respawn);
                return true;
            }
        }
        private static async void LoadSequence(string scene, Vector3 playerPosition, bool respawn = false)
        {
            ConsoleUtilities.Log($"Loading scene {scene:info}");
            Player.invincible = true;
            Instance.enabled = false;

            new Transition(() => Settings.masterVolume, value => Audio.Master.SetVolume((int)value), Settings.masterVolume, 0, "Master Volume").Curve(Function.Quadratic, Direction.In, 400).Start();
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

            Game.Camera.Rotation = 0;
            Game.Camera.ReleaseHijack();
            Player.Enable(true, playerPosition);
            Player.enabled = false;
            operation.allowSceneActivation = true;

            UI.Overlay.Instance.loading.RemoveFromClassList("show");
            await GeneralUtilities.DelayMS(1000);

            new Transition(() => 0, value => Audio.Master.SetVolume((int)value), 0, Settings.masterVolume, "Master Volume").Curve(Function.Quadratic, Direction.Out, 600).Start();
            UI.Overlay.Instance.Transition(VisualElementField.BackgroundColor, Unit.A, 1, 0).Curve(Function.Quadratic, Direction.Out, 600).Start();

            if (respawn) Respawn();
            else
            {
                Player.enabled = true;
                Instance.enabled = true;
                InitializeScene();
            }

            ConsoleUtilities.Log($"Loaded scene {scene:info}");
        }

        public static async void Respawn()
        {
            Player.enabled = false;
            Player.VisibleSword = false;
            Instance.enabled = false;
            InitializeScene();

#if UNITY_EDITOR
            await GeneralUtilities.DelayFrame(1);
#else
            Player.animator.Play("Wake Up");
            await GeneralUtilities.DelayMS(8000);
#endif

            Player.enabled = true;
            Player.VisibleSword = true;
            Instance.enabled = true;
        }
        public static void ResetSaves()
        {
            ConsoleUtilities.Log($"Resetting Saves");
            Progress.Defaults();
            Load(Progress.scene, true);
        }
    }
}