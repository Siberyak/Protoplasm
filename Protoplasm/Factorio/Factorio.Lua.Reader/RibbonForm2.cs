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
            _craftsView.CardCaptionFormat = "{1}";
            _craftsView.CustomCardCaptionImage += (sender, args) => args.Image = RowData<CraftInfo>(_craftsView, args.RowHandle).Image;
            _craftsView.FocusedRowChanged += CraftsRowChanged;

            _partsView.CardCaptionFormat = "{1}";
            _partsView.CustomCardCaptionImage += (sender, args) => args.Image = RowData<Part>(_partsView, args.RowHandle).Image;


            //_craftsView.Card
        }

        private void CraftsRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            var craftInfo = FocusedRowData<CraftInfo>(_craftsView);
            _setCrafterButtonItem.Enabled = craftInfo != null && craftInfo.Crafter == null;
        }

        private void barButtonItem1_ItemClick(object sender, ItemClickEventArgs e)
        {
            var recipe = RecipiesControl.SelectRecipe();
            if(recipe == null)
                return;


            _calculation.Add(recipe);
        }


        public static T FocusedRowData<T>(ColumnView view)
        {
            var handle = view.FocusedRowHandle;
            return RowData<T>(view, handle);
        }

        private static T RowData<T>(ColumnView view, int handle)
        {
            if (!view.IsDataRow(handle))
                return default(T);

            var index = view.GetDataSourceRowIndex(handle);
            var item = ((IList) view.GridControl.DataSource)[index];
            return item is T ? (T) item : default(T);
        }

        private void barButtonItem2_ItemClick(object sender, ItemClickEventArgs e)
        {
            new Form1().Show();
        }
    }
}