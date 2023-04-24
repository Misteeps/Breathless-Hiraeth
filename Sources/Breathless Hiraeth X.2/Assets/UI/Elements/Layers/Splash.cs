using System;

using UnityEngine;

using Simplex;


namespace Game.UI
{
    public class Splash : Layer<Splash>
    {
        protected override bool DefaultFocusable => false;
        protected override UnityEngine.UIElements.PickingMode DefaultPickingMode => UnityEngine.UIElements.PickingMode.Ignore;

        protected override int ShowMilliseconds => 400;
        protected override int HideMilliseconds => 400;

        public readonly Label warning1;
        public readonly Label warning2;
        public readonly Div image1;
        public readonly Div image2;
        public readonly Div cover;


        public Splash()
        {
            style.opacity = 1;

            Div warnings = this.Create<Div>("area");
            warning1 = warnings.Create<Label>("warning").Text("Unity WebGL may look and run poorly");
            warning2 = warnings.Create<Label>("warning").Text("Consider downloading for best experience");

            Div images = this.Create<Div>("area");
            image1 = images.Create<Div>("image", "top");
            image2 = images.Create<Div>("image", "bottom");

            cover = this.Create<Div>("cover");
        }

        public override async void Show(int milliseconds)
        {
            try
            {
                base.Show(milliseconds);
                await GeneralUtilities.DelayMS(milliseconds);

#if !UNITY_EDITOR
#if UNITY_WEBGL
            warning1.AddToClassList("show");
            await GeneralUtilities.DelayMS(1200);
            warning2.AddToClassList("show");
            await GeneralUtilities.DelayMS(2000);

            warning1.AddToClassList("hide");
            warning2.AddToClassList("hide");
            await GeneralUtilities.DelayMS(600);
#endif

            image1.AddToClassList("show");
            await GeneralUtilities.DelayMS(800);
            image2.AddToClassList("show");
            await GeneralUtilities.DelayMS(1600);
#endif
            }
            catch (Exception exception) { exception.Error($"Failed showing splash"); }
            finally { Hide(); }
        }
        public override async void Hide(int milliseconds)
        {
            Monolith.Load(Progress.scene);

            try
            {
                await GeneralUtilities.DelayMS(1000);
                base.Hide(milliseconds);
                await GeneralUtilities.DelayMS(milliseconds);
                this.RemoveFromHierarchy();
            }
            catch (Exception exception) { exception.Error($"Failed hiding splash"); }
        }
    }
}