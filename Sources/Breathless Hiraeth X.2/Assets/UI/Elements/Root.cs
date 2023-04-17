using System;

using UnityEngine;

using Simplex;


namespace Game.UI
{
    public class Root : Div
    {
        public static Root Instance { get; } = Monolith.Refs.uiDocument.rootVisualElement.Create<Root>().Style(Monolith.Refs.uiStyle);
        public static UnityEngine.UIElements.Focusable Focused => Instance.panel.focusController.focusedElement;

        private static Layer layer;
        public static Layer Layer
        {
            get => layer;
            set
            {
                layer = value;
                Monolith.Player.enabled = layer == null;
                Time.timeScale = (layer == null) ? 1 : 0;
            }
        }

        protected override string[] DefaultClasses => new string[] { "root" };


        public Root()
        {

        }
    }
}