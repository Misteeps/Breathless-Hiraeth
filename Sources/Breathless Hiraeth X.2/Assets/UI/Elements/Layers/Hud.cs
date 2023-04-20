using System;

using UnityEngine;

using Simplex;


namespace Game.UI
{
    public class Hud : Layer<Hud>
    {
        protected override bool DefaultFocusable => false;
        protected override UnityEngine.UIElements.PickingMode DefaultPickingMode => UnityEngine.UIElements.PickingMode.Ignore;

        public readonly Div health;
        public readonly Div pressure;

        public readonly Div abilities;
        public readonly Div ability1;
        public readonly Div ability2;
        public readonly Div ability3;
        public readonly Div ability4;

        public readonly Label stage;
        public readonly Label breath;

        private int currentStage;
        private Label dialog;
        private Label tip;


        public Hud()
        {
            health = this.Create<Div>("health");

            Div gauge = health.Create<Div>("pressure");
            pressure = gauge.Create<Div>("bar");
            gauge.Create<Div>("tick", "1");
            gauge.Create<Div>("tick", "2");
            gauge.Create<Div>("tick", "3");

            abilities = this.Create<Div>("abilities");
            ability1 = abilities.Create<Div>("ability", "gui", "button", "square", "icon", "gray");
            ability2 = abilities.Create<Div>("ability", "gui", "button", "square", "icon", "gray");
            ability3 = abilities.Create<Div>("ability", "gui", "button", "square", "icon", "gray");
            ability4 = abilities.Create<Div>("ability", "gui", "button", "square", "icon", "gray");

            stage = this.Create<Label>("stage");
            breath = this.Create<Label>("breath").Text("Press [F] to take a deep breath");
            breath.schedule.Execute(() => breath.style.translate = new UnityEngine.UIElements.Translate(RNG.Generic.Int(-2, 2), RNG.Generic.Int(-2, 2))).Every(10);
        }

        public void Scale(float scale)
        {
            var scaleStyle = new UnityEngine.UIElements.Scale(new Vector2(scale, scale));
            health.style.scale = scaleStyle;
            abilities.style.scale = scaleStyle;
            stage.style.scale = scaleStyle;
            breath.style.scale = scaleStyle;
        }
        public void SetHealth(int amount)
        {
            while (health.childCount - 1 < amount)
                health.Create<Div>("slot").Create<Div>("heart", "icon");

            for (int i = 1; i < health.childCount; i++)
                health[i].ClassToggle("show", "hide", i <= amount);
        }
        public void SetPressure(float amount)
        {
            pressure.style.width = new UnityEngine.UIElements.Length(amount, UnityEngine.UIElements.LengthUnit.Percent);
            breath.style.opacity = Mathf.InverseLerp(40, 70, amount);

            int pressureStage = Monolith.PressureStage;
            if (pressureStage != currentStage)
            {
                currentStage = pressureStage;
                if (pressureStage == 0) stage.RemoveFromClassList("show");
                else stage.Text($"Damage Dealt x{pressureStage + 1}\nDamage Taken x{pressureStage * 2}").AddToClassList("show");
            }

        }

        public void Tip(string text, int duration = 4000)
        {
            if (tip != null)
            {
                Label trash = tip;
                trash.RemoveFromClassList("show");
                trash.schedule.Execute(() => trash.RemoveFromHierarchy()).ExecuteLater(600);
                tip = null;
            }

            if (string.IsNullOrEmpty(text))
                return;

            tip = this.Create<Label>("tip").Text(text);
            tip.schedule.Execute(() => tip.AddToClassList("show")).ExecuteLater(10);
            tip.schedule.Execute(() => Tip(null)).ExecuteLater(duration);
        }
        public void FairyDialog(string text)
        {
            if (dialog != null)
            {
                Label trash = dialog;
                trash.RemoveFromClassList("show");
                trash.schedule.Execute(() => trash.RemoveFromHierarchy()).ExecuteLater(320);
                dialog = null;
            }

            if (string.IsNullOrEmpty(text))
                return;

            dialog = this.Create<Label>("dialog").Text(text);
            dialog.schedule.Execute(() => dialog.AddToClassList("show")).ExecuteLater(10);
        }
        public void PositionFairyDialog(Vector3 fairy)
        {
            if (dialog == null) return;

            Vector2 position = UnityEngine.UIElements.RuntimePanelUtils.CameraTransformWorldToPanel(panel, fairy, Monolith.Camera);
            dialog.style.top = position.y - 40 - (dialog.resolvedStyle.height / 2);
            dialog.style.left = position.x - (dialog.resolvedStyle.width / 2);
        }
    }
}