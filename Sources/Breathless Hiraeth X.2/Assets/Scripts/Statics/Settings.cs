using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering.Universal;

using Simplex;


namespace Game
{
    public static class Settings
    {
        #region Setting
        public abstract class Setting
        {
            public string Name { get; protected set; }
            public abstract void Default();
            public abstract void Set(string value, bool invoke = true);
        }
        public abstract class Setting<T> : Setting, IValue<T>
        {
            public T DefaultValue { get; protected set; }
            public T Value { get; protected set; }

            object IValue.Value { get => Value; set => Set((T)value); }
            T IValue<T>.Value { get => Value; set => Set(value); }

            public readonly Action<T> onSet;


            public Setting(string name, T defaultValue, Action<T> onSet = null)
            {
                this.Name = name;
                this.DefaultValue = defaultValue;
                this.onSet = onSet;
            }

            public override void Default() => Set(DefaultValue);
            public virtual void Set(T value, bool invoke = true)
            {
                try
                {
                    Value = value;
                    if (invoke) onSet?.Invoke(Value);
                }
                catch (Exception exception) { exception.Error(ConsoleUtilities.settingsTag, $"Failed applying {value:info} to {this:type}"); }
            }

            public override string ToString() => Value.ToString();
            public static implicit operator T(Setting<T> setting) => setting.Value;
        }
        #endregion Setting

        #region Bool
        public class Bool : Setting<bool>
        {
            public Bool(string name, bool defaultValue, Action<bool> onSet = null) : base(name, defaultValue, onSet) { }

            public override void Set(string value, bool invoke = true) => Set(bool.Parse(value), invoke);
        }
        #endregion Bool
        #region Int
        public class Int : Setting<int>
        {
            public readonly int min;
            public readonly int max;


            public Int(string name, int defaultValue, int min = int.MinValue, int max = int.MaxValue, Action<int> onSet = null) : base(name, defaultValue, onSet)
            {
                this.min = min;
                this.max = max;
            }

            public override void Set(string value, bool invoke = true) => Set(int.Parse(value), invoke);
            public override void Set(int value, bool invoke = true) => base.Set(Mathf.Clamp(value, min, max), invoke);
        }
        #endregion Int
        #region Float
        public class Float : Setting<float>
        {
            public readonly float min;
            public readonly float max;


            public Float(string name, float defaultValue, float min = float.MinValue, float max = float.MaxValue, Action<float> onSet = null) : base(name, defaultValue, onSet)
            {
                this.min = min;
                this.max = max;
            }

            public override void Set(string value, bool invoke = true) => Set(float.Parse(value), invoke);
            public override void Set(float value, bool invoke = true) => base.Set(Mathf.Clamp(value, min, max), invoke);
        }
        #endregion Float
        #region Choice
        public class Choice<T> : Setting<T>
        {
            public readonly string[] choices;
            public readonly T[] values;


            public Choice(string name, (string choice, T value)[] array, Action<T> onSet = null) : this(name, array[0].value, array, onSet) { }
            public Choice(string name, T defaultValue, (string choice, T value)[] array, Action<T> onSet = null) : base(name, defaultValue, onSet)
            {
                if (array.IsEmpty()) throw new ArgumentException("Empty or null choice/value array").Overwrite($"Failed constructing choice setting {typeof(T):type}");

                choices = new string[array.Length];
                values = new T[array.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    (string choice, T value) = array[i];
                    choices[i] = choice;
                    values[i] = value;
                }
            }

            public override void Set(string value, bool invoke = true) => Set(Find(value), invoke);

            public bool Contains(string choice) => Array.IndexOf(choices, choice) != -1;
            public T Find(string choice)
            {
                T value = values[0];

                if (!string.IsNullOrEmpty(choice))
                    try { value = values[Array.IndexOf(choices, choice)]; }
                    catch (Exception exception) { exception.Error(ConsoleUtilities.settingsTag, $"Failed finding value linked to {choice:info} in {typeof(T):type}"); return value; }

                return value;
            }

            public bool Contains(T value) => Array.IndexOf(values, value) != -1;
            public string Find(T value)
            {
                string choice = choices[0];

                try { choice = choices[Array.IndexOf(values, value)]; }
                catch (Exception exception) { exception.Error(ConsoleUtilities.settingsTag, $"Failed finding choice linked to {value:info} in {typeof(T):type}"); return choice; }

                return choice;
            }

            public override string ToString() => Find(Value);
        }
        #endregion Choice
        #region Color
        public class Color : Setting<UnityEngine.Color>
        {
            public Color(string name, UnityEngine.Color defaultValue, Action<UnityEngine.Color> onSet = null) : base(name, defaultValue, onSet) { }

