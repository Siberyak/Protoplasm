using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraSplashScreen;

namespace Factorio.Lua.Reader
{
    public static class ViewsExtender
    {
        public static bool SelectResult<TControl>(Func<Form, TControl, bool> processResult, Action<Form, TControl> onFormLoad = null)
            where TControl : Control, ISelectorView, new()
        {
            var form = new XtraForm();
            var control = new TControl { Dock = DockStyle.Fill };
            control.Selected += SetOkDialogesult;

            form.Controls.Add(control);

            form.Load += (sender, args) =>
            {
                onFormLoad?.Invoke(form, control);
                ShowProgress(control.AfterLoad);
            };

            var dialogResult = form.ShowDialog();
            return dialogResult == DialogResult.OK && processResult(form, control);
        }

        private static void ShowProgress(Action action)
        {
            SplashScreenManager.ShowDefaultWaitForm();
            action();
            SplashScreenManager.CloseDefaultWaitForm();
        }

        public static bool SelectResult<TControl, TResult>(out TResult result, Action<Form, TControl> onFormLoad = null)
            where TControl : Control, ISelectorView<TResult>, new()
        {
            
            var localResult = default(TResult);

            Func<Form, TControl, bool> pr = (form, control) => {localResult = control.Selection;return true;};

            var selectResult = SelectResult(pr, onFormLoad);

            result = selectResult ? localResult : default(TResult);

            return selectResult;
        }

        public static bool SelectResult<TControl, TResult>(out TResult result, Size? minimumSize)
            where TControl : Control, ISelectorView<TResult>, new()
        {
            Action<Form, TControl> init = (form, control) =>
            {
                if (minimumSize.HasValue)
                    form.MinimumSize = minimumSize.Value;
            };

            return SelectResult(out result, init);
        }


        private static void SetOkDialogesult(object sender, EventArgs e)
        {
            ((Form)((Control)sender).Parent).DialogResult = DialogResult.OK;
        }


        public static T FocusedRowData<T>(this ColumnView view)
        {
            if (view == null)
                return default(T);

            var handle = view.FocusedRowHandle;
            return RowData<T>(view, handle);
        }

        public static T RowData<T>(this ColumnView view, int handle)
        {
            if (view == null)
                return default(T);

            if (!view.IsDataRow(handle))
                return default(T);

            var index = view.GetDataSourceRowIndex(handle);
            var item = ((IList)view.GridControl.DataSource)[index];
            return item is T ? (T)item : default(T);
        }
    }
}