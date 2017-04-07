using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

namespace Protoplasm.ComponentModel
{
    public abstract class CustomTypeDescriptorBase : ICustomTypeDescriptor, INotifyPropertiesChanging
	{
		private static readonly object Locker = new object();
		protected PropertyDescriptorCollection _properties;

		protected PropertyDescriptorCollection Properties
		{
			get
			{
			    CheckProperties();
			    return _properties;
			}
		}

        protected virtual void ResetProperties()
        {
            _properties = null;
            CheckProperties();
        }

        protected void CheckProperties()
        {
            if (_properties == null)
            {
                lock (Locker)
                {
                    if (_properties == null)
                    {
                        OnPropertiesChanging(NotifyPropertiesChangingEventArgs.Starting);
                        _properties = new PropertyDescriptorCollection(GetPropertiesInternal().ToArray());
                        OnPropertiesChanging(NotifyPropertiesChangingEventArgs.Ended);
                    }
                }
            }
        }

        protected virtual List<PropertyDescriptor> GetPropertiesInternal()
		{
			return GetProperties(GetType());
		}

		public static List<PropertyDescriptor> GetProperties(Type type)
		{
			var collection = TypeDescriptor.GetProperties(type);
			var ret = collection.Cast<PropertyDescriptor>().ToList();
			return ret;
		}

		AttributeCollection ICustomTypeDescriptor.GetAttributes()
		{
			return TypeDescriptor.GetAttributes(GetType());
		}

		string ICustomTypeDescriptor.GetClassName()
		{
			return TypeDescriptor.GetClassName(GetType());
		}

		string ICustomTypeDescriptor.GetComponentName()
		{
			return TypeDescriptor.GetComponentName(GetType());
		}

		TypeConverter ICustomTypeDescriptor.GetConverter()
		{
			return TypeDescriptor.GetConverter(GetType());
		}

		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
		{
			return TypeDescriptor.GetDefaultEvent(GetType());
		}

		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
		{
			return TypeDescriptor.GetDefaultProperty(GetType());
		}

