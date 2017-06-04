using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ColorsMixer;
using DevExpress.Skins;
using DevExpress.Utils.Drawing;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Drawing;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Card;
using DevExpress.XtraGrid.Views.Card.ViewInfo;
using DevExpress.XtraGrid.Views.Layout.Events;
using Factorio.Lua.Reader.Controls;
using Factorio.Lua.Reader.Views;
using Newtonsoft.Json;
using Protoplasm.Collections;
using Protoplasm.Graph;
using System.Drawing;
using Color = ColorsMixer.Color;

namespace Factorio.Lua.Reader
{
    public partial class RibbonForm2 : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private Storage _storage => Storage.Current;

        Calculation _calculation = new Calculation();
        private string _json;

        public RibbonForm2()
        {
            InitializeComponent();

            _craftsGrid.DataSource = _calculation.Crafts;
            _partsGrid.DataSource = new PredicatedList<Part>(_calculation.Parts, x => x.Enabled);
            _craftsView.FocusedRowChanged += CraftsRowChanged;

            _partsView.CardCaptionFormat = "{1}";
            _partsView.CustomCardCaptionImage += (sender, args) => args.Image = _partsView.RowData<Part>(args.RowHandle).Image;
            _partsView.FocusedRowChanged += PartsRowChanged;

            _partsGrid.ContextMenuStrip = new ContextMenuStrip();
            _partsGrid.ContextMenuStrip.Opening += PartsContextMenuStripOpening;


            _calculation.Crafts.BeforeRemoveItem += BeforeRemoveCraft;
            _calculation.Crafts.ListChanged += CraftsListChanged;
        }

        private void BeforeRemoveCraft(object sender, BeforeRemoveEventArgs e)
        {
            var craftInfo = ((IList<CraftInfo>)sender)[e.Index];
            var card = _viewPort.Controls.OfType<CraftCard>().FirstOrDefault(x => x.CraftInfo == craftInfo);
            if(card == null)
                return;

            card.Reset();
            _viewPort.Controls.Remove(card);
            card.Dispose();
        }

        private void CraftsListChanged(object sender, ListChangedEventArgs e)
        {
            if(e.ListChangedType != ListChangedType.ItemAdded)
                return;

            var craftInfo = ((IList<CraftInfo>) sender)[e.NewIndex];
            var card = new CraftCard();
            _viewPort.Controls.Add(card);
            card.BringToFront();
            Application.DoEvents();
            card.CraftInfo = craftInfo;

            if(card.Location.IsEmpty)
                card.Location = new Point(100, 100);
        }

        private void PartsContextMenuStripOpening(object sender, CancelEventArgs e)
        {
            var strip = _partsGrid.ContextMenuStrip;
            strip.Items.Clear();
            e.Cancel = !_partsView.IsDataRow(_partsView.FocusedRowHandle);
            if(e.Cancel)
                return;

            var part = _partsView.RowData<Part>(_partsView.FocusedRowHandle);
            var recipies = ((INode) part._part).BackReferences
                .OfType<RecipePartEdge>()
                .Where(x => x.Direction == Direction.Output)
                .Select(x => x.Recipe)
                .Distinct()
                .ToArray();

            e.Cancel = recipies.Length == 0;
            if (e.Cancel)
                return;

            foreach (var recipe in recipies)
            {
                strip.Items.Add(recipe.LocalizedName, recipe.Image32());
            }

        }


        private void CraftsCustomDrawCardCaption(object sender, CardCaptionCustomDrawEventArgs e)
        {
            var cardInfo = e.CardInfo as CardInfo;
            if(cardInfo == null)
                return;
            cardInfo.CaptionInfo.PaintAppearance.BackColor = System.Drawing.Color.Aqua;

            var color = e.Appearance.BackColor;
            var fromArgb = Color.FromArgb(128, 0, 255, 0);

            var backColor1 = fromArgb.Mix(color);
            var backColor2 = color.Mix(fromArgb);

            color = color.ApplyAlfa(128);
            var backColor3 = fromArgb.Mix(color);
            var backColor4 = color.Mix(fromArgb);

            e.Appearance.ForeColor = System.Drawing.Color.Aqua;

            e.DefaultDraw();

            e.Handled = true;
        }

