using System;

using UnityEngine;
using UnityEngine.Rendering.Universal;

using Simplex;


namespace Game.UI
{
    #region Dropdown Binder Extension
    public static class DropdownBinderExtension
    {
        public static void BindDropdown<T>(this Labeled<Dropdown<T>> labeled, Game.Settings.Choice<T> setting) => labeled.Bind(setting).Elements(dropdown => dropdown.Bind(setting, setting.values, value => setting.Find(value)));
    }
    #endregion Dropdown Binder Extension

    public class Settings : Layer<Settings>
    {
        #region Keybind
        public class Keybind : Field, IBindable
        {
            protected override string[] DefaultClasses => new string[] { "field", "keybind" };

            public IValue IValue { get => keybind; set => keybind = (Game.Settings.Keybind)value; }
            private Game.Settings.Keybind keybind;

            public readonly Button primary;
            public readonly Button secondary;


            public Keybind()
            {
                primary = this.Create<Button>("gui", "rectangle", "yellow").Bind(_ => Rebind(true));
                secondary = this.Create<Button>("gui", "rectangle", "yellow").Bind(_ => Rebind(false));
            }

            protected override void OnRefresh(RefreshEvent refreshEvent)
            {
                primary.Text(keybind.Primary.KeycodeString());
                secondary.Text(keybind.Secondary.KeycodeString());
            }

            private async void Rebind(bool primaryBind)
            {
                if (keybind == null) throw new NullReferenceException("Keybind");
                if (primaryBind && keybind.lockPrimary)
                {
                    Audio.UI.global.PlayOneShot(Monolith.Refs.uiError);
                    primary.ClassToggle("red", "yellow", true);
                    await GeneralUtilities.DelayMS(120);
                    primary.ClassToggle("red", "yellow", false);
                    return;
                }

                Audio.UI.global.PlayOneShot(Monolith.Refs.uiClick);
                KeyCode key = (primaryBind) ? keybind.Primary : keybind.Secondary;
                Button button = (primaryBind) ? primary : secondary;
                button.ClassToggle("green", "yellow", true);
                Monolith.Instance.enabled = false;

                bool loop = true;
                while (loop)
                {
                    await GeneralUtilities.DelayFrame(1);
                    if (Input.GetKeyDown(KeyCode.Escape)) break;
                    if (Input.GetKeyDown(key)) { key = KeyCode.None; break; }
                    foreach (KeyCode keycode in Enum.GetValues(typeof(KeyCode)))
                        if (Input.GetKeyDown(keycode))
                        {
                            key = keycode;
                            loop = false;
                            break;
                        }
                }

                if (primaryBind) keybind.Set((key, keybind.Secondary));
                else keybind.Set((keybind.Primary, key));

                button.ClassToggle("green", "yellow", false);
                Monolith.Instance.enabled = true;

                this.Refresh();
            }
        }
        #endregion Keybind


        public Settings()
        {
            Div center = this.Create<Div>("center");

            Div panel = center.Create<Div>("gui", "background1", "panel");
            VerticalScrollView contents = panel.Create<VerticalScrollView>("flexible");
            CreateGeneral(contents);
            CreateGraphics(contents);
            CreateAudio(contents);
            CreateKeybinds(contents);

            Div options = center.Create<Div>("gui", "background3", "options");
            options.Create<Button>("gui", "rectangle", "purple").Modify("Defaults").Bind(_ => { Game.Settings.Defaults(null); this.Refresh(); Audio.UI.global.PlayOneShot(Monolith.Refs.uiClick); });
            options.Create<Button>("gui", "rectangle", "green").Modify("Return").Bind(_ => { UI.Settings.Hide(); Audio.UI.global.PlayOneShot(Monolith.Refs.uiClick); });
        }

        public override void Hide(int milliseconds)
        {
            base.Hide(milliseconds);
            Game.Settings.Save();
        }

