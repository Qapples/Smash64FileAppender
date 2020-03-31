using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace Smash64FileAppender.Models
{
    public class PointerModel
    {
        string _locationText;
        string _offsetText;
        
        /// <summary>
        /// Text of the input.
        /// </summary>
        public string LocationText
        {
            get => _locationText;
            set
            {
                if (value != _locationText)
                {
                    _locationText = value;
                    OnPropertyChanged(nameof(LocationText));
                }
            }
        }

        public string OffsetText
        {
            get => _offsetText;
            set
            {
                if (value != _offsetText)
                {
                    _offsetText = value;
                    OnPropertyChanged(nameof(OffsetText));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}