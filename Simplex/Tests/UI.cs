using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.TestTools;
using Style = UnityEngine.UIElements.StyleSheet;

using UnityEditor;
using UnityEditor.TestTools;

using NUnit.Framework;

using Simplex.Editor;


namespace Simplex.Tests
{
    [TestFixture]
    public class UI
    {
        public static TestsWindow window;
        public static Div body;


        [OneTimeSetUp]
        public void Setup()
        {
            window = TestsWindow.Get().OpenTab(WindowTypes.Inspector);
            body = window.Element.body;
        }
        [OneTimeTearDown]
        public void TearDown()
        {
            window.Close();
            window = null;
            body = null;
        }

        [Test]
        public void WIP()
        {
            Assert.That(body.childCount, Is.GreaterThan(0));
        }
    }


    public class TestsWindow : CustomWindow<TestsWindow>
    {
        public TestsWindowElement Element { get; private set; }


        [MenuItem("Window/Simplex Tests", priority = 8000)]
        public static void Open() => Get().OpenTab(WindowTypes.Inspector);
        protected override void OnEnable()
        {
            base.OnEnable();

            Root.Clear();
            Element = Root.Create<TestsWindowElement>("flexible");
            Element.Style(true, AssetUtilities.LoadGuid<Style>((EditorGUIUtility.isProSkin) ? "b29c248324e3d6a43a72964d60a090a5" : "6406c4c077c6ffd429f07d536c9a3638"));
            Element.Refresh();
        }
    }


    public class TestsWindowElement : Div
    {
        protected override string[] DefaultClasses => new string[] { "window", "tests" };

        public readonly Div header;
        public readonly Div body;


        public TestsWindowElement()
        {
            header = this.Create<Div>("header", "toolbar");
            body = this.Create<VerticalScrollView>("body", "flexible");

            header.Create<Label>().Text("Simplex Tests");
            header.Create<HorizontalSpace>("short").Size(Size.Mini);
            header.Create<Label>().Text("UI");
            header.Create<HorizontalSpace>().Size(Size.Mini);
            header.Create<ObjectPicker<Style>>("flexible").Bind(() => (styleSheets.count == 0) ? null : styleSheets[0], ApplyStyle);
            header.Create<HorizontalSpace>().Size(Size.Mini);
            header.Create<Button>().Modify("O").Bind(_ => { TestsWindow.Find()?.Close(); TestsWindow.Open(); });
            header.Create<HorizontalSpace>().Size(Size.Mini);
            header.Create<Button>().Modify("X").Bind(_ => TestsWindow.Find()?.Close());

            body.Create<VerticalSpace>().Size(Size.Huge);
            body.Create<Label>().Text("Work In Progress");
            body.Create<VerticalSpace>().Size(Size.Huge);

            CreateFields();

            CreateCollapsibleView(0);
            CreateCollapsibleView(25);
            CreateCollapsibleView(50);
            CreateCollapsibleView(100);

            CreateCollectionView(0);
            CreateCollectionView(20);
            CreateCollectionView(40);
            CreateCollectionView(80);

            body.Create<DirectoryView<DayOfWeek>>().Bind<DayOfWeek>((v, s) => Debug.Log($"[{s}] {v}"));
            body.Create<DirectoryView<KeyCode>>().Modify("Keycodes", true).Bind<KeyCode>((v, s) => Debug.Log($"[{s}] {v}"));
            body.Create<DirectoryView<Sprite>>().Modify("Sprites", true).Bind<Sprite>(AssetUtilities.Find<Sprite>(null), (v, s) => Debug.Log($"[{s}] {v}"));

            body.Create<VerticalSpace>().Size(Size.Huge);
        }

        private void ApplyStyle(Style style)
        {
            if (style == null) styleSheets.Clear();
            else this.Style(true, style);
        }

        private void CreateFields()
        {
            body.Create<Labeled>().Modify("Button").Create<Button>("flexible").Modify("Clicky").Bind(_ => Debug.Log("Click"));
            body.Create<VerticalSpace>();
            body.Create<Labeled>().Modify("Toggle Slide").Create<ToggleSlide>("flexible");
            body.Create<Labeled>().Modify("Toggle Check").Create<ToggleCheck>("flexible");
            body.Create<Labeled>().Modify("Toggle Button").Create<ToggleButton>("flexible").Modify();
            body.Create<VerticalSpace>();
            body.Create<Labeled>().Modify("Int Slider").Create<IntSlider>("flexible").Modify(0, 20);
            body.Create<Labeled>().Modify("Double Input").Create<DoubleInput>("flexible").Modify();
            body.Create<Labeled>().Modify("String Input").Create<StringInput>("flexible").Modify();
            body.Create<Labeled>().Modify("Float Input Slider").Create<FloatInputSlider>("flexible").Modify(-1, 1, 1);
            body.Create<VerticalSpace>();
            body.Create<Labeled>().Modify("Dropdown").Create<Dropdown>("flexible").Modify();
            body.Create<Labeled>().Modify("Window Mode").Create<Dropdown<string>>("flexible").Modify().Bind(new DelegateValue<string>(() => "Borderless Window", value => Debug.Log(value)), new string[] { "Fullscreen", "Borderless Window", "Fullscreen Window", "Windowed" });
            body.Create<Labeled>().Modify("Keycode").Create<Dropdown<KeyCode>>("flexible").Modify(searchable: true).Bind<KeyCode>(new DelegateValue<KeyCode>(() => KeyCode.Space, value => Debug.Log(value)));
            body.Create<Labeled>().Modify("Day of Week").Create<Dropdown<DayOfWeek>>("flexible").Modify().Bind<DayOfWeek>(new DelegateValue<DayOfWeek>(() => DayOfWeek.Monday, value => Debug.Log(value)));
            body.Create<VerticalSpace>();
            body.Create<Labeled>().Modify("Color Picker").Create<ColorPicker>("flexible").Modify();
            body.Create<Labeled>().Modify("Object Picker").Create<ObjectPicker>("flexible").Modify();
            body.Create<Labeled>().Modify("Sprite Picker").Create<ObjectPicker<Sprite>>("flexible").Modify();
        }
        private CollapsibleView CreateCollapsibleView(int rows)
        {
            body.Create<VerticalSpace>();
            CollapsibleView view = body.Create<CollapsibleView>();
            for (int i = 0; i < rows; i++)
                view.Create<Labeled>().Modify($"{i}");
            return view;
        }
        private CollectionView<int> CreateCollectionView(int rows)
        {
            body.Create<VerticalSpace>();
            CollectionView<int> view = body.Create<CollectionView<int>>();
            view.Bind(Enumerable.Range(0, rows).ToList());
            return view;
        }
    }
}