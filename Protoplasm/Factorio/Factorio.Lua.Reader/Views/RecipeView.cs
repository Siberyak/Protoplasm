using System;
using System.Linq;
using DevExpress.XtraLayout;
using DevExpress.XtraLayout.Utils;

namespace Factorio.Lua.Reader
{
    public partial class RecipeView : LayoutedView
    {
        private Recipe _recipe;
        
        public RecipeView()
        {
            InitializeComponent();

            ClearItems(LayoutControl.Root.Items);
            ClearItems(LayoutControl.Items);

            LayoutControl.Images = ImagesHelper.Images32;
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

            ClearItems(LayoutControl.Root.Items);
            ClearItems(LayoutControl.Items);
        }

        private void ClearItems(BaseItemCollection items)
        {
            foreach (var item in items.OfType<SimpleLabelItem>())
            {
                item.Click -= InputLabelClick;
                item.Click -= OutputLabelClick;
            }

            items.Clear();
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

            var parts = Recipe.Parts;


            LayoutControl.BeginInit();

            LayoutControl.AddLabel(Recipe.LocalizedName, Recipe.ImageIndex32());



            LayoutControl.AddSeparator();

            var outs = parts.Where(x => x.Direction == Direction.Output).ToArray();
            if (outs.Length > 0)
            {
                foreach (var edge in outs)
                {
                    var text = $" x {edge.Amount}   {edge.Part.LocalizedName}";
                    var imageIndex = ImagesHelper.GetIndex32(edge.Part._Icon, k => ImagesHelper.LoadImage(Recipe.Storage, k));
                    var item = LayoutControl.AddLabel(text, imageIndex);
                    item.Click += OutputLabelClick;
                }

                LayoutControl.AddSeparator();
            }

            LayoutControl.AddLabel($"{Recipe._EnergyRequired}", ImagesHelper.GetIndex32(Recipe.ClockIcon, k => ImagesHelper.LoadImage(Recipe.Storage, k)));

            foreach (var edge in parts.Where(x => x.Direction == Direction.Input))
            {
                var text = $" x {edge.Amount}   {edge.Part.LocalizedName}";
                var imageIndex = ImagesHelper.GetIndex32(edge.Part._Icon, k => ImagesHelper.LoadImage(Recipe.Storage, k));
                var item = LayoutControl.AddLabel(text, imageIndex);
                item.Click += InputLabelClick;
            }

            LayoutControl.AddSeparator();

            

            var crafters = Recipe.RecipeCategory.References.OfType<RecipeCategoryCrafterEdge>().Select(x => x.Crafter).ToArray();
            if(crafters.Length > 0)
            {
                foreach (var crafter in crafters)
                {
                    var imageIndex = ImagesHelper.GetIndex32(crafter._Icon, k => ImagesHelper.LoadImage(Recipe.Storage, k));
                    LayoutControl.AddLabel(crafter.LocalizedName, imageIndex);
                }

                LayoutControl.AddSeparator();
            }

            //AddLabel(Recipe.RecipeCategory.Name);

            LayoutControl.TerminateLayout();
            LayoutControl.EndInit();
        }
    }
}
