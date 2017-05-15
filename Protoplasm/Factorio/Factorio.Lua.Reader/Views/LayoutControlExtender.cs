using System.ComponentModel;
using System.Drawing;
using System.Linq;
using DevExpress.XtraLayout;

namespace Factorio.Lua.Reader
{
    public interface ILayoutedView
    {
        LayoutControl LayoutControl { get; }
    }

    public static class LayoutControlExtender
    {
        public static void TerminateLayout(this ILayoutedView control)
        {
            control.LayoutControl.TerminateLayout();
        }

        public static void TerminateLayout(this LayoutControl control)
        {
            control.Root.TerminateLayout();
        }

        public static void TerminateLayout(this LayoutControlGroup control)
        {
            control.Add(new EmptySpaceItem() { Location = new Point(0, control.Height - 10) });
        }

        public static void AddSeparator(this ILayoutedView control)
        {
            control.LayoutControl.AddSeparator();
        }

        public static void AddSeparator(this LayoutControl control)
        {
            control.Root.AddSeparator();
        }
        public static void AddSeparator(this LayoutControlGroup control)
        {
            control.Add(new SimpleSeparator() { Location = NextLocation(control) });
        }

        public static SimpleLabelItem AddLabel(this ILayoutedView control, string text, int? imageIndex = null, Color? foreColor = null)
        {
            return control.LayoutControl.AddLabel(text, imageIndex, foreColor);
        }
        public static SimpleLabelItem AddLabel(this LayoutControl control, string text, int? imageIndex = null, Color? foreColor = null)
        {
            return control.Root.AddLabel(text, imageIndex, foreColor);
        }

        public static SimpleLabelItem AddLabel(this LayoutControlGroup control, string text, int? imageIndex = null, Color? foreColor = null)
        {

            var item = new SimpleLabelItem();
            ((ISupportInitialize) item).BeginInit();
            item.AllowHotTrack = false;

            if (foreColor.HasValue)
            {
                item.AppearanceItemCaption.ForeColor = foreColor.Value;
                item.AppearanceItemCaption.Options.UseForeColor = true;
            }


            var location = NextLocation(control);

            item.Location = location;

            item.Name = "simpleLabelItem2";
            item.Text = text;
            item.ImageIndex = imageIndex ?? -1;

            control.Add(item);

            ((ISupportInitialize) (item)).EndInit();
            return item;
        }

        public static Point NextLocation(this ILayoutedView control, bool down = true)
        {
            return control.LayoutControl.NextLocation(down);
        }

        public static Point NextLocation(this LayoutControl control, bool down = true)
        {
            var lastItem = control.Items.LastOrDefault();
            return lastItem.NextLocation(down);
        }

        public static Point NextLocation(this LayoutControlGroup control, bool down = true)
        {
            var lastItem = control.Items.LastOrDefault();
            return lastItem.NextLocation(down);
        }

        public static Point NextLocation(this BaseLayoutItem item, bool down = true)
        {
            var location = Point.Empty;

            if (item == null)
                return location;

            if (down)
                location.Offset(0, item.Location.Y + item.Height);
            else
                location.Offset(item.Location.X + item.Width, item.Location.Y);

            return location;
        }
    }
}