using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Utils.Drawing;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraLayout;

namespace Factorio.Lua.Reader.Controls
{
    public partial class MovableContainer : DevExpress.XtraEditors.XtraUserControl
    {
        private bool _movingMode;

        public MovableContainer()
        {
            InitializeComponent();
        }

        private Control _panel;
        Stack<Point> _locations = new Stack<Point>();

        public bool MovingMode
        {
            get { return _movingMode; }
            set
            {
                if(DesignMode || _movingMode == value)
                    return;

                _movingMode = value;
                if (_movingMode)
                {
                    var bitmap = new Bitmap(Width, Height);
                    DrawToBitmap(bitmap, new Rectangle(Point.Empty, Size));

                    using (var gr = Graphics.FromImage(bitmap))
                    {
                        gr.FillRectangle(new SolidBrush(Color.FromArgb(64, 255, 0, 0)), new Rectangle(Point.Empty, Size));
                    }

                    _panel = new Control
                    {
                        Size = Size,
                        Location = Location,
                        //TabStop = false,
                        BackgroundImage = bitmap
                    };

                    Parent.Controls.Add(_panel);

                    _panel.BringToFront();
                    _panel.MouseMove += PanelMouseMove;
                    _panel.MouseDown += PanelMouseDown;
                    _panel.MouseUp += PanelMouseUp;
                    _panel.KeyDown += PanelKeyPress;


                    Visible = false;

                    _locations.Push(Location);
                    //_panel.Focus();
                }
                else
                {
                    _locations.Clear();
                    _panel.MouseMove -= PanelMouseMove;
                    _panel.MouseDown -= PanelMouseDown;
                    _panel.MouseUp -= PanelMouseUp;
                    _panel.KeyDown -= PanelKeyPress;

                    Parent.Controls.Remove(_panel);
                    _panel = null;

                    Visible = true;
                    Tag = null;

                }

                Refresh();
            }
        }

        private void PanelKeyPress(object sender, KeyEventArgs e)
        {
            if (MouseButtons != MouseButtons.Left)
                return;

            if (e.KeyData == Keys.Escape)
            {
                ResetMoving();
                SetLocation(_locations.Peek());
            }
        }


        private void PanelMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if(_locations.Peek() != Location)
                _locations.Push(Location);

            ResetMoving();

        }

        private void ResetMoving()
        {
            _panel.Tag = null;
            _panel.Cursor = Cursors.Default;
        }

        private void PanelMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            _panel.Tag = e.Location;
            _panel.Cursor = Cursors.NoMove2D;
        }

        private void PanelMouseMove(object sender, MouseEventArgs e)
        {
            if(e.Button != MouseButtons.Left || !(_panel.Tag is Point))
                return;

            var point = ((Point) _panel.Tag);
            var offset = e.Location - new Size(point);
            var location = GetLocation();
            location.Offset(offset);
            SetLocation(location);
            //Console.WriteLine(offset);
        }

        private void SetLocation(Point location)
        {
            Location = location;
            _panel.Location = location;
        }

        private Point GetLocation()
        {
            return _panel.Location;
        }
    }

}
