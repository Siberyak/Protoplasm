using System;
using MAS.Core.Contracts;

namespace ConsoleApplication1.TestData
{
    public class Agent : IAgent
    {
        public virtual bool Equals(IAgent other)
        {
            throw new NotImplementedException();
        }

        public virtual bool Ask<TQuestion, TAnswer>(TQuestion question, out TAnswer answer)
        {
            throw new NotImplementedException();
        }

        public virtual bool Tell<TMessage>(TMessage message)
        {
            throw new NotImplementedException();
        }

        public virtual bool Request<TData>(out TData data)
        {
            throw new NotImplementedException();
        }

        public virtual void Initialize()
        {
            
        }
    }
}