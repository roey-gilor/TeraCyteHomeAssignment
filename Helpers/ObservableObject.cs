using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TeraCyteHomeAssignment.Helpers
{
    /// <summary>
    /// Base class for MVVM ViewModels.
    /// Implements INotifyPropertyChanged to support UI data-binding in WPF.
    /// Provides helper SetProperty and OnPropertyChanged methods to raise notifications.
    /// </summary>
    public class ObservableObject : INotifyPropertyChanged
    {
        /* Event raised when a property value changes.
          WPF/XAML binding system listens to this event to refresh UI automatically. */
        public event PropertyChangedEventHandler? PropertyChanged;

        // Raises PropertyChanged event for the given property name.
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Sets a backing field and raises PropertyChanged only if the new value differs.
        // Prevents infinite loops / unnecessary UI refreshes.
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null!)
        {
            // If the value hasn't changed, do nothing
            if (Equals(field, value))
                return false;

            // Update field and notify UI
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
