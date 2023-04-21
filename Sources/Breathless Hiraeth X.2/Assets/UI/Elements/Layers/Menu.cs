using System;

using UnityEngine;

using Simplex;


namespace Game.UI
{
    public class Menu : Layer<Menu>
    {
        #region Ability
        public class Ability : Div
        {
            protected override string[] DefaultClasses => new string[] { "ability" };


            public int position;
            public string color;
            public string title;
            public string description;

            public readonly Div button;
            public readonly Div icon;
            public readonly Label titleLabel;
            public readonly Label descLabel;

            public Color textColor => (color) switch
            {
                "green" => new Color(0.4f, 1.0f, 0.4f),
                "red" => new Color(1.0f, 0.4f, 0.4f),
                "purple" => new Color(0.5f, 0.4f, 1.0f),
                "yellow" => new Color(1.0f, 0.8f, 0.4f),
                _ => Color.white,
            };


            public Ability()
            {
                button = this.Create<Div>("gui", "button", "square", "gray");
                icon = button.Create<Div>("icon");
                titleLabel = this.Create<Label>("title");
                descLabel = this.Create<Label>("description");

                RegisterCallback<RefreshEvent>(OnRefresh);
            }

            public Ability Bind(int position, string color, string title, string description)
            {
                this.position = position;
                this.color = color;
                this.title = title;
                this.description = description;

                return this.Refresh();
            }

            private void OnRefresh(RefreshEvent refreshEvent)
            {
                if (Progress.abilities >= position)
                {
                    button.ClassToggle(color, "gray", true);
                    icon.Display(true);
                    titleLabel.Text(title).style.color = textColor;
                    descLabel.Text(description);
                }
                else
                {
                    button.ClassToggle(color, "gray", false);
                    icon.Display(false);
                    titleLabel.Text("Locked").style.color = Color.white;
                    descLabel.Text("Keep following Auraline to unlock");
                }
            }
        }
        #endregion Ability


        public Menu()
        {
            Div viewport = this.Create<Div>("viewport", "flexible");

            Div map = viewport.Create<Div>("gui", "background1", "map", "flexible");

            Div abilities = viewport.Create<Div>("gui", "background2", "abilities");
            abilities.Create<Ability>().Bind(1, "green", "Forest Glide", "Conjure a path boosting movement speed and repelling monsters");
            abilities.Create<Ability>().Bind(2, "red", "Inferno Eruption", "Unleash an explosion dealing massive damage to enemies in the radius");
            abilities.Create<Ability>().Bind(3, "purple", "Void Warp", "Teleport to a new location");
            abilities.Create<Ability>().Bind(4, "yellow", "Radiant Sunburst", "Summons the energy of the sun to harm monsters around you");

            Div stats = viewport.Create<Div>("gui", "background2", "stats");
            stats.Create<LabelInput>("stat", "icon").Name("hearts").Bind(() => $"+{Progress.hearts}", null);
            stats.Create<LabelInput>("stat", "icon").Name("damage").Bind(() => $"+{Progress.damage}", null);
            stats.Create<LabelInput>("stat", "icon").Name("ability").Bind(() => $"+{Progress.ability}", null);
            stats.Create<LabelInput>("stat", "icon").Name("speed").Bind(() => $"+{Progress.speed}", null);
            stats.Create<LabelInput>("stat", "icon").Name("cooldown").Bind(() => $"+{Progress.cooldown}", null);
            stats.Create<VerticalSpace>().Size(Size.Huge);
            stats.Create<LabelInput>("stat", "icon").Name("memories").Bind(() => $"+{Progress.memories}", null);

            Div options = this.Create<Div>("gui", "background3", "options");
            options.Create<Button>("gui", "rectangle", "green").Modify("Resume").Bind(_ => UI.Menu.Hide());
            options.Create<Button>("gui", "rectangle", "yellow").Modify("Settings").Bind(_ => { UI.Menu.Hide(); UI.Settings.Show(); });
            options.Create<Button>("gui", "rectangle", "red").Modify("Quit").Bind(_ => { Game.Progress.Save(); Game.Settings.Save(); GeneralUtilities.Quit(); });

            Label saved = this.Create<LabelInput>("saved").Bind(() => $"Last Saved: {Progress.lastSaved.ToShortTimeString()}", null);
        }
    }
}