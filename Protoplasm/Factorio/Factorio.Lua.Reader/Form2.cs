using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraNavBar;
using Factorio.Lua.Reader.Controls;

namespace Factorio.Lua.Reader
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            var splitGroupPanel = splitContainerControl1.Panel1;
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            var movableContainers = Controls.OfType<MovableContainer>().ToArray();
            foreach (var mc in movableContainers)
            {
                mc.MovingMode = !mc.MovingMode;
                Application.DoEvents();
            }
        }
    }

}
