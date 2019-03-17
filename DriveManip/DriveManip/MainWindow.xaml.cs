using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;
using DriveManip.Annotations;

namespace DriveManip
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //public CharacterList Characters { get; set; }
        public ObservableCollection<CheckedListItem<MetaCharacterdisplay>> Characters { get; set; }
        public GoogleHandler GHandler;

        public MainWindow()
        {
            GHandler = new GoogleHandler();
            InitializeComponent();
            Characters = new ObservableCollection<CheckedListItem<MetaCharacterdisplay>>();
            Fetch.Click += FetchOnClick;
            sAll.Click += SAllOnClick;
            PrintSel.Click += PrintSelOnClick;
            OpenOutput.Click += OpenOutputOnClick;
            Status.Content = "Not Connected";
            DisplayList.ItemsSource = Characters;
        }

        private void OpenOutputOnClick(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory("Output");
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Output");
            Process.Start("explorer", path);
        }

        private void PrintSelOnClick(object sender, RoutedEventArgs e)
        {
            Status.Content = "Collecting for print...";
            var pdfs = GHandler.GetPdfDocuments(Characters.Where(c => c.IsChecked).Select(c => c.Item.MetaFile).ToList());
            Directory.CreateDirectory("Output");
            pdfs.Save("Output/ExportedSheets.pdf");
            pdfs.Close();
            Status.Content = "Output at ExportedSheets.pdf";
            Characters.Clear();
        }

        private void SAllOnClick(object sender, RoutedEventArgs e)
        {
            foreach (var character in Characters)
            {
                character.IsChecked = true;
            }
        }

        private void FetchOnClick(object sender, RoutedEventArgs e)
        {
            Status.Content = "Connecting...";
            Characters.Clear();
            var files = GHandler.FetchFiles();
            foreach (var file in files)
            {
                Characters.Add(new CheckedListItem<MetaCharacterdisplay>
                {
                    Item = new MetaCharacterdisplay
                    {
                        CharacterName = file.Name,
                        MetaFile = file
                    }
                });
            }

            Status.Content = " Fetched";
        }

        private void Scroll_List(object sender, MouseWheelEventArgs e)
        {
            ScrollView.ScrollToVerticalOffset(ScrollView.VerticalOffset - e.Delta * 0.5);
            e.Handled = true;
        }
    }
}
