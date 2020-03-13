using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace Smash64FileAppender.Models
{
    public class PointerModel
    {
        string _inputText;
        
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}