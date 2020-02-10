using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace Smash64FileAppender.Models
{
    public class TextBoxModel : INotifyPropertyChanged
    {
        string _inputText;
        string _offsetText;

        
        /// <summary>
        /// Text of the input.
        /// </summary>
        public string InputText
        {
            get => _inputText;
            set
            {
                if (value != _inputText)
                {
                    _inputText = value;
                    OnPropertyChanged(nameof(InputText));
                }
            }
        }
        
        /// <summary>
        /// Text Offset
        /// </summary>
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
