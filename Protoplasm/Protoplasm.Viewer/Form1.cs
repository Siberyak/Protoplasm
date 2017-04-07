using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Protoplasm.ViewsAndActions;

namespace Protoplasm.Viewer
{
    public partial class Form1 : Form
    {

        ModelViewController _mvc;
        ViewHelper _viewHelper;

        public Form1()
        {
            InitializeComponent();

            if (DesignMode)
                return;

            InitializeMVC();
        }

        public Form1(Action<ModelViewController, ViewHelper> initMVC, object datasource, object viewContext = null) : this()
        {
            initMVC(_mvc, _viewHelper);

            _viewHelper.Apply(this, datasource, viewContext);
        }

        private void InitializeMVC()
        {
            _mvc = new ModelViewController();

            _viewHelper = new ViewHelper(_mvc);

            _viewHelper.RegisterToken<GeneralViewToken, GridView>(GeneralViewToken.Grid);
            _viewHelper.RegisterToken<GeneralViewToken, PropertiesView>(GeneralViewToken.PropertiesGrid);

        }
    }
}
