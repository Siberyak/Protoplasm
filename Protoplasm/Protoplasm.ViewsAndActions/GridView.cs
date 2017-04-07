using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Data;
using System.Drawing.Design;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.Data;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;


namespace Protoplasm.ViewsAndActions
{
    public partial class GridView : BaseView
    {
        public GridView()
        {
            InitializeComponent();

            _gridView.OptionsBehavior.Editable = false;

            //_gridView.FocusedRowChanged += (sender, args) => Console.WriteLine($"-> [{sender.GetHashCode()}].FocusedRowChanged");
            //_gridView.FocusedRowObjectChanged += (sender, args) => Console.WriteLine($"-> [{sender.GetHashCode()}].FocusedRowObjectChanged");
            //_gridView.ColumnFilterChanged += (sender, args) => Console.WriteLine($"-> [{sender.GetHashCode()}].ColumnFilterChanged");

            //_gridView.FocusedRowChanged += _gridViewFocusedRowChanged;
            _gridView.FocusedRowObjectChanged += _gridViewFocusedRowObjectChanged;
        }

        private void _gridViewFocusedRowObjectChanged(object sender, FocusedRowObjectChangedEventArgs e)
        {
            OnSelectionChanged();
        }

        private void _gridViewFocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            OnSelectionChanged();
        }

        protected override void OnInitEnd()
        {
            OnSelectionChanged();
        }

        public override object Selection => FocusedRowData<object>();

        protected override object GetDatasource()
        {
            return _grid.DataSource;
        }

        protected override void SetDatasource(object datasource)
        {
            if (ResetColumns)
                _gridView.Columns.Clear();

            _grid.DataSource = null;

            if (datasource == null)
                return;

            _grid.DataSource = datasource;
            if (_gridView.Columns.Count == 0)
            {
                _gridView.PopulateColumns();
            }

            //if(!_gridView.OptionsView.ShowFooter)
            //{
            //    _gridView.OptionsView.ShowFooter = true;
            //    if (_gridView.Columns.Count != 0)
            //    {
            //        var column = _gridView.Columns.First();
            //        _gridView.GroupSummary.Add(SummaryItemType.Count, column.FieldName, column);
            //        column.Summary.Add(SummaryItemType.Count);
            //    }
            //}

            _grid.RefreshDataSource();
        }

        public bool ResetColumns { get; set; } = false;

        public T FocusedRowData<T>()
        {
            var handle = _gridView.FocusedRowHandle;
            if (!_gridView.IsDataRow(handle))
                return default(T);

            var index = _gridView.GetDataSourceRowIndex(handle);
            var item = ((IList)_grid.DataSource)[index];
            return item is T ? (T)item : default(T);
        }

        
    }


}