        private void CreateGeneral(VerticalScrollView contents)
        {
            contents.Create<VerticalSpace>().Size(Size.Huge);
            contents.Create<Labeled>("header").Modify("General", highlight: false);
            contents.Create<VerticalSpace>();
            contents.Create<Labeled<ToggleCheck>>().Bind(Game.Settings.combatZoom);
            contents.Create<Labeled<ToggleCheck>>().Bind(Game.Settings.abilityZoom);
            contents.Create<Labeled<ToggleCheck>>().Bind(Game.Settings.autoExitCombat);
            contents.Create<VerticalSpace>();
            contents.Create<Labeled<ToggleCheck>>().Bind(Game.Settings.cameraShake);
            contents.Create<Labeled<IntInputSlider>>().Bind(Game.Settings.zoom).Elements(e => e.Modify(0, 15));
            contents.Create<VerticalSpace>();
            contents.Create<Labeled<FloatInputSlider>>().Bind(Game.Settings.hudScale).Elements(e => e.Modify(0, 2));
            contents.Create<VerticalSpace>().Size(Size.Huge);
        }
        private void CreateGraphics(VerticalScrollView contents)
        {
            contents.Create<VerticalSpace>().Size(Size.Huge);
            contents.Create<Labeled>("header").Modify("Graphics", highlight: false);
            contents.Create<VerticalSpace>();
            contents.Create<Labeled<Dropdown<FullScreenMode>>>().BindDropdown(Game.Settings.windowMode);
            contents.Create<Labeled<Dropdown<(int, int)>>>().BindDropdown(Game.Settings.resolution);
            contents.Create<Labeled<IntInputSlider>>().Bind(Game.Settings.fpsLimit).Elements(e => e.Modify(30, 361).OnRefresh(async _ => { await GeneralUtilities.DelayFrame(1); if (Game.Settings.fpsLimit == 361) e.input.text = "Inf."; }));
            contents.Create<Labeled<ToggleCheck>>().Bind(Game.Settings.fpsCounter);
            contents.Create<Labeled<ToggleCheck>>().Bind(Game.Settings.vSync);
            contents.Create<VerticalSpace>();
            contents.Create<Labeled<FloatInputSlider>>().Bind(Game.Settings.renderScale).Elements(e => e.Modify(0.5f, 1.25f, 2));
            contents.Create<Labeled<FloatInputSlider>>().Bind(Game.Settings.viewDistance).Elements(e => e.Modify(0.5f, 5f, 1));
            contents.Create<Labeled<Dropdown<int>>>().BindDropdown(Game.Settings.textureQuality);
            contents.Create<Labeled<Dropdown<int>>>().BindDropdown(Game.Settings.anisotropicFiltering);
            contents.Create<Labeled<Dropdown<int>>>().BindDropdown(Game.Settings.antiAliasing);
            contents.Create<Labeled<Dropdown<SoftShadowQuality>>>().BindDropdown(Game.Settings.shadowQuality);
            contents.Create<Labeled<IntInputSlider>>().Bind(Game.Settings.shadowDistance).Elements(e => e.Modify(5, 30));
            contents.Create<VerticalSpace>().Size(Size.Huge);
        }
        private void CreateAudio(VerticalScrollView contents)
        {
            contents.Create<VerticalSpace>().Size(Size.Huge);
            contents.Create<Labeled>("header").Modify("Audio", highlight: false);
            contents.Create<VerticalSpace>();
            contents.Create<Labeled<IntInputSlider>>().Bind(Game.Settings.masterVolume).Elements(e => e.Modify(0, 100));
            contents.Create<VerticalSpace>();
            contents.Create<Labeled<IntInputSlider>>().Bind(Game.Settings.uiVolume).Elements(e => e.Modify(0, 100));
            contents.Create<Labeled<IntInputSlider>>().Bind(Game.Settings.sfxVolume).Elements(e => e.Modify(0, 100));
            contents.Create<Labeled<IntInputSlider>>().Bind(Game.Settings.voiceVolume).Elements(e => e.Modify(0, 100));
            contents.Create<Labeled<IntInputSlider>>().Bind(Game.Settings.ambianceVolume).Elements(e => e.Modify(0, 100));
            contents.Create<Labeled<IntInputSlider>>().Bind(Game.Settings.musicVolume).Elements(e => e.Modify(0, 100));
            contents.Create<VerticalSpace>().Size(Size.Huge);
        }
        private void CreateKeybinds(VerticalScrollView contents)
        {
            contents.Create<VerticalSpace>().Size(Size.Huge);
            contents.Create<VerticalSpace>();
            contents.Create<Labeled>("header").Modify("Keybinds", highlight: false);
            contents.Create<Labeled<Keybind>>("keybind").Bind(Game.Settings.click);
            contents.Create<Labeled<Keybind>>("keybind").Bind(Game.Settings.escape);
            contents.Create<VerticalSpace>();
            contents.Create<Labeled<Keybind>>("keybind").Bind(Game.Settings.moveUp);
            contents.Create<Labeled<Keybind>>("keybind").Bind(Game.Settings.MoveDown);
            contents.Create<Labeled<Keybind>>("keybind").Bind(Game.Settings.moveLeft);
            contents.Create<Labeled<Keybind>>("keybind").Bind(Game.Settings.moveRight);
            contents.Create<Labeled<Keybind>>("keybind").Bind(Game.Settings.sprint);
            contents.Create<Labeled<Keybind>>("keybind").Bind(Game.Settings.jump);
            contents.Create<VerticalSpace>();
            contents.Create<Labeled<Keybind>>("keybind").Bind(Game.Settings.breath);
            contents.Create<Labeled<Keybind>>("keybind").Bind(Game.Settings.attack);
            contents.Create<Labeled<Keybind>>("keybind").Bind(Game.Settings.ability1);
            contents.Create<Labeled<Keybind>>("keybind").Bind(Game.Settings.ability2);
            contents.Create<Labeled<Keybind>>("keybind").Bind(Game.Settings.ability3);
            contents.Create<Labeled<Keybind>>("keybind").Bind(Game.Settings.ability4);
            contents.Create<Labeled<Keybind>>("keybind").Bind(Game.Settings.cancelAbility);
            contents.Create<VerticalSpace>();
            contents.Create<Labeled<Keybind>>("keybind").Bind(Game.Settings.zoomIn);
            contents.Create<Labeled<Keybind>>("keybind").Bind(Game.Settings.zoomOut);
            contents.Create<Labeled<Keybind>>("keybind").Bind(Game.Settings.rotateCamera);
            contents.Create<VerticalSpace>().Size(Size.Huge);
        }
    }
}