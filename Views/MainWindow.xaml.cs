using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
        static object _dataContext;
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new TextBoxModel() {InputText = "", OffsetText = ""};
            _dataContext = DataContext;

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

            //Escape the method if the user exits the window
            if (fileNames.Length <= 0)
            {
                return;
            }

            int prevLength = Bytes.Count;
            int firstPointerIndex = 0;

            //Read bytes and change output
            AddBytes = (await File.ReadAllBytesAsync(fileNames[0])).ToList();
            Bytes.AddRange(AddBytes);

            //If there is already data, then show the window that asks the pointer location
            if (prevLength > 0)
            {
                //Open the window
                _pointerWindow.Show();
            }
            
            
            ((TextBoxModel) DataContext).InputText +=
                AddBytes.Aggregate("", (current, b) => current + b.ToString("X2"));
        }

        public static Task EditPointers()
        {
            int index = _pointerWindow.PointerIndex;
            int offset = ((AddBytes[index + 2] << 8) + AddBytes[index + 3]) * 4 - 8;

            //Next pointer
            int pointer = (((AddBytes[index] << 8) + AddBytes[index + 1]) * 4 - offset + Bytes.Count - AddBytes.Count) /
                          4;

            //Location of the start of the file 
            int fileLocation = (((AddBytes[index + 2] << 8) + AddBytes[index + 3]) * 4 - offset + Bytes.Count -
                                AddBytes.Count) / 4;
            
            Console.WriteLine($"First: {pointer:X} {fileLocation:X} {offset:X} {index:X}");

            //Modify the pointers
            AddBytes[index] = (byte) ((pointer & 0xFF00) >> 8);
            AddBytes[index + 1] = (byte) (pointer & 0xFF);
            AddBytes[index + 2] = (byte) ((fileLocation & 0xFF00) >> 8);
            AddBytes[index + 3] = (byte) (fileLocation & 0xFF);

            int prevPointer = pointer;
            pointer *= 4;
            while (pointer < Bytes.Count)
            {
                //Update the pointers
                prevPointer = pointer;

                Console.WriteLine($"In Loop: {pointer:X} {fileLocation:X}");
                Console.WriteLine(
                    $"In Loop: {Bytes[pointer]:X2} {Bytes[pointer + 1]:X2} {Bytes[pointer + 2]:X2} {Bytes[pointer + 3]:X2} {Bytes.Count:X} {AddBytes.Count:X}");
                
                fileLocation = (((Bytes[pointer + 2] << 8) + Bytes[pointer + 3]) * 4 - offset + Bytes.Count -
                                AddBytes.Count) / 4;

                pointer = (((Bytes[pointer] << 8) + Bytes[pointer + 1]) * 4 - offset + Bytes.Count -
                           AddBytes.Count);
                
                //Modify the pointers in the file
                AddBytes[prevPointer - (Bytes.Count - AddBytes.Count)] = (byte) (((pointer / 4) & 0xFF00) >> 8);
                AddBytes[prevPointer + 1 - (Bytes.Count - AddBytes.Count)] = (byte) ((pointer / 4) & 0xFF);
                AddBytes[prevPointer + 2 - (Bytes.Count - AddBytes.Count)] = (byte) ((fileLocation & 0xFF00) >> 8);
                AddBytes[prevPointer + 3 - (Bytes.Count - AddBytes.Count)] = (byte) (fileLocation & 0xFF);

                Console.WriteLine($"In Loop2: {pointer:X} {fileLocation:X} {prevPointer:X}");
                //Console.WriteLine(
                //  $"In Loop2: {Bytes[pointer]:X2} {Bytes[pointer + 1]:X2} {Bytes[pointer + 2]:X2} {Bytes[pointer + 3]:X2} {Bytes.Count:X} {AddBytes.Count:X}");
            }

            Console.WriteLine($"A {fileLocation:X} {pointer:X} {prevPointer:X} {Bytes.Count} {AddBytes.Count}");

            //Modify the last pointers
            AddBytes[prevPointer - (Bytes.Count - AddBytes.Count)] = 0xFF;
            AddBytes[prevPointer + 1 - (Bytes.Count - AddBytes.Count)] = 0xFF;
            AddBytes[prevPointer + 2 - (Bytes.Count - AddBytes.Count)] = (byte) ((fileLocation & 0xFF00) >> 8);
            AddBytes[prevPointer + 3 - (Bytes.Count - AddBytes.Count)] = (byte) (fileLocation & 0xFF);

            //Re add the bytes
            Bytes.RemoveRange(Bytes.Count - AddBytes.Count, AddBytes.Count);
            ((TextBoxModel) _dataContext).InputText.Remove(Bytes.Count - AddBytes.Count);

            Bytes.AddRange(AddBytes);
            ((TextBoxModel) _dataContext).InputText +=
                AddBytes.Aggregate("", (current, b) => current + b.ToString("X2"));

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

            //Get ouf of the method if the user exits the dialog
            if (fileName is null)
            {
                return;
            }

            await File.WriteAllBytesAsync(fileName, Bytes.ToArray());
            /*
            await using (StreamWriter writer = new StreamWriter(fileName))
            {
                await writer.WriteAsync(AddBytes.Select(c => (char) c).ToArray());
                for (int i = 0; i < AddBytes.Count; i++)
                {
                    if (i % 25 == 0)
                    {
                        Console.WriteLine();
                    }

                    Console.Write($"{AddBytes[i]:X2} {i:X2}   ");
                }

                await writer.WriteAsync((char) 100);
                await writer.WriteAsync((char) 97);

                await writer.FlushAsync();
            }*/

            Console.WriteLine("finished");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}