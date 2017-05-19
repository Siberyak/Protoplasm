using System.ComponentModel;

namespace Protoplasm.Collections
{
	public interface INotifyPropertyChangedExtended : INotifyPropertyChanged
	{
		void OnProertyChanged(string propertyName);
	    bool IsSubscribed(PropertyChangedEventHandler @delegate);
	}
}