        private void PartsRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            var part = _partsView.FocusedRowData<Part>();
            _calculation.CurrentPart = part;
        }

        private void CraftsRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            var craftInfo = _craftsView.FocusedRowData<CraftInfo>();
            _calculation.CurrentCraftInfo = craftInfo;
            _setCrafterButtonItem.Enabled = craftInfo != null && craftInfo.Crafter == null;
            //craftInfoCard1.CraftInfo = craftInfo;
        }

        private void barButtonItem1_ItemClick(object sender, ItemClickEventArgs e)
        {
            var recipe = RecipiesControl.SelectRecipe();
            if(recipe == null)
                return;


            _calculation.Add(recipe);
        }




        private void _setCrafterButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            var craftInfo = _craftsView.FocusedRowData<CraftInfo>();
            if(craftInfo == null)
                return;

            ICrafter crafter;
            if(!ViewsExtender.SelectResult<CraftersView, ICrafter>(out crafter, (f,v) =>  InitSelectorView(f,v, craftInfo)))
                return;

            craftInfo.Crafter = crafter;
        }

        private void InitSelectorView(Form form, CraftersView view, CraftInfo craftInfo)
        {
            view.Recipe = craftInfo.Recipe;
            view.Crafter = craftInfo.Crafter;
        }

        private void barButtonItem2_ItemClick(object sender, ItemClickEventArgs e)
        {
            new Form1().Show();
        }
        private void barButtonItem3_ItemClick(object sender, ItemClickEventArgs e)
        {
            new Form2().ShowDialog();
        }

        private void barButtonItem4_ItemClick(object sender, ItemClickEventArgs e)
        {
            foreach (var mc in splitContainerControl1.Panel2.Controls.OfType<MovableContainer>().ToArray())
            {
                mc.MovingMode = !mc.MovingMode;
                Application.DoEvents();
            }
        }


        private void imagedPanel1_Load(object sender, EventArgs e)
        {

        }

        private void barButtonItem5_ItemClick(object sender, ItemClickEventArgs e)
        {
            _calculation.Save();
            
        }

        private void barButtonItem6_ItemClick(object sender, ItemClickEventArgs e)
        {
            _viewPort.SuspendLayout();
            _viewPort.Visible = false;

            _calculation.Reload();

            _viewPort.ResumeLayout();
            _viewPort.Visible = true;
        }

        private void barButtonItem7_ItemClick(object sender, ItemClickEventArgs e)
        {
            _viewPort.SuspendLayout();
            _viewPort.Visible = false;

            Calculation.LoadFrom(_calculation, null);

            _viewPort.ResumeLayout();
            _viewPort.Visible = true;
        }

        private void barButtonItem8_ItemClick(object sender, ItemClickEventArgs e)
        {
            _calculation.SaveAs();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if(!_calculation.Changed)
                return;

            var dialogResult = XtraMessageBox.Show("сохранить данные перед выходом?", "выходим?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.Cancel)
                e.Cancel = true;

            if (dialogResult == DialogResult.Yes)
                _calculation.Save();

            base.OnFormClosing(e);
        }

        private void barButtonItem9_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        //private void labelControl1_Paint(object sender, PaintEventArgs e)
        //{
        //    using (GraphicsCache cache = new GraphicsCache(e.Graphics))
        //    {
        //        var matrix = new Matrix();
        //        matrix.Rotate(45);
                
        //        cache.TransformMatrix.RotateAt(45, PointF.Empty);
        //        labelControl1.Appearance.DrawString(cache, labelControl1.Text, e.ClipRectangle);
        //    }
        //}

    }

}