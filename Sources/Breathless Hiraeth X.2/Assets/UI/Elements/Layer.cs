using System;

using UnityEngine;

using Simplex;


namespace Game.UI
{
    public abstract class Layer<T> : Layer where T : Layer, new()
    {
        public static T Instance { get; } = Root.Instance.Create<T>();


        public static new void Show() => Instance.Show();
        public static new void Hide() => Instance.Hide();
    }


    public abstract class Layer : Div
    {
        protected override string[] DefaultClasses => new string[] { "layer" };
        protected override bool DefaultFocusable => true;
        protected override UnityEngine.UIElements.PickingMode DefaultPickingMode => UnityEngine.UIElements.PickingMode.Position;

        protected virtual string Name => GetType().Name;

        protected virtual int ShowMilliseconds => 320;
        public event Action OnShow;
        protected virtual int HideMilliseconds => 240;
        public event Action OnHide;


        public Layer()
        {
            this.Name(Name, toLower: false).Display(false);

            style.opacity = 0;

            OnHide += () => this.Display(false);
        }

        public void Show() => Show(ShowMilliseconds);
        public void Hide() => Hide(HideMilliseconds);
        public virtual void Show(int milliseconds)
        {
            this.Enable(true, DefaultPickingMode).Display(true).Refresh().Focus();
            this.Transition(VisualElementField.Opacity, Unit.A, 0, 1).Curve(Function.Circular, Direction.Out, milliseconds).Start();

            if (focusable)
                Root.Layer = this;

            OnShow?.Invoke();
        }
        public virtual void Hide(int milliseconds)
        {
            this.Enable(false);
            this.Transition(VisualElementField.Opacity, Unit.A, 1, 0, OnHide).Curve(Function.Circular, Direction.Out, milliseconds).Start();

            if (Root.Layer == this)
                Root.Layer = null;
        }
    }
}