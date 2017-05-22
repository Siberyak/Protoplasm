using System;
using MAS.Core.Contracts;

namespace MAS.Utils
{
    internal interface IDataRequest
    {
        IScene Scene { get; }
    }
    internal class DataRequest : IDataRequest
    {
        public IScene Scene { get; }

        public DataRequest(IScene scene)
        {
            Scene = scene;
        }
    }

    internal interface IQuestion<out T>
    {
        IScene Scene { get; }
        T OriginalQuestion { get; }
    }

    internal class Question<T> : IQuestion<T>
    {
        public IScene Scene { get; }
        public T OriginalQuestion { get; }

        public Question(IScene scene, T question)
        {
            Scene = scene;
            OriginalQuestion = question;
        }
    }

}