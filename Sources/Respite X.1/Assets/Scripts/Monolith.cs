using System;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Audio;
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
        public static Camera Camera { get; private set; }

        public static GameObject PlayerObject { get; private set; }
        //public static Player Player { get; private set; }


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
            Camera = CameraObject.GetComponent<Camera>();
            DontDestroyOnLoad(CameraObject);

            PlayerObject = GameObject.FindWithTag("Player");
            //Player = PlayerObject.GetComponent<Player>();
            DontDestroyOnLoad(PlayerObject);
        }
        private void Start()
        {
            Settings.Load();
        }

        public static async Task Load(string scene) => await Load(scene, new Vector3(0, 100, 0));
        public static async Task Load(string scene, Vector3 playerPosition)
        {
            ConsoleUtilities.Log($"Loading scene {scene:info}");

            AsyncOperation operation = SceneManager.LoadSceneAsync(scene, new LoadSceneParameters(LoadSceneMode.Single, LocalPhysicsMode.None));
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
                await GeneralUtilities.DelayMS(40);

            await GeneralUtilities.DelayMS(400);
            operation.allowSceneActivation = true;

            ConsoleUtilities.Log($"Loaded scene {scene:info}");
        }
    }
}