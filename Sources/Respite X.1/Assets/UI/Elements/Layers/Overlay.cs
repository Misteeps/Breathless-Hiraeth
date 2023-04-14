using System;

using UnityEngine;

using Simplex;


namespace Game.UI
{
    public class Overlay : Layer<Overlay>
    {
        protected override bool DefaultFocusable => false;
        protected override UnityEngine.UIElements.PickingMode DefaultPickingMode => UnityEngine.UIElements.PickingMode.Ignore;

        public readonly Label fps;
        public readonly Div loading;
        public readonly Div loadingBar;


        public Overlay()
        {
            style.backgroundColor = Color.black;

            fps = this.Create<Label>("fps").Size(Size.Small);
            fps.schedule.Execute(UpdateFPS).Every(1000);

            loading = this.Create<Div>("loading");
            loading.Create<Div>("bar").style.opacity = 0.02f;
            loadingBar = loading.Create<Div>("bar");
        }

        private void UpdateFPS()
        {
            int fpsCount = (int)(1f / Time.unscaledDeltaTime);
            fps.text = $"FPS: {fpsCount}";
        }
    }
}