            public override void Set(string value, bool invoke = true) => Set((ColorUtility.TryParseHtmlString($"#{value}", out UnityEngine.Color color)) ? color : throw new FormatException($"String was not recognized as a valid color hex."), invoke);

            public override string ToString() => ColorUtility.ToHtmlStringRGBA(Value);
        }
        #endregion Color
        #region Keybind
        public class Keybind : Setting<(KeyCode, KeyCode)>
        {
            public readonly bool lockPrimary;

            public KeyCode Primary => Value.Item1;
            public KeyCode Secondary => Value.Item2;

            public bool Up => UnityEngine.Input.GetKeyUp(Primary) || UnityEngine.Input.GetKeyUp(Secondary);
            public bool Down => UnityEngine.Input.GetKeyDown(Primary) || UnityEngine.Input.GetKeyDown(Secondary);
            public bool Held => UnityEngine.Input.GetKey(Primary) || UnityEngine.Input.GetKey(Secondary);


            public Keybind(string name, KeyCode defaultPrimary, KeyCode defaultSecondary = KeyCode.None, bool lockPrimary = false, Action<(KeyCode, KeyCode)> onSet = null) : base(name, (defaultPrimary, defaultSecondary), onSet)
            {
                this.lockPrimary = lockPrimary;
            }

            public override void Set(string value, bool invoke = true)
            {
                int split = value.IndexOf(',');
                KeyCode primary = (KeyCode)Enum.Parse(typeof(KeyCode), value[..split].Trim());
                KeyCode secondary = (KeyCode)Enum.Parse(typeof(KeyCode), value[(split + 1)..].Trim());
                Set((primary, secondary), invoke);
            }

            public override string ToString() => $"{Primary}, {Secondary}";
            public string Display()
            {
                if (Primary != KeyCode.None && Secondary != KeyCode.None) return $"[{Primary.KeycodeString()}] or [{Secondary.KeycodeString()}]";
                if (Primary != KeyCode.None) return $"[{Primary.KeycodeString()}]";
                if (Secondary != KeyCode.None) return $"[{Secondary.KeycodeString()}]";
                return "[<UNBOUND>]";
            }


            public override bool Equals(object obj) => (obj is Keybind keybind) && keybind.Primary == Primary && keybind.Secondary == Secondary;
            public override int GetHashCode() => HashCode.Combine(Primary, Secondary);
            public static bool Equals(Keybind keybind, KeyCode keycode) => keybind.Primary == keycode || keybind.Secondary == keycode;
            public static bool operator ==(Keybind keybind, KeyCode keycode) => Equals(keybind, keycode);
            public static bool operator !=(Keybind keybind, KeyCode keycode) => !Equals(keybind, keycode);
            public static bool operator ==(KeyCode keycode, Keybind keybind) => Equals(keybind, keycode);
            public static bool operator !=(KeyCode keycode, Keybind keybind) => !Equals(keybind, keycode);
        }
        #endregion Keybind


