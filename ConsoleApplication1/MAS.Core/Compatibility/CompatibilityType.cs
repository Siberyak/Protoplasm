namespace MAS.Core
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