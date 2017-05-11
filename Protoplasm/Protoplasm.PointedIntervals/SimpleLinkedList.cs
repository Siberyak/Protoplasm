using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Protoplasm.Utils;

namespace Protoplasm.PointedIntervals
{
    public interface INode<out T>
    {
        bool Alive { get; }

        T Value { get; }

        INode<T> Previous { get;  }

        INode<T> Next { get; }

    }

    internal class SimpleLinkedList<T> : IEnumerable<T>
    {
        public override string ToString()
        {
            return GetType().TypeName();
        }

        public SimpleLinkedList(IEnumerable<T> values = null)
        {
            var arr = values?.ToArray() ?? new T[0];

            foreach (var value in arr)
            {
                if (Count == 0)
                    AddFirst(value);
                else
                    AddAfter(Last, value);
            }
        }

        public int Count { get; private set; }

        public INode<T> First { get; private set; }
        public INode<T> Last { get; private set; }

        public INode<T> Find(Func<T, bool> predicate)
        {
            if (predicate == null)
                return null;

            var current = First;
            while (current != null)
            {
                if (predicate(current.Value))
                    return current;

                current = current.Next;
            }

            return null;
        }

        public T[] ToArray()
        {
            var result = new T[Count];

            var current = First;
            for (var i = 0; i < Count; i++)
            {
                result[i] = current.Value;
                current = current.Next;
            }

            return result;
        }

        public class Node : INode<T>
        {
            internal Node(SimpleLinkedList<T> list, Node previous, Node next, T value)
            {
                List = list;
                Previous = previous;
                Next = next;
                Value = value;

                if (previous != null)
                    previous.Next = this;

                if(next != null)
                    next.Previous = this;
            }

            public bool Alive => List != null;

            public SimpleLinkedList<T> List { get; private set; }

            public INode<T> Previous { get; private set; }

            public INode<T> Next { get; private set; }

            public T Value { get; }

            internal void Remove()
            {
                var previous = (Node)Previous;
                var next = (Node)Next;

                if(previous != null)
                    previous.Next = next;

                if(next != null)
                    next.Previous = previous;

                List = null;
            }

            public override string ToString()
            {
                return $"{GetType().TypeName()}: {Value}";
            }
        }

        public void Remove(INode<T> node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (((Node)node).List != this)
                throw new ArgumentException("node.List != this", nameof(node));

            if (node == First)
                First = node.Next;
            if (node == Last)
                Last = node.Previous;

            ((Node)node).Remove();
            Count--;
        }

        public INode<T> AddFirst(T value)
        {
            Count++;
            return First == null 
                ? (Last = First = new Node(this, null, null, value)) 
                : AddBefore(First, value);
        }

        public INode<T> AddAfter(INode<T> node, T value)
        {
            if(node == null)
                throw new ArgumentNullException(nameof(node));

            if (((Node)node).List != this)
                throw new ArgumentException("node.List != this", nameof(node));

            var result = new Node(this, (Node)node, (Node)node.Next, value);

            if (result.Next == null)
                Last = result;

            Count++;

            return result;
        }

        public INode<T> AddBefore(INode<T> node, T value)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (((Node)node).List != this)
                throw new ArgumentException("node.List != this", nameof(node));

            var result = new Node(this, (Node)node.Previous, (Node)node, value);

            if (result.Previous == null)
                First = result;

            Count++;

            return result;
        }

        private class Enumerator : IEnumerator<T>
        {
            private readonly INode<T> _startNode;
            private INode<T> _current;

            public Enumerator(INode<T> current)
            {
                if (current == null)
                    throw new ArgumentNullException(nameof(current));

                _startNode = current;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_current == null)
                {
                    _current = _startNode;
                    return true;
                }

                if (_current?.Next == null)
                    return false;

                _current = _current.Next;
                return true;
            }

            public void Reset()
            {
                _current = null;
            }

            public T Current
            {
                get
                {
                    if(_current == null)
                        throw new Exception("Current dont defined");

                    return _current.Value;
                }
            }

            object IEnumerator.Current => Current;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(First);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}