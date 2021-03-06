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

        static int originalByteCount;
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new TextBoxModel() {InputText = "", OffsetText = ""};
            _dataContext = DataContext;

            Bytes = new List<byte>();
            AddBytes = new List<byte>();
            _pointerWindow = new PointerWindow();
            originalByteCount = 0;

            ClientSize = new Size(540, 500);
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

            //Read bytes and change output
            byte[] tempBytes = await File.ReadAllBytesAsync(fileNames[0]);
            AddBytes = tempBytes.ToList();

            //If there is already data, then show the window that asks the pointer location
            if (prevLength > 0)
            {
                originalByteCount = Bytes.Count;
                Bytes.AddRange(tempBytes);
            }
            else
            {
                Bytes.AddRange(tempBytes);
                originalByteCount = tempBytes.Length;
            }
            
            ((TextBoxModel) DataContext).InputText +=
                tempBytes.Aggregate("", (current, b) => current + b.ToString("X2") + " ");
        }

        public static void EditPointers(object sender, RoutedEventArgs e)
        {
            if (Bytes.Count <= 0) return;

            int index = Convert.ToInt32(((TextBoxModel) _dataContext).OffsetText, 16);
            int offset = ((AddBytes[index + 2] << 8) + AddBytes[index + 3]) * 4 - 8;

            //Next pointer
            int pointer = (((AddBytes[index] << 8) + AddBytes[index + 1]) * 4 - offset + Bytes.Count - AddBytes.Count) /
                          4;

            //Location of the start of the file 
            int fileLocation = (((AddBytes[index + 2] << 8) + AddBytes[index + 3]) * 4 - offset + Bytes.Count -
                                AddBytes.Count) / 4;

            //Update the last pointer in the og file
            int firstPointerLocation = (index + originalByteCount) / 4;

            Bytes[originalByteCount - 20] = (byte) (firstPointerLocation >> 8);
            Bytes[originalByteCount - 19] = (byte) (firstPointerLocation & 0xFF);

            Console.WriteLine($"First: {pointer:X} {fileLocation:X} {offset:X} {index:X} {originalByteCount:X}");

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

            //Re add the bytes
            Bytes.RemoveRange(Bytes.Count - AddBytes.Count, AddBytes.Count);
            ((TextBoxModel) _dataContext).InputText.Remove(Bytes.Count - AddBytes.Count);

            Bytes.AddRange(AddBytes);
            ((TextBoxModel) _dataContext).InputText +=
                AddBytes.Aggregate("", (current, b) => current + b.ToString("X2"));
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

            Console.WriteLine("finished");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}