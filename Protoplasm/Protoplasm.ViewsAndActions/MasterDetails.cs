namespace Protoplasm.ViewsAndActions
{
    public class MasterDetails
    {
        /// <summary>
        /// ћастре-детали расположены горизонтально, сплиттер двигаетс€ вертикально
        /// </summary>
        public static MasterDetails Horizontal = new MasterDetails() { SplitterMoveHorizontal = false };

        /// <summary>
        /// ћастре-детали расположены вертикально, сплиттер двигаетс€ горизонтально
        /// </summary>
        public static MasterDetails Vertical = new MasterDetails() { SplitterMoveHorizontal = true };

        private MasterDetails()
        {
        }

        public object DetailsViewContext { get; private set; }
        public bool SplitterMoveHorizontal { get; private set; }
        public bool IsSplitterFixed { get; private set; }

        public MasterDetails SetDetailsViewContext(object viewContext = null)
        {
            viewContext = viewContext ?? ModelViewController.DefaultViewContext;

            return new MasterDetails
            {
                IsSplitterFixed = IsSplitterFixed,
                SplitterMoveHorizontal = SplitterMoveHorizontal,
                DetailsViewContext = viewContext,
            };
        }
    }
}