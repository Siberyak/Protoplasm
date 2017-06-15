using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Drawing;
using DevExpress.XtraEditors.ViewInfo;

namespace Factorio.Lua.Reader
{
    class LabelButton : MovableButton
    {
        public LabelButton()
        {
            AllowMoving = true;
        }

        public float Angle { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (var graphics = Parent.CreateGraphics())
            {
                var transform = new Matrix();
                var point = new PointF(Left + Width / 2f, Top + Height / 2f);
                transform.RotateAt(Angle, point);

                graphics.Transform = transform;
                graphics.DrawString("testtest", Font, Brushes.Black, point);
            }
        }
    }
    class PartButton : CraftInfoButton
    {
        private readonly RecipePartEdge _edge;
        private MovableButton _anchor;

        public PartButton(RecipePartEdge edge, CraftInfo craftInfo) : base(craftInfo)
        {
            _edge = edge;

            Size = new Size(DefaultWidth, DefaultHeight);

            var imageLocation = edge.Direction == Direction.Input
                ? ImageLocation.BottomCenter
                : ImageLocation.TopCenter;

            ImageOptions.Location = imageLocation;

            ImageOptions.ImageList = ImagesHelper.Images32;
            ImageOptions.ImageIndex = edge.Part.ImageIndex32();
            SuperTip.Items.Add(edge.Part.LocalizedName);

            var infos = edge.Graph.Edges.OfType<RecipePartEdge>()
                .Where(x => x.Direction != edge.Direction && x.Part == edge.Part)
                .Select(x => new { x.Recipe, Edge = x })
                .Distinct()
                .ToArray();

            //AddAnchor();


            if (infos.Length == 0)
                return;

            ContextMenuStrip = new ContextMenuStrip();
            foreach (var info in infos)
            {
                var item = new ToolStripButton
                {
                    Text = $"(x{info.Edge.Amount}, {info.Recipe._EnergyRequired} s.) {info.Recipe.LocalizedName}",
                    Image = info.Recipe.Image32(),
                    Tag = info.Recipe,
                };
                item.Click += ContextMenuStripItemClick;
                ContextMenuStrip.Items.Add(item);
            }
        }

        private void AddAnchor()
        {
            _anchor = new LabelButton
            {
                Size = new Size(10, 10),
                Top = _edge.Direction == Direction.Input ? Top - 15 : Bottom + 5,
                Left = Left + Width/2 - 5,
                Text = "",
                Visible = false,
                Angle = _edge.Direction == Direction.Input ? -90 : 90
            };

            AddRelatedControl(_anchor);
        }

        private void ContextMenuStripItemClick(object sender, EventArgs e)
        {
            var recipe = (Recipe)((ToolStripButton) sender).Tag;
            CraftInfo.RequestRecipe = recipe;
        }

        private void ContextMenuStripOpening(object sender, CancelEventArgs e)
        {
            ContextMenuStrip.Items.Clear();
            if (false)
                e.Cancel = true;
        }


        public override string ButtonText => $"{_edge.Amount}";

        protected override void OnClick(EventArgs e)
        {
            //_anchor.Visible = !_anchor.Visible;
            base.OnClick(e);
        }

    }


}