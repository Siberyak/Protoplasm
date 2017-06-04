using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;

namespace Factorio.Lua.Reader
{
    class TimerButton : CraftInfoButton
    {
        public TimerButton(CraftInfo craftInfo) : base(craftInfo)
        {
            Size = new Size(DefaultWidth, DefaultHeight);
            ImageOptions.Location = ImageLocation.BottomCenter;
            ImageOptions.ImageList = ImagesHelper.x16.Images;
            ImageOptions.ImageIndex = ImagesHelper.x16.GetIndex(Recipe.ClockIcon);
            ButtonTextDelegate = TimeButtonText;
            Appearance.TextOptions.WordWrap = WordWrap.Wrap;
        }

        private static string TimeButtonText(CraftInfoButton b)
        {
            var text = "";

            var byRecipe = $"{b.CraftInfo.RecipeTime}";
            var byCrafter = b.CraftInfo.CraftTime.HasValue
                ? $"{Math.Round(b.CraftInfo.CraftTime.Value, 2)}"
                : "";

            var mode = b.CraftInfo.Mode;

            text = (mode | CraftInfo.ViewMode.Recipe) == mode 
                ? $"<b>{byRecipe}</b>" 
                : $"<size=-2>{byRecipe}</size>";


            if(b.CraftInfo.Crafter != null)
            {
                text += "<br>";
                text += (mode | CraftInfo.ViewMode.Crafter) == mode
                    ? $"<b>{byCrafter}</b>" : 
                    $"<size=-2>{byCrafter}</size>";
            }

            text += "<br><size=-2>sec.</size>";

            return text;
        }

        protected override void OnPaint(PaintEventArgs e)
        {

            base.OnPaint(e);

            if (!CraftInfo.Enabled)
                return;

            var mode = CraftInfo.Mode;
            
            if ((mode | CraftInfo.ViewMode.Recipe) == mode)
            {
                var color = Color.Blue;
                color = Color.FromArgb(48, color.R, color.G, color.B);
                e.Graphics.FillRectangle(new SolidBrush(color), e.ClipRectangle);
                e.Graphics.DrawString("R", Font, Brushes.Blue, 3, Height - 20 + 3);
            }
            else if ((mode | CraftInfo.ViewMode.Crafter) == mode)
            {
                var color = Color.Green;
                color = Color.FromArgb(48, color.R, color.G, color.B);
                e.Graphics.FillRectangle(new SolidBrush(color), e.ClipRectangle);
                e.Graphics.DrawString("C", Font, Brushes.Green, 3, Height - 20 + 3);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            if(CraftInfo.Crafter == null)
                return;

            var mode = CraftInfo.Mode;

            if ((mode | CraftInfo.ViewMode.Recipe) == mode)
                mode = (mode & ~CraftInfo.ViewMode.Recipe) | CraftInfo.ViewMode.Crafter;
            else if ((mode | CraftInfo.ViewMode.Crafter) == mode)
                mode = (mode & ~CraftInfo.ViewMode.Crafter) | CraftInfo.ViewMode.Recipe;
            else
                mode |= CraftInfo.ViewMode.Recipe;

            CraftInfo.Mode = mode;

            base.OnClick(e);
        }
    }
}