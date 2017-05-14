using System.Collections;
using System.ComponentModel;
using DevExpress.XtraTreeList;

namespace Factorio.Lua.Reader
{
    abstract class VirtualTreeListData<T> : VirtualTreeListData
    {


        public T Data;

        protected VirtualTreeListData(Storage storage, T data) : base(storage)
        {
            Data = data;
        }

        public override object NodeData => Data;

        public override string Icon => (Data as IRecipePart)?._Icon;


    }

    abstract class VirtualTreeListData : TreeList.IVirtualTreeListData, IVirtualNode
    {
        protected readonly Storage Storage;

        protected VirtualTreeListData(Storage storage)
        {
            Storage = storage;
        }

        public void VirtualTreeGetChildNodes(VirtualTreeGetChildNodesInfo info)
        {
            info.Children = GetChildren();
        }

        protected abstract IList GetChildren();

        public void VirtualTreeGetCellValue(VirtualTreeGetCellValueInfo info)
        {
            info.CellData = GetValue(info.Column.FieldName);
        }
        protected abstract object GetValue(string fieldName);

        public void VirtualTreeSetCellValue(VirtualTreeSetCellValueInfo info)
        {
        }

        public abstract object NodeData { get; }

        public virtual string Icon { get; } = null;
    }
}