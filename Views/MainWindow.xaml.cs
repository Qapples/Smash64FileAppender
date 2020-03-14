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
    public class MainWindow : Window
    {
        public static List<byte> Bytes { get; set; }
        public static List<byte> AddBytes { get; set; }

        static PointerWindow _pointerWindow;
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new TextBoxModel() {InputText = "", OffsetText = ""};

            Bytes = new List<byte>();
            _pointerWindow = new PointerWindow();

            ClientSize = new Size(500, 500);
        }

        public async void ImportClick(object sender, RoutedEventArgs e)
        {
            //Dialog
            OpenFileDialog dialog = new OpenFileDialog();
            FileDialogFilter filter = new FileDialogFilter
            {
                Extensions = new[] {"bin"}.ToList(), Name = ".bin file"
            };

            List<FileDialogFilter> list = new List<FileDialogFilter> {filter};
            dialog.Filters = list;

            string[] fileNames = await dialog.ShowAsync(this);
            string output = "";

            int prevLength = Bytes.Count;
            int firstPointerIndex = 0;

            //Read bytes and change output
            AddBytes = (await File.ReadAllBytesAsync(fileNames[0])).ToList();
            Bytes.AddRange(AddBytes);

            //Update the pointer bytes. If there are already bytes, then search for the first pointer.
            if (prevLength > 0)
            {
                //Open the window
                _pointerWindow.Show();
            }

            ((TextBoxModel) DataContext).InputText +=
                AddBytes.Aggregate("", (current, b) => current + b.ToString("X2"));
        }

        public static void EditPointers()
        {
            int index = _pointerWindow.PointerIndex;
            int offset = (AddBytes[index + 2] << 8) + AddBytes[index + 3] - 8;
            int pointer = (AddBytes[index] << 8) + AddBytes[index + 1];

            while (pointer < AddBytes.Count)
            {
                
            }

            Console.WriteLine(
                $"Offset {offset:X2}. Index {index:X2}. Pointer {pointer:X2}. {AddBytes[index]:X2} {AddBytes[index + 1]:X2} {AddBytes[index + 2]:X2} {AddBytes[index + 3]:X2}");
        }

        public async void ExportClick(object sender, RoutedEventArgs e)
        {
            TextBoxModel context = DataContext as TextBoxModel;

            //Dialog
            SaveFileDialog dialog = new SaveFileDialog();
            FileDialogFilter filter = new FileDialogFilter
            {
                Extensions = new string[] {"bin",}.ToList(), Name = "bin files"
            };
            List<FileDialogFilter> list = new List<FileDialogFilter> {filter};
            dialog.Filters = list;

            string fileName = await dialog.ShowAsync(this);

            await using StreamWriter writer = new StreamWriter(fileName);
            for (int i = 0; i < context.InputText.Length - 1; i += 2)
            {
                await writer.WriteAsync((char) Convert.ToByte($"{context.InputText[i]}{context.InputText[i + 1]}", 16));
            }

            await writer.FlushAsync();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}