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

        public readonly Div ability1;
        public readonly Div ability2;
        public readonly Div ability3;
        public readonly Div ability4;


        public Hud()
        {
            health = this.Create<Div>("health");
            Div gauge = health.Create<Div>("pressure");
            pressure = gauge.Create<Div>("bar");

            Div abilities = this.Create<Div>("abilities");
            ability1 = abilities.Create<Div>("ability", "gui", "button", "square", "icon", "gray");
            ability2 = abilities.Create<Div>("ability", "gui", "button", "square", "icon", "gray");
            ability3 = abilities.Create<Div>("ability", "gui", "button", "square", "icon", "gray");
            ability4 = abilities.Create<Div>("ability", "gui", "button", "square", "icon", "gray");

            SetHealth(18);
        }

        public void SetHealth(int amount)
        {
            for (int i = 0; i < amount; i++)
                health.Create<Div>("icon", "heart");
        }
    }
}