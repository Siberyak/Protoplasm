using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.XtraEditors;
using Protoplasm.Utils;

namespace Factorio.Lua.Reader
{
    public class CraftCard : CraftInfoButton
    {

        public CraftCard() : base(null)
        {
            AllowMoving = true;
        }

        protected override void OnClick(EventArgs e)
        {

            if (CraftInfo?.Crafter != null && (ModifierKeys == Keys.Control || ModifierKeys == Keys.ControlKey))
            {
                CraftInfo.Enabled = !CraftInfo.Enabled;
            }

            base.OnClick(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete)
            {
                BeginInvoke((Action)RequestDelete);
            }

            base.OnKeyUp(e);
        }

        private void RequestDelete()
        {
            Action request = () =>
            {
                if (XtraMessageBox.Show("Удалить?", "...", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    CraftInfo.OnDeleted();
            };

            Invoke(request);
        }

        public override string ButtonText => "";

        protected override void AfterCraftInfoChanged()
        {
            base.AfterCraftInfoChanged();
            if (CraftInfo == null)
                return;

            var binding = new Binding(nameof(Location), CraftInfo, nameof(CraftInfo.Location))
            {
                ControlUpdateMode = ControlUpdateMode.OnPropertyChanged,
                DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged
            };

            DataBindings.Add(binding);


            InitContent();
        }



        protected override void BeforeCraftInfoChanged()
        {
            base.BeforeCraftInfoChanged();

            if (CraftInfo == null)
            {
                return;
            }

            ResetContent();
        }


        private void InitContent()
        {
            InitSelf();
            InitButtons();
        }

        //protected override void OnClick(EventArgs e)
        //{
        //    CraftInfo.SetFocus();
        //    base.OnClick(e);
        //}

        private void InitButtons()
        {
            Height = DefaultHeight * 3 / 2;

            var parts = CraftInfo.Recipe.Parts.ToArray();
            var inputs = parts.Where(x => x.Direction == Direction.Input).ToArray();
            var outputs = parts.Where(x => x.Direction == Direction.Output).ToArray();


            //================
            // Timer
            //================
            var timeButton = new TimerButton(CraftInfo);
            timeButton.Left = Left - timeButton.Width / 2;
            timeButton.Top = Bottom - DefaultHeight / 2;
            AddRelatedControl(timeButton);

            //================
            // Crafter
            //================
            var crafterButton = new CrafterButton(CraftInfo);
            crafterButton.Left = Left - crafterButton.Width / 2;
            crafterButton.Top = Top - crafterButton.Height / 2;
            AddRelatedControl(crafterButton);

            //================
            // Counter
            //================
            var counterButton = new CounterButton(CraftInfo);
            counterButton.Top = crafterButton.Bottom + 3;
            counterButton.Height = timeButton.Top - 3 - counterButton.Top;
            counterButton.Left = Left - counterButton.Width / 2; 
            AddRelatedControl(counterButton);



            //================
            // Inputs
            //================
            var left = crafterButton.Right + 5;
            var top = Top - DefaultHeight / 2;
            foreach (var edge in inputs)
            {
                var button = new PartButton(edge, CraftInfo) { Location = new Point(left, top) };
                left = button.Right + 3;
                AddRelatedControl(button);
            }

            //================
            // Outputs
            //================
            left = crafterButton.Right + 5;
            var bottom = Bottom - DefaultHeight / 2;
            foreach (var edge in outputs)
            {
                var button = new PartButton(edge, CraftInfo) { Location = new Point(left, bottom) };
                left = button.Right + 3;
                AddRelatedControl(button);
            }


            Width = RelatableControls.Max(x => x.Right) + 5 - Left + timeButton.Width / 2;




            //in
            //out
            //crafter
            //count
        }


        private void InitSelf()
        {
            //Size
            //Image
            //Text

            //Size = new Size(220, 90);
            SuperTip.Items.Add($"{CraftInfo?.Recipe.LocalizedName}");

            //ImageOptions.Location = ImageLocation.MiddleLeft;
            //Appearance.TextOptions.HAlignment = HorzAlignment.Near;

            //ImageOptions.ImageList = ImagesHelper.Images32;
            //ImageOptions.ImageIndex = CraftInfo.Recipe.ImageIndex32();

        }

        public override void Reset()
        {
            ResetContent();
            base.Reset();
        }

        private void ResetContent()
        {
            ClearButtons();
            ResetSelf();
        }

        private void ResetSelf()
        {
            //Size
            //Image
            //Text
            //craft speed ???

            //Size = new Size(220, 90);
            ImageOptions.ImageIndex = -1;
        }


        protected override void CraftInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.CraftInfoPropertyChanged(sender, e);

            //crafter
            //count
        }

        private void ClearButtons()
        {
            foreach (var button in RelatableControls)
            {
                button.Reset();
                button.Parent.Controls.Remove((Control)button);
            }

            RelatableControls.Clear();
        }

        PartButton CreatePartButton(RecipePartEdge edge)
        {
            var button = new PartButton(edge, CraftInfo);
            return button;
        }
    }
}