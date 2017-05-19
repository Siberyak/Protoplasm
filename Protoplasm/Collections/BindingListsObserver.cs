using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Protoplasm.Utils;

namespace Protoplasm.Collections
{
    public static class BindingListsObserver
    {
        static BindingListsObserver()
        {
            ListChanged += (sender, args) => { };
            CreatingList += args => { };
        }

        public static event ListChangedEventHandler ListChanged;

        public static void OnListChanged(object sender, ListChangedEventArgs e)
        {
            ListChanged(sender, e);
        }

        public static event ListCreatingEventHandler CreatingList;

        public static void OnListCreating<TBase, TResult, TSenderClass>(IBindingList<TResult> sender, IList<TBase> baseList, bool? processIBindingListItem)
        {
            var e = ListCreatingEventArgs.New<TBase, TResult, TSenderClass>(sender, baseList, processIBindingListItem);
            CreatingList(e);
        }

        public static IList<TBase> OnStartCreatingList<TBase, TResult, TSenderClass>(this IList<TBase> baseList, bool? processIBindingListItem)
        {
            OnListCreating<TBase, TResult, TSenderClass>(null, baseList, processIBindingListItem);
            return baseList;
        }

        public static bool OnStartCreatingList<TBase, TResult, TSenderClass>(this bool processIBindingListItem)
        {
            OnListCreating<TBase, TResult, TSenderClass>(null, null, processIBindingListItem);
            return processIBindingListItem;
        }
    }

    public delegate void ListCreatingEventHandler(ListCreatingEventArgs e);

    public class ListCreatingEventArgs : EventArgs
    {
        public Type SenderClass { get; private set; }
        public Type BaseType { get; private set; }
        public Type ResultType { get; private set; }
        public object Sender { get; private set; }
        public object BaseList { get; private set; }
        public bool? ProcessIBindingListItem { get; private set; }

        public bool IsStart {get { return Sender == null; }}

        private ListCreatingEventArgs(Type senderClass, Type baseType, Type resultType, object sender, object baseList, bool? processIBindingListItem)
        {
            SenderClass = senderClass;
            BaseType = baseType;
            ResultType = resultType;
            Sender = sender;
            BaseList = baseList;
            ProcessIBindingListItem = processIBindingListItem;
        }

        internal static ListCreatingEventArgs New<TBase, TResult, TSenderClass>(IList<TResult> sender, IList<TBase> baseList, bool? processIBindingListItem)
        {
            return new ListCreatingEventArgs(typeof(TSenderClass), typeof(TBase), typeof(TResult), sender, baseList, processIBindingListItem);
        }

        public override string ToString()
        {
            return string.Format
                (
                "{0}: {1} [{2}] ({3})",
                IsStart ? ">>" : "<<",
                SenderClass.TypeName(),
                BaseList,
                ProcessIBindingListItem
                );
        }
    }
}