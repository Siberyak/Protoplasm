using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;

namespace Factorio.Lua.Reader
{
    public class CraftInfoButton : MovableButton
    {

        private CraftInfo _craftInfo;

        protected static readonly int DefaultWidth = 40;
        protected static readonly int DefaultHeight = 60;

        public CraftInfoButton()
        {
            AllowHtmlDraw = DefaultBoolean.True;
            AllowHtmlTextInToolTip = DefaultBoolean.True;
            ShowFocusRectangle = DefaultBoolean.False;
            SuperTip = new SuperToolTip {AllowHtmlText = DefaultBoolean.True};
        }

        public CraftInfoButton(Size size) : this()
        {
            Size = size;
        }

        public CraftInfoButton(CraftInfo craftInfo) : this()
        {
            CraftInfo = craftInfo;
        }

        public virtual CraftInfo CraftInfo
        {
            get { return _craftInfo; }
            set
            {
                if (Equals(value, _craftInfo))
                    return;

                BeforeCraftInfoChanged();

                _craftInfo = value;

                AfterCraftInfoChanged();

                OnPropertyChanged();
            }
        }

        protected virtual void AfterCraftInfoChanged()
        {
            if (_craftInfo == null)
                return;

            _craftInfo.PropertyChanged += CraftInfoPropertyChanged;
            DataBindings.Add(nameof(Text), this, nameof(ButtonText));
        }

        protected virtual void BeforeCraftInfoChanged()
        {
            if (_craftInfo == null)
                return;

            _craftInfo.PropertyChanged -= CraftInfoPropertyChanged;
            DataBindings.Clear();
            SuperTip.Items.Clear();
        }

        protected virtual void CraftInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ButtonText));
            if (e.PropertyName == "Crafter" || e.PropertyName == "Enabled")
                Refresh();
        }

        public override void Reset()
        {
            DataBindings.Clear();
            CraftInfo = null;
            base.Reset();
        }

        public virtual string ButtonText => ButtonTextDelegate.Invoke(this) ?? "button";
        public Func<CraftInfoButton, string> ButtonTextDelegate { get; set; }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if(CraftInfo?.Crafter != null && CraftInfo.Enabled)
            {
                return;
            }

            var color = CraftInfo?.Crafter == null ? Color.Red : Color.Yellow;
            color = Color.FromArgb(48, color.R, color.G, color.B);
            e.Graphics.FillRectangle(new SolidBrush(color), e.ClipRectangle);

        }
    }
}