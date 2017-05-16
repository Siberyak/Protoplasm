using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using DevExpress.XtraLayout.Utils;

namespace Factorio.Lua.Reader
{
    public interface ILayoutedView
    {
        LayoutControl LayoutControl { get; }
    }

    public interface ISelectorView
    {
        event EventHandler Selected;

        void AfterLoad();
    }

    public interface ISelectorView<out TResult> : ISelectorView
    {
        TResult Selection { get; }
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
            var emptySpaceItem = new EmptySpaceItem { Location = new Point(0, control.Height - 10) };
            var lastItem = control.Items.LastOrDefault();
            control.Add(emptySpaceItem);
            if (lastItem != null)
                emptySpaceItem.Move(lastItem, InsertType.Bottom);
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

        public static LayoutControlItem AddControl<T>(this ILayoutedView control, Action<LayoutControlItem, T> init)
            where T : Control, new()
        {
            return control.LayoutControl.AddControl(init);
        }

        public static LayoutControlItem AddControl<T>(this LayoutControl control, Action<LayoutControlItem, T> init)
            where T : Control, new()
        {
            var edit = new T {Name = $"edit{Guid.NewGuid()}"};
            var item = new LayoutControlItem(control, edit) {Name = $"item{Guid.NewGuid()}"};

            init?.Invoke(item, edit);

            control.Root.Add(item);
            return item;
        }


        public static LayoutControlItem AddImage(this ILayoutedView control, Image image, Point? location = null)
        {
            return control.LayoutControl.AddImage(image);
        }

        public static LayoutControlItem AddImage(this LayoutControl control, Image image, Point? location = null)
        {
            var loc = location ?? control.NextLocation();

            var edit = new PictureEdit {Image = image, Size = image.Size, MinimumSize = image.Size, MaximumSize = image.Size, Name = $"edit{Guid.NewGuid()}", ReadOnly = true};
            edit.BackColor = Color.Transparent;

            var item = new LayoutControlItem(control, edit);

            item.TextVisible = false;
            item.Name = $"item{Guid.NewGuid()}";
            item.Location = loc;
            item.Size = image.Size;
            item.MaxSize = image.Size;
            item.MinSize = image.Size;

            control.Root.Add(item);
            return item;
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
            ((ISupportInitialize)item).BeginInit();
            item.AllowHotTrack = false;

            if (foreColor.HasValue)
            {
                item.AppearanceItemCaption.ForeColor = foreColor.Value;
                item.AppearanceItemCaption.Options.UseForeColor = true;
            }


            var location = NextLocation(control);

            item.Location = location;

            item.Name = $"simpleLabelItem{Guid.NewGuid()}";
            item.Text = text;
            item.ImageIndex = imageIndex ?? -1;

            control.Add(item);

            ((ISupportInitialize)(item)).EndInit();
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