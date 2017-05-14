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
using Protoplasm.Utils.Graph;

namespace Factorio.Lua.Reader
{
    public partial class RecipiesControl : UserControl
    {
        private readonly Storage _storage;

        public RecipiesControl()
        {
            InitializeComponent();
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

            foreach (var g1 in groupBy.OrderBy(x => x.Key._Order))
            {
                ItemGroup itemGroup = g1.Key;
                var itemGroupControl = Add(splitContainerControl1.Panel1, itemGroup);

                foreach (var g2 in g1.OrderBy(x => x.Key.Order))
                {
                    ItemSubGroup itemSubGroup = g2.Key;

                    foreach (var recipe in g2.OrderBy(x => x._Order))
                    {
                        Add((Control)itemGroupControl.Tag, recipe);
                    }

                    Point point = (Point) ((Control) itemGroupControl.Tag).Tag;
                    if(point.X != 0)
                        ((Control)itemGroupControl.Tag).Tag = new Point(0, point.Y+1);
                }
            }
        }

        private static Control AddButton(Control holdedBy, int perLine, int dim, ImageCollection images, int imageIndex, string toolTip)
        {
            Point point = (Point)holdedBy.Tag;
            var count = point.X + point.Y * perLine;
            int i;
            var j = Math.DivRem(count, perLine, out i);


            Point location = new Point(4 + i * dim, 4 + j * dim);

            count++;
            j = Math.DivRem(count, perLine, out i);
            holdedBy.Tag = new Point(i, j);


            var button = new SimpleButton
            {
                Size = new Size(dim-6, dim-6),
                ImageList = images,
                ImageIndex = imageIndex,
                ImageLocation = ImageLocation.MiddleCenter,
                AllowGlyphSkinning = DefaultBoolean.False,
                Location = location,
                AllowFocus = false,
                ToolTip = toolTip
            };


            holdedBy.Controls.Add(button);
            return button;
        }
        private Image IconLoader(string key)
        {
            return ImagesHelper.LoadImage(_storage, key);
        }


        private Control Add(Control holdedBy, ItemGroup itemGroup)
        {
            var toolTip = itemGroup.LocalizedName;
            var images = ImagesHelper.Images64;
            var imageIndex = ImagesHelper.GetIndex64(itemGroup._Icon, IconLoader);
            var perLine = 6;
            var dim = 80;

            var button = AddButton(holdedBy, perLine, dim, images, imageIndex, toolTip);
            var panelControl = new PanelControl() { BorderStyle = BorderStyles.NoBorder, Dock = DockStyle.Fill, Tag = new Point(0, 0) };
            button.Tag = panelControl;
            button.Click += (sender, args) =>
            {
                splitContainerControl1.Panel2.Controls.Clear();
                splitContainerControl1.Panel2.Controls.Add(panelControl);
            };

            return button;
        }

        private Control Add(Control holdedBy, Recipe recipe)
        {
            var toolTip = recipe.LocalizedName;
            var images = ImagesHelper.Images32;
            var imageIndex = ImagesHelper.GetIndex32(recipe.Icon, IconLoader);
            var perLine = 10;
            var dim = 40;

            var button = AddButton(holdedBy, perLine, dim, images, imageIndex, toolTip);
            button.Tag = new PanelControl() { BorderStyle = BorderStyles.NoBorder, Dock = DockStyle.Fill, Tag = new Point(0, 0) };
            return button;
        }

    }
}
