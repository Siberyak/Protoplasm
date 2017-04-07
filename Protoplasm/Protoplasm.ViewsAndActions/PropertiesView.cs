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
    public partial class PropertiesView : BaseView
    {
        public PropertiesView()
        {
            InitializeComponent();
            if(DesignMode)
                return;

            _propertyGrid.OptionsBehavior.Editable = false;
        }

        protected override object GetDatasource()
        {
            return _propertyGrid.SelectedObject;
        }

        protected override void SetDatasource(object datasource)
        {
            _propertyGrid.SelectedObject = datasource;
        }
    }
}
