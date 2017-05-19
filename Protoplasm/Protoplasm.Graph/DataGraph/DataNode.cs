using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Protoplasm.ComponentModel;
using Protoplasm.Collections;

namespace Protoplasm.Graph
{
    public class DataNode<TNodeData> : CustomTypeDescriptorBase, IDataNode<TNodeData>, IPropertyDataProvider
    {
        protected readonly MutableDataGraph _dataGraph;
        private PredicatedList<IEdge> _backReferences;

        private PredicatedList<IEdge> _references;

        public DataNode(MutableDataGraph dataGraph, TNodeData data)
        {
            _dataGraph = dataGraph;
            Data = data;
        }

        [Browsable(false)]
        public IMutableDataGraph Graph { get { return _dataGraph; } }
        IGraph INode.Graph
        {
            get { return Graph; }
        }

        public object NodeData()
        {
            return Data;
        }

        [Browsable(false)]
        public string Caption
        {
            get { return "" + Data; }
        }

        [Browsable(false)]
        public IEnumerable<IEdge> References
        {
            get { return _references ?? (_references = new PredicatedList<IEdge>(_dataGraph.EdgesDataList, x => x.IsBackreference ? x.To == this : x.From == this)); }
        }

        [Browsable(false)]
        public IEnumerable<IEdge> BackReferences
        {
            get { return _backReferences ?? (_backReferences = new PredicatedList<IEdge>(_dataGraph.EdgesDataList, x => x.IsBackreference ? x.From == this : x.To == this)); }
        }

        [Browsable(false)]
        public TNodeData Data { get; }

        [Browsable(false)]
        public Dictionary<object, object> Tags { get; } = new Dictionary<object, object>();

        public override string ToString()
        {
            return string.Format("Node, Data: [{0}]", Data);
        }


        private static readonly Dictionary<Type, PropertyDescriptorCollection> _propertiesByDataType
            = new Dictionary<Type, PropertyDescriptorCollection>();

        static PropertyDescriptorCollection GetDataProperties()
        {
            var type = typeof (TNodeData);
            lock (_propertiesByDataType)
            {
                PropertyDescriptorCollection properties;
                if (!_propertiesByDataType.TryGetValue(type, out properties))
                {
                    properties = TypeDescriptor.GetProperties(type);
                    _propertiesByDataType.Add(type, properties);
                }

                return properties;
            }
        }

        protected override List<PropertyDescriptor> GetPropertiesInternal()
        {
            var descriptors = base.GetPropertiesInternal();

            var dataProperties = GetDataProperties();

            var pds = dataProperties
                .OfType<PropertyDescriptor>()
                .Select(x => new DynamicPropertyDescriptor(GetType(), x.Name, x.PropertyType, x.IsReadOnly, x.Attributes.OfType<Attribute>().ToArray()))
                .ToArray();

            descriptors.AddRange(pds);

            return descriptors;
        }

        bool IPropertyDataProvider.CanResetValue(string propertyName)
        {
            return GetDataProperties()[propertyName].CanResetValue(Data);
        }

        bool IPropertyDataProvider.CanSetValue(string propertyName)
        {
            return !GetDataProperties()[propertyName].IsReadOnly;
        }

        void IPropertyDataProvider.ResetPropertyValue(string propertyName)
        {
            GetDataProperties()[propertyName].ResetValue(Data);
        }

        object IPropertyDataProvider.GetPropertyValue(string propertyName)
        {
            return GetDataProperties()[propertyName].GetValue(Data);
        }

        void IPropertyDataProvider.SetPropertyValue(string propertyName, object value)
        {
            GetDataProperties()[propertyName].SetValue(Data, value);
        }
    }
}