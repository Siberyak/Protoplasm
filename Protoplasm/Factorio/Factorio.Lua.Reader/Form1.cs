using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.Utils.Menu;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraTreeList;
using Protoplasm.Graph;

namespace Factorio.Lua.Reader
{
    public partial class Form1 : Form
    {
        private Storage _storage => Storage.Current;

        public Form1()
        {
            InitializeComponent();
        }

        void LoadData()
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadData();

            InitTree();

            _grid.DataSource = new PartsList(_storage.Nodes<IRecipePart>());
        }

        private void InitTree()
        {

            treeList1.DataSource = new Root(_storage);
            treeList1.StateImageList = ImagesHelper.Images32;
            treeList1.GetStateImage += GetStateImage;

            treeList1.PopupMenuShowing += PopupMenuShowing;
            treeList1.ColumnFilterChanged += ColumnFilterChanged;
        }

        private void ColumnFilterChanged(object sender, EventArgs e)
        {
            
        }

        private void PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            e.Menu.Items.Add(new DXMenuItem("Filter", ShowFilter));
        }

        private void ShowFilter(object sender, EventArgs e)
        {
            treeList1.ShowFilterEditor(treeListColumn1);
        }

        private void GetStateImage(object sender, GetStateImageEventArgs e)
        {
            var data = treeList1.GetDataRecordByNode(e.Node);
            e.NodeImageIndex = (data as IVirtualNode)?.IconIndex ?? -1;

        }

        private Image IconLoader(string key)
        {
            return ImagesHelper.LoadImage(_storage, key);
        }

        private void treeList1_FocusedNodeChanged(object sender, FocusedNodeChangedEventArgs e)
        {
            var nodeData = (treeList1.GetDataRecordByNode(treeList1.FocusedNode) as IVirtualNode)?.NodeData;
            propertyGridControl1.SelectedObject = nodeData;
            propertyGridControl1.UpdateRows();

            simpleButton1.Visible = (nodeData is IRecipePart);
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            //var form2 = new Form2();
            //var recipiesControl = new RecipiesControl(_storage) {Dock = DockStyle.Fill};
            //recipiesControl.Selected += RecipeSelected;
            //form2.Controls.Add(recipiesControl);
            //form2.ShowDialog();

            var recipe = RecipiesControl.SelectRecipe();

        }

        private void RecipeSelected(object sender, EventArgs e)
        {
            var control = (RecipiesControl) sender;
            var recipe = control.Recipe;
            ((Form)control.Parent).DialogResult = DialogResult.OK;
        }
    }


}
