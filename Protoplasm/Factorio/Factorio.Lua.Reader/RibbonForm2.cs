using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Layout.Events;
using Factorio.Lua.Reader.Views;

namespace Factorio.Lua.Reader
{
    public partial class RibbonForm2 : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private Storage _storage => Storage.Current;

        Calculation _calculation = new Calculation();

        public RibbonForm2()
        {
            InitializeComponent();

            _craftsGrid.DataSource = _calculation.Crafts;
            _partsGrid.DataSource = _calculation.Parts;
            _craftsView.CardCaptionFormat = "{0}";
            _craftsView.CustomCardCaptionImage += (sender, args) => args.Image = _craftsView.RowData<CraftInfo>(args.RowHandle).Image;
            _craftsView.FocusedRowChanged += CraftsRowChanged;

            _partsView.CardCaptionFormat = "{1}";
            _partsView.CustomCardCaptionImage += (sender, args) => args.Image = _partsView.RowData<Part>(args.RowHandle).Image;
            _partsView.FocusedRowChanged += PartsRowChanged;

            //_craftsView.Card
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
        }

        private void barButtonItem1_ItemClick(object sender, ItemClickEventArgs e)
        {
            var recipe = RecipiesControl.SelectRecipe();
            if(recipe == null)
                return;


            _calculation.Add(recipe);
        }



        private void barButtonItem2_ItemClick(object sender, ItemClickEventArgs e)
        {
            new Form1().Show();
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
            view.Recipe = craftInfo._recipe;
            view.Crafter = craftInfo.Crafter;
        }

        
    }
}