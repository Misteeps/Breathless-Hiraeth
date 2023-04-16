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
            options.Create<Button>("gui", "rectangle", "purple").Modify("Defaults").Bind(_ => Debug.Log("DEFAULTS"));
            options.Create<Button>("gui", "rectangle", "green").Modify("Return").Bind(_ => UI.Settings.Hide());
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
            contents.Create<Labeled<IntInputSlider>>().Bind(Game.Settings.zoom).Elements(e => e.Modify(0, 15));
            contents.Create<VerticalSpace>();
            contents.Create<Labeled<ToggleCheck>>().Bind(Game.Settings.abilityAimZoom);
            contents.Create<Labeled<ToggleCheck>>().Bind(Game.Settings.autoExitCombat);
            contents.Create<Labeled<ToggleCheck>>().Bind(Game.Settings.cameraShake);
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
            contents.Create<Labeled<Label>>().Bind(Game.Settings.click).Elements(e => e.Text(Game.Settings.click.Display()));
            contents.Create<Labeled<Label>>().Bind(Game.Settings.escape).Elements(e => e.Text(Game.Settings.escape.Display()));
            contents.Create<VerticalSpace>();
            contents.Create<Labeled<Label>>().Bind(Game.Settings.moveUp).Elements(e => e.Text(Game.Settings.moveUp.Display()));
            contents.Create<Labeled<Label>>().Bind(Game.Settings.MoveDown).Elements(e => e.Text(Game.Settings.MoveDown.Display()));
            contents.Create<Labeled<Label>>().Bind(Game.Settings.moveLeft).Elements(e => e.Text(Game.Settings.moveLeft.Display()));
            contents.Create<Labeled<Label>>().Bind(Game.Settings.moveRight).Elements(e => e.Text(Game.Settings.moveRight.Display()));
            contents.Create<Labeled<Label>>().Bind(Game.Settings.sprint).Elements(e => e.Text(Game.Settings.sprint.Display()));
            contents.Create<Labeled<Label>>().Bind(Game.Settings.jump).Elements(e => e.Text(Game.Settings.jump.Display()));
            contents.Create<VerticalSpace>();
            contents.Create<Labeled<Label>>().Bind(Game.Settings.breath).Elements(e => e.Text(Game.Settings.breath.Display()));
            contents.Create<Labeled<Label>>().Bind(Game.Settings.attack).Elements(e => e.Text(Game.Settings.attack.Display()));
            contents.Create<Labeled<Label>>().Bind(Game.Settings.ability1).Elements(e => e.Text(Game.Settings.ability1.Display()));
            contents.Create<Labeled<Label>>().Bind(Game.Settings.ability2).Elements(e => e.Text(Game.Settings.ability2.Display()));
            contents.Create<Labeled<Label>>().Bind(Game.Settings.ability3).Elements(e => e.Text(Game.Settings.ability3.Display()));
            contents.Create<Labeled<Label>>().Bind(Game.Settings.ability4).Elements(e => e.Text(Game.Settings.ability4.Display()));
            contents.Create<Labeled<Label>>().Bind(Game.Settings.cancelAbility).Elements(e => e.Text(Game.Settings.cancelAbility.Display()));
            contents.Create<VerticalSpace>();
            contents.Create<Labeled<Label>>().Bind(Game.Settings.zoomIn).Elements(e => e.Text(Game.Settings.zoomIn.Display()));
            contents.Create<Labeled<Label>>().Bind(Game.Settings.zoomOut).Elements(e => e.Text(Game.Settings.zoomOut.Display()));
            contents.Create<VerticalSpace>().Size(Size.Huge);
        }
    }
}