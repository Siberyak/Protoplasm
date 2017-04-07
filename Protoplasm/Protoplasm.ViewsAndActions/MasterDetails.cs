namespace Protoplasm.ViewsAndActions
{
    public class MasterDetails
    {
        /// <summary>
        /// ������-������ ����������� �������������, �������� ��������� �����������
        /// </summary>
        public static MasterDetails Horizontal = new MasterDetails() { SplitterMoveHorizontal = false };

        /// <summary>
        /// ������-������ ����������� �����������, �������� ��������� �������������
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