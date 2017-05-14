using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;

namespace Factorio.Lua.Reader
{
    public partial class RecipeView : XtraUserControl
    {
        private Recipe _recipe;

        public RecipeView()
        {
            InitializeComponent();
            _layoutControl.Root.Items.Clear();
            _layoutControl.Images = ImagesHelper.Images32;
        }

        public RecipeView(Recipe recipe)
        {
            Recipe = recipe;
        }

        public Recipe Recipe
        {
            get { return _recipe; }
            set
            {
                if(_recipe == value)
                    return;

                Reset();

                _recipe = value;

                Init();
            }
        }

        private void Reset()
        {
            if (DesignMode)
                return;

            if (Recipe == null)
                return;

            foreach (var item in _layoutControl.Root.Items.OfType<SimpleLabelItem>())
            {
                item.Click -= InputLabelClick;
                item.Click -= OutputLabelClick;
            }

            _layoutControl.Root.Items.Clear();

        }

        private void InputLabelClick(object sender, EventArgs e)
        {
            
        }

        private void OutputLabelClick(object sender, EventArgs e)
        {
            
        }

        private void Init()
        {
            if (DesignMode)
                return;

            if(Recipe == null)
                return;

            _layoutControl.BeginInit();

            AddLabel(Recipe.LocalizedName, imageIndex: Recipe.ImageIndex32());
            AddSeparator();

            AddLabel($"{Recipe._EnergyRequired}", imageIndex: ImagesHelper.GetIndex32(Recipe.ClockIcon, k => ImagesHelper.LoadImage(Recipe.Storage, k)));

            var parts = Recipe.Parts;
            foreach (var edge in parts.Where(x => x.Direction == Direction.Input))
            {
                var text = $" x {edge.Amount}   {edge.Part.LocalizedName}";
                var imageIndex = ImagesHelper.GetIndex32(edge.Part._Icon, k => ImagesHelper.LoadImage(Recipe.Storage, k));
                var item = AddLabel(text, imageIndex);
                item.Click += InputLabelClick;
            }

            AddSeparator();

            var outs = parts.Where(x => x.Direction == Direction.Output).ToArray();
            if (outs.Length > 1)
            {
                foreach (var edge in outs)
                {
                    var text = $" x {edge.Amount}   {edge.Part.LocalizedName}";
                    var imageIndex = ImagesHelper.GetIndex32(edge.Part._Icon, k => ImagesHelper.LoadImage(Recipe.Storage, k));
                    var item = AddLabel(text, imageIndex);
                    item.Click += OutputLabelClick;
                }

                AddSeparator();
            }

            var crafters = Recipe.RecipeCategory.References.OfType<RecipeCategoryCrafterEdge>().Select(x => x.Crafter).ToArray();
            if(crafters.Length > 0)
            {
                foreach (var crafter in crafters)
                {
                    var imageIndex = ImagesHelper.GetIndex32(crafter._Icon, k => ImagesHelper.LoadImage(Recipe.Storage, k));
                    AddLabel(crafter.LocalizedName, imageIndex);
                }

                AddSeparator();
            }

            //AddLabel(Recipe.RecipeCategory.Name);

            TerminateLayout();
            _layoutControl.EndInit();
        }

        private void TerminateLayout()
        {
            _layoutControl.Root.Add(new EmptySpaceItem() {Location = new Point(0, _layoutControl.Height - 10)});
        }
        private void AddSeparator()
        {
            _layoutControl.Root.Add(new SimpleSeparator() { Location = NextLocation() });
        }
        private void AddGroup()
        {
            //_layoutControl.Root.Add(new () { Location = NextLocation() });
        }

        private SimpleLabelItem AddLabel(string text, int? imageIndex = null, Color? foreColor = null)
        {
            var item = new SimpleLabelItem();
            ((ISupportInitialize) item).BeginInit();
            item.AllowHotTrack = false;

            if (foreColor.HasValue)
            {
                item.AppearanceItemCaption.ForeColor = foreColor.Value;
                item.AppearanceItemCaption.Options.UseForeColor = true;
            }


            var location = NextLocation();

            item.Location = location;

            item.Name = "simpleLabelItem2";
            item.Text = text;
            item.ImageIndex = imageIndex ?? -1;

            _layoutControl.Root.Add(item);

            ((ISupportInitialize) (item)).EndInit();
            return item;
        }

        private Point NextLocation()
        {
            var lastItem = _layoutControl.Items.LastOrDefault();

            var location = Point.Empty;

            if (lastItem != null)
            {
                location.Offset(0, lastItem.Location.Y + lastItem.Height);
            }
            return location;
        }
    }
}
