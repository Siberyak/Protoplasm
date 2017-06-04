using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Factorio.Lua.Reader.Views;
using Protoplasm.Utils;

namespace Factorio.Lua.Reader
{
    class CrafterButton : CraftInfoButton
    {
        public CrafterButton(CraftInfo craftInfo) : base(craftInfo)
        {
            Size = new Size(DefaultWidth, DefaultHeight);
            ImageOptions.Location = ImageLocation.TopCenter;
            ImageOptions.ImageList = ImagesHelper.Images32;

            UpdateInfo();
        }

        private void UpdateInfo()
        {
            ImageOptions.ImageIndex = CraftInfo.Crafter?.ImageIndex32() ?? -1;
            var localizedName = CraftInfo.Crafter?.LocalizedName;
            if (localizedName == null)
                SuperTip.Items.Clear();
            else
            {
                SuperTip.Items.Add(localizedName);
            }
        }

        public override string ButtonText => $"x {CraftInfo.Crafter?._CraftingSpeed ?? 1}";

        protected override void CraftInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.CraftInfoPropertyChanged(sender, e);

            if (e.PropertyName != nameof(CraftInfo.Crafter))
                return;

            UpdateInfo();
            OnPropertyChanged(nameof(ButtonText));
        }

        protected override void OnClick(EventArgs e)
        {
            if(CraftInfo == null)
                return;

            if (ModifierKeys == Keys.Control || ModifierKeys == Keys.ControlKey)
            {
                CraftInfo.Crafter = null;
                return;
            }

            ICrafter crafter;
            if (!ViewsExtender.SelectResult<CraftersView, ICrafter>(out crafter, (f, v) => InitSelectorView(f, v, CraftInfo)))
                return;

            CraftInfo.Crafter = crafter;

            //base.OnClick(e);
        }

        private void InitSelectorView(Form form, CraftersView view, CraftInfo craftInfo)
        {
            view.Recipe = craftInfo.Recipe;
            view.Crafter = craftInfo.Crafter;
        }
    }
}