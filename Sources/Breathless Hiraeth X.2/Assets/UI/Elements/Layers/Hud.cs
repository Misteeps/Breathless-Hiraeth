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

            SetHealth(10);
        }

        public void Scale(float scale)
        {
            health.style.scale = new UnityEngine.UIElements.Scale(new Vector2(scale, scale));
            abilities.style.scale = new UnityEngine.UIElements.Scale(new Vector2(scale, scale));
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
            // Set Breath Text
        }
    }
}