        private static (string, FullScreenMode)[] WindowModes
        {
            get
            {
#if UNITY_WEBGL
                return new (string, FullScreenMode)[] { ("Fullscreen", FullScreenMode.FullScreenWindow), ("Normal", FullScreenMode.Windowed), };
#elif UNITY_STANDALONE_OSX
                return new (string, FullScreenMode)[] { ("Fullscreen", FullScreenMode.ExclusiveFullScreen), ("Borderless Window", FullScreenMode.FullScreenWindow), ("Maximized Window", FullScreenMode.MaximizedWindow), ("Windowed", FullScreenMode.Windowed), };
#else
                return new (string, FullScreenMode)[] { ("Fullscreen", FullScreenMode.ExclusiveFullScreen), ("Borderless Window", FullScreenMode.FullScreenWindow), ("Windowed", FullScreenMode.Windowed), };
#endif
            }
        }
        private static (string, (int, int))[] Resolutions
        {
            get
            {
#if UNITY_WEBGL
                return new (string, (int, int))[] { ("WebGL", (0, 0)) };
#else
                List<Vector2Int> newResolutions = new List<Vector2Int>();
                foreach (Resolution res in Screen.resolutions)
                {
                    Vector2Int size = new Vector2Int(res.width, res.height);
                    if (!newResolutions.Contains(size))
                        newResolutions.Add(size);
                }

                return newResolutions
                .ConvertAll(res => ($"{res.x} x {res.y}", (res.x, res.y)))
                .OrderByDescending(res => res.Item2.x).ThenByDescending(res => res.Item2.y)
                .ToArray();
#endif
            }
        }
        private static (string, int)[] TextureQualities
        {
            get => new (string, int)[] { ("High", 0), ("Medium", 1), ("Low", 2) };
        }
        private static (string, int)[] AnisotropicFilterings
        {
            get => new (string, int)[] { ("16x", 16), ("8x", 8), ("4x", 4), ("2x", 2), ("Off", 0), };
        }
        private static (string, int)[] AntiAliases
        {
            get => new (string, int)[] { ("MSAA x8", 8), ("MSAA x4", 4), ("MSAA x2", 2), ("Off", 1) };
        }
        private static (string, SoftShadowQuality)[] ShadowQualities
        {
            get => new (string, SoftShadowQuality)[] { ("High", SoftShadowQuality.High), ("Medium", SoftShadowQuality.Medium), ("Low", SoftShadowQuality.Low) };
        }

#if UNITY_WEBGL
        private static int DefaultAnisotropicFiltering => 4;
        private static int DefaultAntiAliasing => 2;
        private static SoftShadowQuality DefaultShadowQuality => SoftShadowQuality.Low;
        private static int DefaultShadowDistance => 15;
#else
        private static int DefaultAnisotropicFiltering => 16;
        private static int DefaultAntiAliasing => 8;
        private static SoftShadowQuality DefaultShadowQuality => SoftShadowQuality.High;
        private static int DefaultShadowDistance => 20;
#endif

        [Header("General")]
        public static Bool combatZoom = new Bool("Combat Zoom", true, _ => Camera.ZoomOffset = 0);
        public static Bool abilityZoom = new Bool("Ability Zoom", true, _ => Camera.ZoomOffset = 0);
        public static Bool autoExitCombat = new Bool("Auto Exit Combat", true);
        public static Bool cameraShake = new Bool("Camera Shake", true, value => Monolith.Pressure = Monolith.Pressure);
        public static Int zoom = new Int("Zoom", 5, 0, 15, value => Camera.Zoom(value));
        public static Float hudScale = new Float("HUD Scale", 1, 0, 2, value => UI.Hud.Instance.Scale(value));

        [Header("Graphics")]
        public static Choice<FullScreenMode> windowMode = new Choice<FullScreenMode>("Window Mode", FullScreenMode.FullScreenWindow, WindowModes, _ => ApplyResolution());
        public static Choice<(int width, int height)> resolution = new Choice<(int width, int height)>("Resolution", Resolutions, _ => ApplyResolution());
        public static Int fpsLimit = new Int("FPS Limit", 361, 0, int.MaxValue, value => Application.targetFrameRate = (value == 361) ? 0 : value);
        public static Bool fpsCounter = new Bool("FPS Counter", false, value => UI.Overlay.Instance.fps.Display(value));
        public static Bool vSync = new Bool("V-Sync", true, value => QualitySettings.vSyncCount = (value) ? 1 : 0);
        public static Float renderScale = new Float("Render Scale", 1, 0.5f, 1.25f, value => Monolith.Refs.quality.renderScale = value);
        public static Float viewDistance = new Float("View Distance", 2, 0.5f, 4f, value => QualitySettings.lodBias = value);
        public static Choice<int> textureQuality = new Choice<int>("Texture Quality", TextureQualities, value => QualitySettings.globalTextureMipmapLimit = value);
        public static Choice<int> anisotropicFiltering = new Choice<int>("Anisotropic Filtering", DefaultAnisotropicFiltering, AnisotropicFilterings, value => Texture.SetGlobalAnisotropicFilteringLimits(-1, value));
        public static Choice<int> antiAliasing = new Choice<int>("Anti-aliasing", DefaultAntiAliasing, AntiAliases, value => Monolith.Refs.quality.msaaSampleCount = value);
        public static Choice<SoftShadowQuality> shadowQuality = new Choice<SoftShadowQuality>("Shadow Quality", DefaultShadowQuality, ShadowQualities, value => typeof(UniversalRenderPipelineAsset).GetProperty("softShadowQuality", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Monolith.Refs.quality, value));
        public static Int shadowDistance = new Int("Shadow Distance", DefaultShadowDistance, 5, 30, value => Monolith.Refs.quality.shadowDistance = value * 10);

