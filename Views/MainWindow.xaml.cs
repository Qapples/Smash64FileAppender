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
        List<byte> _bytes = new List<byte>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new TextBoxModel() {InputText = "", OffsetText = ""};
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

            int prevLength = _bytes.Count;
            int firstPointerIndex = 0;

            //Read bytes and change output
            List<byte> addBytes = (await File.ReadAllBytesAsync(fileNames[0])).ToList();
            _bytes.AddRange(addBytes);

            //Last pointer is USUALLY the last two bytes (minus the 15 0's)
            int lastPointerIndex = _bytes[^19];

            //Update the pointer bytes. If there are already bytes, then search for the first pointer.
            if (prevLength > 0)
            {
                PointerWindow pointerWindow = new PointerWindow();
                pointerWindow.Show();

                int index = pointerWindow.PointerIndex;
                int offset = (addBytes[index + 2] << 4) + (addBytes[index + 3] - index);

                //Follow the pointers and update them accordingly
                //await EditPointers(firstPointerIndex, prevLength);
            }

            Console.WriteLine("\n" + firstPointerIndex.ToString("X"));

            //string output = _bytes.Aggregate("", (current, b) => current + b.ToString("X2"));

            ((TextBoxModel) DataContext).InputText +=
                addBytes.Aggregate("", (current, b) => current + b.ToString("X2"));
        }

        public Task EditPointers(int index, int length)
        {
            while (_bytes[index] != 0xFF)
            {
                int pointerValue = ((_bytes[index] << 8) + _bytes[index + 1]);
                _bytes[index] = (byte) ((pointerValue + length) << 8);
                _bytes[index + 1] = (byte) (pointerValue + length);
                Console.Write(index.ToString("X") + " " + _bytes[index] + " " +_bytes[index + 1].ToString("X2"));
                index = pointerValue * 4;
            }

            return Task.CompletedTask; 
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