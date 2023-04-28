using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex
{
    public class HorizontalSpace : Div
    {
        protected override string[] DefaultClasses => new string[] { "space", "horizontal" };
        protected override Size DefaultSize => Size.Medium;
    }

    public class VerticalSpace : Div
    {
        protected override string[] DefaultClasses => new string[] { "space", "vertical" };
        protected override Size DefaultSize => Size.Medium;
    }
}