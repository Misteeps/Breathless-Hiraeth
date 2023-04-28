using System;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex
{
    public class ScrollBar : Div
    {
        protected override string[] DefaultClasses => new string[] { "scroll-bar" };
        protected override bool DefaultFocusable => true;
        protected override PickingMode DefaultPickingMode => PickingMode.Position;

        public readonly Div startArrow;
        public readonly Div endArrow;
        public readonly Div track;
        public readonly Div bar;

        private (int min, int max) barBounds;
        private (int min, int max) trackBounds;
        private (int min, int max) targetBounds;

        private int targetPosition;
        private float clickOffset;
        private bool dragging;
        private bool refreshing;
        private bool locked;

        public VisualElement Target { get; private set; }
        public bool Vertical
        {
            get => ClassListContains("vertical");
            private set => this.ClassToggle("vertical", "horizontal", value);
        }
        public bool Active
        {
            get => enabledSelf;
            private set
            {
                SetEnabled(!locked && value);

                if (!enabledSelf)
                {
                    barBounds = (0, 0);
                    trackBounds = (0, 0);
                    targetBounds = (0, 0);

                    targetPosition = 0;
                    if (Target != null) Target.style.translate = new Translate(0, 0, 0);
                }
            }
        }
        public bool Locked
        {
            get => locked;
            set
            {
                locked = value;

                if (locked) Active = false;
                else this.Refresh();
            }
        }

        public int Position
        {
            get => targetPosition;
            set
            {
                if (!Active || targetPosition == value) return;
                targetPosition = Mathf.Clamp(value, targetBounds.min, targetBounds.max);

                if (Vertical)
                {
                    bar.style.translate = new Translate(0, Mathf.Lerp(barBounds.min, barBounds.max, Factor), 0);
                    Target.style.translate = new Translate(0, -targetPosition, 0);
                }
                else
                {
                    bar.style.translate = new Translate(Mathf.Lerp(barBounds.min, barBounds.max, Factor), 0, 0);
                    Target.style.translate = new Translate(-targetPosition, 0, 0);
                }
            }
        }
        public float Factor
        {
            get => Mathf.InverseLerp(targetBounds.min, targetBounds.max, targetPosition);
            set => Position = Mathf.RoundToInt(Mathf.Lerp(targetBounds.min, targetBounds.max, value));
        }


        public ScrollBar()
        {
            startArrow = this.Create<Div>("arrow", "icon").Name("start");
            track = this.Create<Div>("track", "flexible");
            bar = track.Create<Div>("bar");
            endArrow = this.Create<Div>("arrow", "icon").Name("end");

            RegisterCallback<RefreshEvent>(OnRefresh);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChange);

            Active = false;
        }

        public ScrollBar Bind(VisualElement target, bool vertical = true)
        {
            Target?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChange);

            Target = target;
            Vertical = vertical;

            Target?.RegisterCallback<GeometryChangedEvent>(OnGeometryChange);

            return this.Refresh();
        }

        private void OnPointerUp(PointerUpEvent pointerEvent)
        {
            if (!dragging || pointerEvent.button != 0) return;

            dragging = false;
            bar.style.transitionDuration = StyleKeyword.Null;

            this.ReleasePointer(pointerEvent.pointerId);
            pointerEvent.StopPropagation();
        }
        private void OnPointerDown(PointerDownEvent pointerEvent)
        {
            if (dragging || pointerEvent.button != 0) return;

            dragging = true;
            bar.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue>() { new TimeValue(0.1f) });

            if (Vertical)
            {
                clickOffset = bar.worldBound.center.y - pointerEvent.position.y;
                if (Mathf.Abs(clickOffset) > bar.resolvedStyle.height / 2)
                    clickOffset = 0;

                Factor = Mathf.InverseLerp(trackBounds.min, trackBounds.max, pointerEvent.localPosition.y + clickOffset);
            }
            else
            {
                clickOffset = bar.worldBound.center.x - pointerEvent.position.x;
                if (Mathf.Abs(clickOffset) > bar.resolvedStyle.width / 2)
                    clickOffset = 0;

                Factor = Mathf.InverseLerp(trackBounds.min, trackBounds.max, pointerEvent.localPosition.x + clickOffset);
            }

            this.CapturePointer(pointerEvent.pointerId);
            pointerEvent.StopPropagation();
        }
        private void OnPointerMove(PointerMoveEvent pointerEvent)
        {
            if (!dragging || !this.HasPointerCapture(pointerEvent.pointerId)) return;

            Factor = Mathf.InverseLerp(trackBounds.min, trackBounds.max, ((Vertical) ? pointerEvent.localPosition.y : pointerEvent.localPosition.x) + clickOffset);

            pointerEvent.StopPropagation();
        }
        private void OnGeometryChange(GeometryChangedEvent geometryEvent) => this.Refresh();
        private async void OnRefresh(RefreshEvent refreshEvent)
        {
            if (refreshing) return;
            refreshing = true;

            await GeneralUtilities.DelayFrame(10);
            refreshing = false;

            Active = IsScrollable(out int length, out int view);
            if (!Active) return;

            Rect trackLocalBound = track.localBound;
            trackBounds = (Vertical) ? ((int)trackLocalBound.yMin, (int)trackLocalBound.yMax) : ((int)trackLocalBound.xMin, (int)trackLocalBound.xMax);

            int barLength = Mathf.RoundToInt(Mathf.Lerp(0, trackBounds.max - trackBounds.min, Mathf.Clamp(Mathf.InverseLerp(800, 0, length - view), 0.2f, 1)));
            int barMargin = Mathf.RoundToInt(barLength * 0.5f);

            barBounds = (barMargin, (trackBounds.max - trackBounds.min) - barMargin);
            trackBounds = (trackBounds.min + barMargin, trackBounds.max - barMargin);
            targetBounds = (0, length - view);

            if (Vertical)
            {
                bar.style.marginTop = -barMargin;
                bar.style.marginLeft = StyleKeyword.Null;
                bar.style.height = barLength;
                bar.style.width = new Length(100, LengthUnit.Percent);
                bar.style.translate = new Translate(0, Mathf.Lerp(barBounds.min, barBounds.max, Factor), 0);
                Target.style.translate = new Translate(0, -targetPosition, 0);

            }
            else
            {
                bar.style.marginTop = StyleKeyword.Null;
                bar.style.marginLeft = -barMargin;
                bar.style.height = new Length(100, LengthUnit.Percent);
                bar.style.width = barLength;
                bar.style.translate = new Translate(Mathf.Lerp(barBounds.min, barBounds.max, Factor), 0, 0);
                Target.style.translate = new Translate(-targetPosition, 0, 0);
            }
        }

        private bool IsScrollable(out int length, out int view)
        {
            if (Target == null || Target.parent == null)
            {
                length = 0;
                view = 0;
                return false;
            }

            Rect boundingBox = (Rect)typeof(VisualElement).GetProperty("boundingBox", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Target);
            //Rect worldClip = (Rect)typeof(VisualElement).GetProperty("worldClip", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Target);

            if (Vertical)
            {
                length = (int)boundingBox.height;
                view = (int)Target.parent.layout.height;
            }
            else
            {
                length = (int)boundingBox.width;
                view = (int)Target.parent.layout.width;
            }

            return length > view;
        }
    }
}