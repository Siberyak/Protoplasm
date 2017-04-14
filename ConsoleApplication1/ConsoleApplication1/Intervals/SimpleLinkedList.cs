using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ConsoleApplication1.TestData;

namespace ConsoleApplication1.Intervals
{
    public class SimpleLinkedList<T> : IEnumerable<T>
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

        public Node First { get; private set; }
        public Node Last { get; private set; }

        public Node Find(Func<T, bool> predicate)
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

        public class Node
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

            public SimpleLinkedList<T> List { get; private set; }

            public Node Previous { get; private set; }

            public Node Next { get; private set; }

            public readonly T Value;

            internal void Remove()
            {
                var previous = Previous;
                var next = Next;

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

        public void Remove(Node node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (node.List != this)
                throw new ArgumentException("node.List != this", nameof(node));

            if (node == First)
                First = node.Next;
            if (node == Last)
                Last = node.Previous;

            node.Remove();
            Count--;
        }

        public Node AddFirst(T value)
        {
            Count++;
            return First == null 
                ? (Last = First = new Node(this, null, null, value)) 
                : AddBefore(First, value);
        }

        public Node AddAfter(Node node, T value)
        {
            if(node == null)
                throw new ArgumentNullException(nameof(node));

            if (node.List != this)
                throw new ArgumentException("node.List != this", nameof(node));

            var result = new Node(this, node, node.Next, value);

            if (result.Next == null)
                Last = result;

            Count++;

            return result;
        }

        public Node AddBefore(Node node, T value)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (node.List != this)
                throw new ArgumentException("node.List != this", nameof(node));

            var result = new Node(this, node.Previous, node, value);

            if (result.Previous == null)
                First = result;

            Count++;

            return result;
        }

        private class Enumerator : IEnumerator<T>
        {
            private readonly Node _startNode;
            private Node _current;

            public Enumerator(Node current)
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