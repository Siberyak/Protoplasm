using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Protoplasm.Utils.Graph;

namespace Factorio.Lua.Reader
{
    public partial class RecipiesControl : UserControl
    {
        private readonly Storage _storage;

        public RecipiesControl()
        {
            InitializeComponent();
            propertyGridControl1.AutoGenerateRows = true;
        }

        public RecipiesControl(Storage storage) : this()
        {
            _storage = storage;
            MinimumSize = new Size(80 * 6, 400);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            AfterLoad();
        }

        private void AfterLoad()
        {
            var recipes = _storage.Nodes<Recipe>(x => !x._Hidden).ToArray();
            var groupBy = recipes.GroupBy(x => x.SubGroup).GroupBy(x => x.Key.ItemGroup);

            splitContainerControl1.Panel1.Tag = new Point(0, 0);

            var groupIndex = -1;
            foreach (var g1 in groupBy.OrderBy(x => x.Key._Order))
            {
                groupIndex++;

                ItemGroup itemGroup = g1.Key;
                var itemGroupControl = Add(splitContainerControl1.Panel1, itemGroup);
                
                foreach (var g2 in g1.OrderBy(x => x.Key.Order))
                {
                    ItemSubGroup itemSubGroup = g2.Key;

                    foreach (var recipe in g2.OrderBy(x => x.Order))
                    {
                        var button = Add((Control)itemGroupControl.Tag, recipe, groupIndex);
                        button.Tag = recipe;
                        button.Click += RecipeButtonClick;
                    }

                    Point point = (Point) ((Control) itemGroupControl.Tag).Tag;
                    if(point.X != 0)
                        ((Control)itemGroupControl.Tag).Tag = new Point(0, point.Y+1);
                }
            }
        }

        private void RecipeButtonClick(object sender, EventArgs e)
        {
            var recipe = ((Control) sender).Tag as Recipe;
            propertyGridControl1.SelectedObject = recipe;
            recipeView1.Recipe = recipe;
        }

        private static CheckButton AddButton(Control holdedBy, int perLine, int dim, ImageCollection images, int imageIndex, string toolTip, int groupIndex)
        {
            Point point = (Point)holdedBy.Tag;
            var count = point.X + point.Y * perLine;
            int i;
            var j = Math.DivRem(count, perLine, out i);


            Point location = new Point(4 + i * dim, 4 + j * dim);

            count++;
            j = Math.DivRem(count, perLine, out i);
            holdedBy.Tag = new Point(i, j);


            var button = new CheckButton
            {
                Size = new Size(dim-6, dim-6),
                ImageList = images,
                ImageIndex = imageIndex,
                ImageLocation = ImageLocation.MiddleCenter,
                AllowGlyphSkinning = DefaultBoolean.False,
                Location = location,
                AllowFocus = false,
                ToolTip = toolTip,
                GroupIndex = groupIndex
            };


            holdedBy.Controls.Add(button);
            return button;
        }


        private Control Add(Control holdedBy, ItemGroup itemGroup)
        {
            var toolTip = itemGroup.LocalizedName;
            var images = ImagesHelper.Images64;
            var imageIndex = ImagesHelper.GetIndex64(itemGroup._Icon, k=> ImagesHelper.IconLoader(_storage, k)) ?? -1;
            var perLine = 6;
            var dim = 80;

            var button = AddButton(holdedBy, perLine, dim, images, imageIndex, toolTip, 1);
            
            var panelControl = new PanelControl() { BorderStyle = BorderStyles.NoBorder, Dock = DockStyle.Fill, Tag = new Point(0, 0) };
            button.Tag = panelControl;
            button.Click += (sender, args) =>
            {
                splitContainerControl1.Panel2.Controls.Clear();
                splitContainerControl1.Panel2.Controls.Add(panelControl);
            };

            return button;
        }

        private Control Add(Control holdedBy, Recipe recipe, int groupIndex)
        {
            var toolTip = recipe.LocalizedName;
            var images = ImagesHelper.Images32;

            var imageIndex = recipe.ImageIndex32();
           
            var perLine = 10;
            var dim = 40;

            var button = AddButton(holdedBy, perLine, dim, images, imageIndex, toolTip, groupIndex+100);
            button.Tag = new PanelControl() { BorderStyle = BorderStyles.NoBorder, Dock = DockStyle.Fill, Tag = new Point(0, 0) };
            return button;
        }
    }


    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class IconInfo
    {
        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("tint")]
        public TintInfo Tint { get; set; }

        [JsonProperty("scale")]
        public double? Scale { get; set; }

        [JsonProperty("shift")]
        public double[] Shift { get; set; }

        public override string ToString()
        {
            var parts = new string[]
            {
                $"[{Icon}]",
                Tint?.ToString(),
                ScaleToString(),
                ShiftToString()
            };

            return $"{string.Join(", ", parts.Where(x => !string.IsNullOrEmpty(x)))}";
        }

        private string ShiftToString()
        {
            return Shift?.Length != 2 ? "" : $"shift: [{Shift[0]},{Shift[1]}]";
        }

        private string ScaleToString()
        {
            return Scale.HasValue ? $"scale: [{Scale}]" : "";
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class TintInfo
    {
        [JsonProperty("a")]
        public double A { get; set; }

        [JsonProperty("r")]
        public double R { get; set; }

        [JsonProperty("g")]
        public double G { get; set; }

        [JsonProperty("b")]
        public double B { get; set; }

        public Color Transform(Color color)
        {
            var a = Convert.ToByte(Math.Round(color.A * A));
            var r = Convert.ToByte(Math.Round(color.R * R));
            var g = Convert.ToByte(Math.Round(color.G * G));
            var b = Convert.ToByte(Math.Round(color.B * B));

            return a == 0 ? Color.Transparent : Color.FromArgb(a,r,g,b);
        }

        public override string ToString()
        {
            return $"tint: [a={A}, r={R}, g={G}, b={B}]";
        }
    }
}
