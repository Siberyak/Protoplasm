#define NOTDESIGN

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Protoplasm.ViewsAndActions
{
    public

#if NOTDESIGN
abstract 
#endif

        partial class BaseView : UserControl
    {
        protected
#if NOTDESIGN
abstract 
#else
            virtual
#endif

            object GetDatasource()
#if NOTDESIGN
;
#else
        {
            return null;
        }
#endif

        protected
#if NOTDESIGN
abstract 
#else
            virtual
#endif
            void SetDatasource(object datasource)
#if NOTDESIGN
;
#else
        { }
#endif

        public virtual bool ShowCaption => true;


        private object _dataSource;
        protected ViewInfo _viewInfo;
        private bool _initing;

        protected bool Initing
        {
            get { return _initing; }
            private set
            {
                if(_initing == value)
                    return;
                _initing = value;
                try
                {
                    if (_initing)
                        OnInitStart();
                    else
                    {
                        OnInitEnd();
                        Application.DoEvents();
                    }
                }
                catch
                {
                }
            }
        }

        protected virtual void OnInitStart()
        {
            
        }
        protected virtual void OnInitEnd()
        {

        }

        protected BaseView()
        {
            InitializeComponent();
            
        }

        public ViewHelper ViewHelper { protected get; set; }

        public virtual object DataSource
        {
            get { return GetDatasource(); }
            protected set
            {
                if(Equals(DataSource, value))
                    return;

                InvokeAction(() => SetDatasource(value));
            }
        }

        public ViewInfo ViewInfo
        {
            get { return _viewInfo; }
            set
            {
                if(_viewInfo == value)
                    return;

                //if(_viewInfo != null)
                //    throw new NotSupportedException();

                if(value == null)
                    throw new ArgumentNullException(nameof(value));

                Initing = true;
                try
                {
                    _viewInfo = value;
                    DataSource = _viewInfo.DataSource;
                }
                finally
                {
                    Initing = false;
                }
            }
        }

        protected void InvokeAction(Action action)
        {
            if (InvokeRequired)
            {
                Invoke(action);
            }
            else
            {
                action();
            }
        }



        public event EventHandler SelectionChanged;

        protected virtual void OnSelectionChanged()
        {
            if(Initing)
                return;

            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public virtual object Selection => DataSource;

    }
}
