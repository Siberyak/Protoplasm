using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace Factorio.Lua.Reader.Controls
{
    public partial class CraftInfoCard : MovableContainer
    {
        public CraftInfoCard()
        {
            InitializeComponent();
            OnSizeChanged(EventArgs.Empty);
        }

        private CraftInfo _craftInfo;
        public CraftInfo CraftInfo
        {
            get { return _craftInfo; }
            set
            {
                if(_craftInfo == value)
                    return;


                if (_craftInfo != null)
                    _craftInfo.PropertyChanged -= CraftInfoPropertyChanged;

                _craftInfo = value;

                if (_craftInfo != null)
                    _craftInfo.PropertyChanged += CraftInfoPropertyChanged;

                UpdateContent();
            }
        }

        private void CraftInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateContent();
        }

        private void UpdateContent()
        {
            SuspendLayout();




            _recipePanel.Reset();
            _crafterPanel.Reset();
            _leftPanel.Controls.Clear();
            _rightPanel.Controls.Clear();


            if (CraftInfo == null)
                return;

            _recipePanel.Image = CraftInfo.Image;
            _recipePanel.LabelText = CraftInfo.Name;

            var crafter = CraftInfo.Crafter;
            if (crafter != null)
            {
                _crafterPanel.Image = crafter.Image32();
                _crafterPanel.LabelText = $"<b>{crafter.LocalizedName}</b><br>speed x{crafter._CraftingSpeed}";
            }

            var recipe = CraftInfo.Recipe;

            foreach (var edge in recipe.Parts)
            {
                Control control = edge.Direction == Direction.Input
                    ? _leftPanel
                    : _rightPanel;

                var part = edge.Part;
                var imagedPanel = new ImagedPanel
                {
                    Image = part.Image32(),
                    LabelText = $" x {edge.Amount} {part.LocalizedName}",
                    Location = new Point(0, control.Height)
                };


                control.Controls.Add(imagedPanel);
                imagedPanel.BringToFront();
                imagedPanel.Dock = DockStyle.Top;

            }


            ResumeLayout(true);
            //Refresh();
            //Invalidate();

        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            ResizePanels();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            ResizePanels();
        }

        private void ResizePanels()
        {
            if (_leftPanel == null)
                return;

            _leftPanel.MinimumSize = new Size(Size.Width / 2, 32);
            _rightPanel.MinimumSize = _leftPanel.MinimumSize;
            _rightPanel.Location = _leftPanel.Location + new Size(_leftPanel.Size.Width, 0);
        }
    }
}
