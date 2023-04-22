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
        public readonly Label ability1;
        public readonly Label ability2;
        public readonly Label ability3;
        public readonly Label ability4;

        public readonly Label breath;

        private int currentStage;
        private Label stage;
        private Label banner;
        private Label dialog;
        private Label tip;


        public Hud()
        {
            health = this.Create<Div>("health");

            Div gauge = health.Create<Div>("pressure");
            pressure = gauge.Create<Div>("bar");
            pressure.Create<Div>("slider");
            gauge.Create<Div>("tick", "1");
            gauge.Create<Div>("tick", "2");
            gauge.Create<Div>("tick", "3");

            abilities = this.Create<Div>("abilities");
            ability1 = abilities.Create<Label>("ability", "gui", "button", "square", "icon", "gray");
            ability1.Create<Div>("icon");
            ability2 = abilities.Create<Label>("ability", "gui", "button", "square", "icon", "gray");
            ability2.Create<Div>("icon");
            ability3 = abilities.Create<Label>("ability", "gui", "button", "square", "icon", "gray");
            ability3.Create<Div>("icon");
            ability4 = abilities.Create<Label>("ability", "gui", "button", "square", "icon", "gray");
            ability4.Create<Div>("icon");

            stage = this.Create<Label>("stage");
            breath = this.Create<Label>("breath").Text("Press [F] to take a deep breath");
            breath.schedule.Execute(() => breath.style.translate = new UnityEngine.UIElements.Translate(RNG.Generic.Int(-2, 2), RNG.Generic.Int(-2, 2))).Every(10);
        }

        public void UpdateAbilities()
        {

        }
        public void CooldownAbilities()
        {
            Set(ability1, RNG.Generic.Float(0, 20));
            Set(ability2, RNG.Generic.Float(0, 20));
            Set(ability3, RNG.Generic.Float(0, 20));
            Set(ability4, RNG.Generic.Float(0, 20));

            static void Set(Label ability, float seconds) => ability.Text(seconds.ToString((seconds < 3) ? "f1" : "f0")).EnableInClassList("cooldown", seconds > 0);
        }

        public void Scale(float scale)
        {
            var scaleStyle = new UnityEngine.UIElements.Scale(new Vector2(scale, scale));
            health.style.scale = scaleStyle;
            abilities.style.scale = scaleStyle;
            breath.style.scale = scaleStyle;
        }
        public void SetHealth(int amount)
        {
            while (health.childCount - 1 < Progress.hearts)
                health.Create<Div>("slot").Create<Div>("heart", "icon");

            for (int i = 1; i < health.childCount; i++)
                health[i].ClassToggle("show", "hide", i <= amount);
        }
        public void SetPressure(float amount)
        {
            pressure.style.width = new UnityEngine.UIElements.Length(amount, UnityEngine.UIElements.LengthUnit.Percent);
            breath.style.opacity = Mathf.InverseLerp(40, 70, amount);
            if (Monolith.PressureStage != currentStage)
            {
                currentStage = Monolith.PressureStage;
                PressureStage(currentStage);
            }
        }

        public void PressureStage(int pressure)
        {
            if (stage != null)
            {
                Label trash = stage;
                trash.RemoveFromClassList("show");
                trash.schedule.Execute(() => trash.RemoveFromHierarchy()).ExecuteLater(480);
                stage = null;
            }

            if (pressure == 0)
                return;

            stage = this.Create<Label>("stage").Text($"Damage Dealt x{pressure + 1}\nDamage Taken x{pressure * 2}");
            stage.schedule.Execute(() => stage.AddToClassList("show")).ExecuteLater(10);
        }
        public void Banner(string text, int duration = 4000)
        {
            if (banner != null)
            {
                Label trash = banner;
                trash.RemoveFromClassList("show");
                trash.schedule.Execute(() => trash.RemoveFromHierarchy()).ExecuteLater(480);
                banner = null;
            }

            if (string.IsNullOrEmpty(text))
                return;

            banner = this.Create<Label>("banner").Text(text);
            banner.schedule.Execute(() => banner.AddToClassList("show")).ExecuteLater(10);
            banner.schedule.Execute(() => Banner(null)).ExecuteLater(duration);
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