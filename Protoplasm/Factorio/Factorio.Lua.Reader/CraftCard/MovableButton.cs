using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Factorio.Lua.Reader.Annotations;

namespace Factorio.Lua.Reader
{
    public class MovableButton : SimpleButton, INotifyPropertyChanged, IRelatableControl
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Point LocationOffset { get; set; }

        protected readonly List<IRelatableControl> RelatableControls = new List<IRelatableControl>();

        public virtual void Reset() { }


        protected void AddRelatedControl(IRelatableControl relatableControl)
        {
            RelatableControls.Add(relatableControl);
            relatableControl.LocationOffset = relatableControl.Location - (Size)Location;

            if (Parent == null)
                return;

            Parent.Controls.Add((Control)relatableControl);
            relatableControl.BringToFront();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            if (Parent != null)
            {
                foreach (var relatableControl in RelatableControls.Where(x => x.Parent == null).ToArray())
                {
                    Parent.Controls.Add((Control)relatableControl);
                    relatableControl.BringToFront();
                }
            }

            base.OnParentChanged(e);
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            foreach (var relatableControl in RelatableControls)
            {
                relatableControl.SuspendLayout();
                var buttonLocation = Location;
                buttonLocation.Offset(relatableControl.LocationOffset);
                relatableControl.Location = buttonLocation;
            }

            foreach (var relatableControl in RelatableControls)
            {
                relatableControl.ResumeLayout();
                relatableControl.Refresh();
            }

            base.OnLocationChanged(e);
            OnPropertyChanged(nameof(Location));
        }

        protected virtual bool AllowMoving { get; set; }

        protected Point? MovingStartLocation { get; set; }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (AllowMoving && e.Button == MouseButtons.Right && MovingStartLocation != null)
            {
                var point = MovingStartLocation.Value;
                var offset = e.Location - new Size(point);

                SetLocation(offset);

            }

            base.OnMouseMove(e);
        }

        protected virtual void SetLocation(Point offset)
        {
            SuspendLayout();

            var location = Location;
            location.Offset(offset);
            Location = location;

            var locationOffset = LocationOffset;
            locationOffset.Offset(offset);
            LocationOffset = locationOffset;

            ResumeLayout();
            Refresh();
            FindForm()?.Refresh();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (AllowMoving && e.Button == MouseButtons.Right)
            {
                MovingStartLocation = e.Location;
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ResetMoving();
            }

            base.OnMouseUp(e);
        }

        private void ResetMoving()
        {
            MovingStartLocation = null;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (MouseButtons == MouseButtons.Right)
            {
                if (e.KeyData == Keys.Escape)
                {
                    ResetMoving();
                }
            }

            base.OnKeyDown(e);
        }
        protected void Invoke(Action action)
        {
            var form = FindForm();
            if (form == null)
                return;

            if (form.InvokeRequired)
            {
                form.Invoke(action);
            }
            else
            {
                action();
            }
        }
    }
}