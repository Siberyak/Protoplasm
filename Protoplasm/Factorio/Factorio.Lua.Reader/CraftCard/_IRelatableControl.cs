using System.Drawing;
using System.Windows.Forms;

namespace Factorio.Lua.Reader
{
    public interface IRelatableControl
    {
        Control Parent { get; }
        Point LocationOffset { get; set; }
        Point Location { get; set; }
        void SuspendLayout();
        void ResumeLayout();
        void Refresh();
        void Reset();
        void BringToFront();
        void SendToBack();

        int Left { get; set; }
        int Top { get; set; }
        int Right { get; }
        int Bottom { get; }
    }
}