using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Smash64FileAppender.Models;

namespace Smash64FileAppender.Views
{
    public class PointerWindow : Window
    {
        public int PointerIndex { get; set; }
        
        public PointerWindow()
        {
            InitializeComponent();
            DataContext = new PointerModel() {LocationText = ""};
            ClientSize = new Size(250, 115);
        }

        public async void PointerButtonClick(object sender, RoutedEventArgs e)
        {
            PointerIndex = Convert.ToInt32((DataContext as PointerModel)?.LocationText, 16);
            Close();
        }
        
        void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}