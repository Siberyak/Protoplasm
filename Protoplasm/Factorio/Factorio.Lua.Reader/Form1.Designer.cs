namespace Factorio.Lua.Reader
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.treeList1 = new DevExpress.XtraTreeList.TreeList();
            this.treeListColumn1 = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this._repoItemImageComboBox = new DevExpress.XtraEditors.Repository.RepositoryItemImageComboBox();
            this.propertyGridControl1 = new DevExpress.XtraVerticalGrid.PropertyGridControl();
            this._tabControl = new DevExpress.XtraTab.XtraTabControl();
            this._treePage = new DevExpress.XtraTab.XtraTabPage();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this._calcPage = new DevExpress.XtraTab.XtraTabPage();
            this._grid = new DevExpress.XtraGrid.GridControl();
            this._gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            ((System.ComponentModel.ISupportInitialize)(this.treeList1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._repoItemImageComboBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.propertyGridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._tabControl)).BeginInit();
            this._tabControl.SuspendLayout();
            this._treePage.SuspendLayout();
            this._calcPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._grid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._gridView)).BeginInit();
            this.SuspendLayout();
            // 
            // treeList1
            // 
            this.treeList1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treeList1.Columns.AddRange(new DevExpress.XtraTreeList.Columns.TreeListColumn[] {
            this.treeListColumn1});
            this.treeList1.Location = new System.Drawing.Point(3, 41);
            this.treeList1.Name = "treeList1";
            this.treeList1.OptionsBehavior.Editable = false;
            this.treeList1.OptionsFilter.FilterMode = DevExpress.XtraTreeList.FilterMode.Extended;
            this.treeList1.OptionsFilter.ShowAllValuesInFilterPopup = true;
            this.treeList1.OptionsFind.AllowFindPanel = true;
            this.treeList1.OptionsView.HeaderFilterButtonShowMode = DevExpress.XtraEditors.Controls.FilterButtonShowMode.Button;
            this.treeList1.OptionsView.ShowAutoFilterRow = true;
            this.treeList1.OptionsView.ShowFilterPanelMode = DevExpress.XtraTreeList.ShowFilterPanelMode.ShowAlways;
            this.treeList1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this._repoItemImageComboBox});
            this.treeList1.Size = new System.Drawing.Size(397, 364);
            this.treeList1.TabIndex = 1;
            this.treeList1.FocusedNodeChanged += new DevExpress.XtraTreeList.FocusedNodeChangedEventHandler(this.treeList1_FocusedNodeChanged);
            // 
            // treeListColumn1
            // 
            this.treeListColumn1.Caption = "Name";
            this.treeListColumn1.FieldName = "Name";
            this.treeListColumn1.FilterMode = DevExpress.XtraGrid.ColumnFilterMode.DisplayText;
            this.treeListColumn1.Name = "treeListColumn1";
            this.treeListColumn1.Visible = true;
            this.treeListColumn1.VisibleIndex = 0;
            // 
            // _repoItemImageComboBox
            // 
            this._repoItemImageComboBox.AutoHeight = false;
            this._repoItemImageComboBox.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this._repoItemImageComboBox.Name = "_repoItemImageComboBox";
            // 
            // propertyGridControl1
            // 
            this.propertyGridControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGridControl1.AutoGenerateRows = true;
            this.propertyGridControl1.Location = new System.Drawing.Point(406, 3);
            this.propertyGridControl1.Name = "propertyGridControl1";
            this.propertyGridControl1.OptionsBehavior.Editable = false;
            this.propertyGridControl1.SelectedObject = this;
            this.propertyGridControl1.Size = new System.Drawing.Size(491, 402);
            this.propertyGridControl1.TabIndex = 2;
            // 
            // _tabControl
            // 
            this._tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tabControl.Location = new System.Drawing.Point(0, 0);
            this._tabControl.Name = "_tabControl";
            this._tabControl.SelectedTabPage = this._treePage;
            this._tabControl.Size = new System.Drawing.Size(910, 440);
            this._tabControl.TabIndex = 3;
            this._tabControl.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this._treePage,
            this._calcPage});
            // 
            // _treePage
            // 
            this._treePage.Controls.Add(this.simpleButton1);
            this._treePage.Controls.Add(this.treeList1);
            this._treePage.Controls.Add(this.propertyGridControl1);
            this._treePage.Name = "_treePage";
            this._treePage.Size = new System.Drawing.Size(904, 412);
            this._treePage.Text = "Tree";
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(3, 3);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(129, 32);
            this.simpleButton1.TabIndex = 3;
            this.simpleButton1.Text = "Calculate Production";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // _calcPage
            // 
            this._calcPage.Controls.Add(this._grid);
            this._calcPage.Name = "_calcPage";
            this._calcPage.Size = new System.Drawing.Size(904, 412);
            this._calcPage.Text = "Calc";
            // 
            // _grid
            // 
            this._grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._grid.Location = new System.Drawing.Point(47, 41);
            this._grid.MainView = this._gridView;
            this._grid.Name = "_grid";
            this._grid.Size = new System.Drawing.Size(773, 329);
            this._grid.TabIndex = 0;
            this._grid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this._gridView});
            // 
            // _gridView
            // 
            this._gridView.GridControl = this._grid;
            this._gridView.Name = "_gridView";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(910, 440);
            this.Controls.Add(this._tabControl);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.treeList1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._repoItemImageComboBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.propertyGridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._tabControl)).EndInit();
            this._tabControl.ResumeLayout(false);
            this._treePage.ResumeLayout(false);
            this._calcPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._grid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._gridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DevExpress.XtraTreeList.TreeList treeList1;
        private DevExpress.XtraTreeList.Columns.TreeListColumn treeListColumn1;
        private DevExpress.XtraVerticalGrid.PropertyGridControl propertyGridControl1;
        private DevExpress.XtraTab.XtraTabControl _tabControl;
        private DevExpress.XtraTab.XtraTabPage _treePage;
        private DevExpress.XtraTab.XtraTabPage _calcPage;
        private DevExpress.XtraGrid.GridControl _grid;
        private DevExpress.XtraGrid.Views.Grid.GridView _gridView;
        private DevExpress.XtraEditors.Repository.RepositoryItemImageComboBox _repoItemImageComboBox;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
    }
}