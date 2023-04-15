using System;

using UnityEngine;

using Simplex;


namespace Game.UI
{
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
            contents.Create<Labeled<Dropdown<FullScreenMode>>>().Bind(Game.Settings.windowMode);
            contents.Create<Labeled<Dropdown<(int, int)>>>().Bind(Game.Settings.resolution);
            contents.Create<Labeled<IntInputSlider>>().Bind(Game.Settings.fpsLimit).Elements(e => e.Modify(30, 361).OnRefresh(async _ => { await GeneralUtilities.DelayFrame(1); if (Game.Settings.fpsLimit == 361) e.input.text = "Inf."; }));
            contents.Create<Labeled<ToggleCheck>>().Bind(Game.Settings.fpsCounter);
            contents.Create<Labeled<ToggleCheck>>().Bind(Game.Settings.vSync);
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
            contents.Create<VerticalSpace>().Size(Size.Huge);
        }
    }
}