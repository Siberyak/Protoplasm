using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using DevExpress.XtraLayout.Utils;

namespace Factorio.Lua.Reader.Views
{
    public partial class CraftersView : LayoutedView, ISelectorView<ICrafter>
    {
        public CraftersView()
        {
            InitializeComponent();
            LayoutControl.Images = ImagesHelper.Images32;
        }

        public ICrafter Selection => Crafter;
        public Recipe Recipe { get; set; }
        public ICrafter Crafter { get; set; }

        protected override void AfterLoad()
        {
            base.AfterLoad();

            //LayoutControl.BeginInit();

            foreach (var crafter in Recipe.RecipeCategory.Crafters)
            {
                this.AddControl<CheckButton>((item, button) => InitButton(item, button, crafter, crafter.ImageIndex32()));
            }

            //var item1 = this.AddControl<CheckButton>((item, button) => InitButton(item, button, Recipe, Recipe.ImageIndex32()));
            //var item2 = this.AddControl<CheckButton>((item, button) => InitButton(item, button, Recipe, Recipe.ImageIndex32()));

            //this.AddSeparator();

            //item2.Move(item1, InsertType.Right);

            this.TerminateLayout();
            //LayoutControl.EndInit();
            //LayoutControl.Update();
        }

        private void InitButton<T>(LayoutControlItem item, CheckButton button, T data, int imageIndex)
            where T : ICrafter
        {
            var size = new Size(240, 36);

            item.TextVisible = false;
            //item.MaxSize = size;
            //item.MinSize = size;

            //item.Text = data.LocalizedName;
            //item.TextLocation = Locations.Right;
            //item.TextToControlDistance = 5;

            button.ToolTip = data.LocalizedName;

            button.Size = size;
            button.MaximumSize = size;
            button.MinimumSize = size;

            if (Equals(data, Crafter))
                button.Toggle();

            button.ImageList = ImagesHelper.Images32;
            button.ImageIndex = imageIndex;
            button.ImageLocation = ImageLocation.MiddleLeft;
            button.AllowGlyphSkinning = DefaultBoolean.False;
            button.AllowFocus = false;
            button.ToolTip = data.LocalizedName;
            button.GroupIndex = 1;
            button.Click += OnButtonClick;
            button.DoubleClick += OnButtonDoubleClick;
            button.Tag = data;
            button.Text = $"<b>{data.LocalizedName}</b><br>x {data._CraftingSpeed}";
            button.AllowHtmlDraw = DefaultBoolean.True;
            button.Appearance.TextOptions.WordWrap = WordWrap.Wrap;
            button.Appearance.TextOptions.HAlignment = HorzAlignment.Near;
        }

        private void OnButtonClick(object sender, EventArgs e)
        {
            Crafter = ((Control)sender).Tag as ICrafter;
        }

        private void OnButtonDoubleClick(object sender, EventArgs e)
        {
            OnButtonClick(sender, e);
            OnSelected();
        }
    }
}
