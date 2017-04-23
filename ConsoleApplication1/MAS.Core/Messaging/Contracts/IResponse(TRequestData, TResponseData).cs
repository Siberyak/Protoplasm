namespace MAS.Core
{
    public static partial class Messaging<TMessanger>
    {
        public interface IResponse<TRequestData, TResponseData>
        {
            IRequest<TRequestData> Request { get; }
            TResponseData Data { get; }
        }
    }
}