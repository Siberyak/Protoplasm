using System.Windows.Forms;

namespace Factorio.Lua.Reader
{
    class CounterButton : CraftInfoButton
    {
        public CounterButton(CraftInfo craftInfo) : base(craftInfo)
        {
            Width = DefaultWidth;
        }

        public override string ButtonText => $"x {CraftInfo.Count}";

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            ((HandledMouseEventArgs)e).Handled = true;

            var offset = e.Delta / SystemInformation.MouseWheelScrollDelta;
            if(offset == 0 || CraftInfo.Count + offset < 1)
                return;

            CraftInfo.Count += offset;

            base.OnMouseWheel(e);
        }
    }
}