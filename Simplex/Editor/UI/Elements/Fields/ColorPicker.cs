using System;

using UnityEngine;
using UnityEngine.UIElements;

using UnityEditor;
using UnityEditor.UIElements;


namespace Simplex.Editor
{
    public class ColorPicker : Field<Color>
    {
        protected override string[] DefaultClasses => new string[] { "field", "color" };

        public readonly IMGUIContainer colorField;

        public bool Alpha { get; set; }
        public bool HDR { get; set; }
        public bool EyeDropper { get; set; }
        public bool Delayed { get; set; }


        public ColorPicker()
        {
            colorField = new IMGUIContainer(DrawGUI);
            Add(colorField);

            Modify();
        }
        public ColorPicker Modify(bool alpha = true, bool hdr = false, bool eyeDropper = true, bool delayed = false)
        {
            Alpha = alpha;
            HDR = hdr;
            EyeDropper = eyeDropper;
            Delayed = delayed;

            return this;
        }

        private void DrawGUI()
        {
            CurrentValue = EditorGUILayout.ColorField(GUIContent.none, (Delayed) ? CurrentValue : BindedValue, EyeDropper, Alpha, HDR, GUILayout.Width(resolvedStyle.width), GUILayout.Height(resolvedStyle.height));

            if (!Delayed && BindedValue != CurrentValue)
                BindedValue = CurrentValue;
        }
    }
}