        [Header("Audio")]
        public static Int masterVolume = new Int("Master Volume", 100, 0, 100, Audio.Master.SetVolume);
        public static Int uiVolume = new Int("UI Volume", 50, 0, 100, Audio.UI.SetVolume);
        public static Int sfxVolume = new Int("SFX Volume", 50, 0, 100, Audio.SFX.SetVolume);
        public static Int voiceVolume = new Int("Voice Volume", 50, 0, 100, Audio.Voice.SetVolume);
        public static Int ambianceVolume = new Int("Ambiance Volume", 50, 0, 100, Audio.Ambiance.SetVolume);
        public static Int musicVolume = new Int("Music Volume", 50, 0, 100, Audio.Music.SetVolume);

        [Header("Keybinds")]
        public static Keybind click = new Keybind("Click", KeyCode.Mouse0, KeyCode.None, true);
        public static Keybind escape = new Keybind("Escape", KeyCode.Escape, KeyCode.Mouse3, true);
        public static Keybind moveUp = new Keybind("Move Up", KeyCode.W, KeyCode.UpArrow);
        public static Keybind MoveDown = new Keybind("Move Down", KeyCode.S, KeyCode.DownArrow);
        public static Keybind moveLeft = new Keybind("Move Left", KeyCode.A, KeyCode.LeftArrow);
        public static Keybind moveRight = new Keybind("Move Right", KeyCode.D, KeyCode.RightArrow);
        public static Keybind sprint = new Keybind("Sprint", KeyCode.LeftShift);
        public static Keybind jump = new Keybind("Jump", KeyCode.Space);
        public static Keybind breath = new Keybind("Breath", KeyCode.F);
        public static Keybind attack = new Keybind("Attack", KeyCode.Mouse0);
        public static Keybind ability1 = new Keybind("Ability 1", KeyCode.Alpha1);
        public static Keybind ability2 = new Keybind("Ability 2", KeyCode.Alpha2);
        public static Keybind ability3 = new Keybind("Ability 3", KeyCode.Alpha3);
        public static Keybind ability4 = new Keybind("Ability 4", KeyCode.Alpha4);
        public static Keybind cancelAbility = new Keybind("Cancel", KeyCode.Mouse1);
        public static Keybind zoomIn = new Keybind("Zoom In", (KeyCode)541, KeyCode.Equals, true);
        public static Keybind zoomOut = new Keybind("Zoom Out", (KeyCode)542, KeyCode.Minus, true);
        public static Keybind rotateCamera = new Keybind("Rotate Camera", KeyCode.Mouse2);

        public static (FieldInfo, Setting)[] SettingFields => typeof(Settings).GetFields().Where(field => field.GetCustomAttribute<NonSerializedAttribute>() == null && field.GetValue(null) is Setting).Select(field => (field, field.GetValue(null) as Setting)).ToArray();


        public static void Defaults(string category)
        {
            bool inCategory = category == null;

            foreach ((FieldInfo field, Setting setting) in SettingFields)
                try
                {
                    HeaderAttribute attribute = field.GetCustomAttribute<HeaderAttribute>();
                    if (attribute != null) inCategory = category == null || category == attribute.header;
                    if (inCategory) setting.Default();
                }
                catch (Exception exception) { exception.Error(ConsoleUtilities.settingsTag, $"Failed defaulting {field?.Name:info}"); }
        }
        public static void Load()
        {
            if (FileUtilities.Load("Settings.ini", out FileUtilities.INI ini))
                foreach ((FieldInfo field, Setting setting) in SettingFields)
                    try
                    {
                        FileUtilities.INI.Item item = ini.FindItem(setting.Name);
                        if (item == null) setting.Default();
                        else setting.Set((string)item.value);
                    }
                    catch (Exception exception) { exception.Error(ConsoleUtilities.settingsTag, $"Failed reading {field?.Name:info}"); }
            else
            {
                Defaults(null);
                Save();
            }
        }
        public static void Save()
        {
            FileUtilities.INI ini = new FileUtilities.INI();

            foreach ((FieldInfo field, Setting setting) in SettingFields)
                try
                {
                    HeaderAttribute attribute = field.GetCustomAttribute<HeaderAttribute>();
                    if (attribute != null) ini.AddSection(attribute.header);
                    ini.AddItem(setting.Name, setting.ToString());
                }
                catch (Exception exception) { exception.Error(ConsoleUtilities.settingsTag, $"Failed writing {field?.Name:info}"); }

            FileUtilities.Save("Settings.ini", ini);
        }

        private static void ApplyResolution()
        {
#if UNITY_WEBGL
            Screen.fullScreenMode = windowMode;
#else
            Screen.SetResolution(resolution.Value.width, resolution.Value.height, windowMode);
#endif
        }
    }
}