		object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
		{
			return TypeDescriptor.GetEditor(GetType(), editorBaseType);
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
		{
			return TypeDescriptor.GetEvents(GetType());
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
		{
			return TypeDescriptor.GetEvents(GetType(), attributes);
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
		{
			return Properties;
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
		{
			return Properties;
		}

		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
		{
			return this;
		}

        public event NotifyPropertiesChangingEventHandler PropertiesChanging;

        protected void ResetPropertiesChanging()
        {
            PropertiesChanging = null;
        }

        protected virtual void OnPropertiesChanging(NotifyPropertiesChangingEventArgs e)
        {
            PropertiesChanging?.Invoke(this, e);
        }
	}

	public interface IPropertyDataProvider
	{
        bool CanResetValue(string propertyName);
        bool CanSetValue(string propertyName);
        void ResetPropertyValue(string propertyName);
        object GetPropertyValue(string propertyName);
        void SetPropertyValue(string propertyName, object value);
    }

	public class DynamicPropertyDescriptor<TComponentType, TPropertyType> : DynamicPropertyDescriptor
		where TComponentType : IPropertyDataProvider
	{
        private static readonly FakePropertyDataProvider Fake = new FakePropertyDataProvider();

        class FakePropertyDataProvider : IPropertyDataProvider
        {
            #region Implementation of IPropertyDataProvider

            public bool CanResetValue(string propertyName)
            {
                return false;
            }

            public bool CanSetValue(string propertyName)
            {
                return false;
            }

            public void ResetPropertyValue(string propertyName)
            {

            }

            public object GetPropertyValue(string propertyName)
            {
                return default(TPropertyType);
            }

            public void SetPropertyValue(string propertyName, object value)
            {

            }

            #endregion
        }

        public DynamicPropertyDescriptor(string name, bool isReadOnly, Attribute[] attributes, TypeConverter converter = null)
            : base(typeof(TComponentType), name, typeof(TPropertyType), isReadOnly, attributes, converter)
	    {
	    }

        public DynamicPropertyDescriptor(string name, Attribute[] attributes, TypeConverter converter = null)
            : base(typeof(TComponentType), name, typeof(TPropertyType), attributes, converter)
	    {
	    }

        public DynamicPropertyDescriptor(string name, TypeConverter converter = null)
            : base(typeof(TComponentType), name, typeof(TPropertyType), converter)
	    {
	    }

	    protected override IPropertyDataProvider GetComponent(object component)
	    {
	        return base.GetComponent(component) ?? Fake;
	    }
	}

   	public class DynamicPropertyDescriptor : PropertyDescriptor
	{

        private readonly bool _isReadOnly;
   	    private readonly TypeConverter _converter;
   	    private readonly Type _componentType;
   	    private readonly Type _propertyType;

   	    public DynamicPropertyDescriptor(Type componentType, string name, Type propertyType, bool isReadOnly, Attribute[] attributes, TypeConverter converter = null) 
            : base(name, attributes)
   	    {
   	        _isReadOnly = isReadOnly;
   	        _converter = converter;
   	        _componentType = componentType;
   	        _propertyType = propertyType;
   	    }

        public DynamicPropertyDescriptor(Type componentType, string name, Type propertyType, Attribute[] attributes, TypeConverter converter = null)
            : this(componentType, name, propertyType, GetIsReadonly(attributes), attributes, converter)
        {
        }

	    private static bool GetIsReadonly(IEnumerable<Attribute> attributes)
	    {
	        var readOnlyAttribute = attributes.OfType<ReadOnlyAttribute>().FirstOrDefault();
	        return readOnlyAttribute != null && readOnlyAttribute.IsReadOnly;
	    }

        public DynamicPropertyDescriptor(Type componentType, string name, Type propertyType, TypeConverter converter = null)
            : this(componentType, name, propertyType, null, converter)
        {
        }

        protected virtual IPropertyDataProvider GetComponent(object component)
        {
            return component as IPropertyDataProvider;
        }

		/// <summary>
		/// When overridden in a derived class, returns whether resetting an object changes its value.
		/// </summary>
		/// <returns>
		/// true if resetting the component changes its value; otherwise, false.
		/// </returns>
		/// <param name="component">The component to test for reset capability. </param>
		public override bool CanResetValue(object component)
		{
			return GetComponent(component).CanResetValue(Name);
		}

		/// <summary>
		/// When overridden in a derived class, gets the current value of the property on a component.
		/// </summary>
		/// <returns>
		/// The value of a property for a given component.
		/// </returns>
		/// <param name="component">The component with the property for which to retrieve the value. </param>
		public override object GetValue(object component)
		{
            return GetComponent(component).GetPropertyValue(Name);
		}

		/// <summary>
		/// When overridden in a derived class, resets the value for this property of the component to the default value.
		/// </summary>
		/// <param name="component">The component with the property value that is to be reset to the default value. </param>
		public override void ResetValue(object component)
		{
			GetComponent(component).ResetPropertyValue(Name);
		}

		/// <summary>
		/// When overridden in a derived class, sets the value of the component to a different value.
		/// </summary>
		/// <param name="component">The component with the property value that is to be set. </param><param name="value">The new value. </param>
		public override void SetValue(object component, object value)
		{
            GetComponent(component).SetPropertyValue(Name, value);
        }

		/// <summary>
		/// When overridden in a derived class, determines a value indicating whether the value of this property needs to be persisted.
		/// </summary>
		/// <returns>
		/// true if the property should be persisted; otherwise, false.
		/// </returns>
		/// <param name="component">The component with the property to be examined for persistence. </param>
		public override bool ShouldSerializeValue(object component)
		{
			return false;
		}

		/// <summary>
		/// When overridden in a derived class, gets the type of the component this property is bound to.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Type"/> that represents the type of component this property is bound to. When the <see cref="M:System.ComponentModel.PropertyDescriptor.GetValue(System.Object)"/> or <see cref="M:System.ComponentModel.PropertyDescriptor.SetValue(System.Object,System.Object)"/> methods are invoked, the object specified might be an instance of this type.
		/// </returns>
		public override Type ComponentType
		{
			get { return _componentType; }
		}

		/// <summary>
		/// When overridden in a derived class, gets a value indicating whether this property is read-only.
		/// </summary>
		/// <returns>
		/// true if the property is read-only; otherwise, false.
		/// </returns>
		public override bool IsReadOnly
		{
			get { return _isReadOnly; }
		}

		/// <summary>
		/// When overridden in a derived class, gets the type of the property.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Type"/> that represents the type of the property.
		/// </returns>
		public override Type PropertyType
		{
			get { return _propertyType; }
		}

   	    public override TypeConverter Converter
   	    {
   	        get { return _converter ?? base.Converter; }
   	    }
	}

    public enum PropertiesChangingType
    {
        Starting, Ended
    }

    public delegate void NotifyPropertiesChangingEventHandler(object sender, NotifyPropertiesChangingEventArgs e);

    public class NotifyPropertiesChangingEventArgs : EventArgs
    {
        public PropertiesChangingType Type { get; private set; }
        //public PropertyDescriptor Descriptor { get; private set; }

        //private NotifyPropertiesChangingEventArgs(PropertiesChangingType type, PropertyDescriptor descriptor)
        //{
        //    Type = type;
        //    Descriptor = descriptor;
        //}

        private NotifyPropertiesChangingEventArgs(PropertiesChangingType type)
        {
            Type = type;
        }

        public static readonly NotifyPropertiesChangingEventArgs Starting = new NotifyPropertiesChangingEventArgs(PropertiesChangingType.Starting);
        public static readonly NotifyPropertiesChangingEventArgs Ended = new NotifyPropertiesChangingEventArgs(PropertiesChangingType.Ended);
    }

    public interface INotifyPropertiesChanging
    {
        event NotifyPropertiesChangingEventHandler PropertiesChanging;
    }
}