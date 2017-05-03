namespace MAS.Core.Compatibility
{
    /// <summary>
    /// ���� �������������
    /// </summary>
    public enum CompatibilityType
    {
        /// <summary>
        /// ������������� ��� ����� ���������������
        /// </summary>
        Always = 0,
        /// <summary>
        /// ������� �� ��������� �����
        /// </summary>
        DependsOnScene =1,
        /// <summary>
        /// ��������������� ��� ����� ���������������
        /// </summary>
        Never =2
    }
}