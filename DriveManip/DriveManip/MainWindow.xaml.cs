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
        private List<string> Folders;
        private bool isFetchingFolders = true;

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
            Folders = new List<string>();
            Fetch.Content = "Find Folders";
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
            OpenOutput.IsEnabled = true;
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
            if (pBar.IsVisible)
            {
                return;
            }
            if (!isFetchingFolders)
            {
                Fetch.Content = "Start Over";
                isFetchingFolders = true;
                Folders = Characters.ToList().Where(c => c.IsChecked).Select(c => c.Item.MetaFile.Id).ToList();
                Status.Content = "Connecting...";
                Characters.Clear();
                pBar.Visibility = Visibility.Visible;
                Task.Run(async () =>
                {
                    var files = await GHandler.FetchFiles(Folders);
                    foreach (var file in files)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Characters.Add(new CheckedListItem<MetaCharacterdisplay>
                            {
                                Item = new MetaCharacterdisplay
                                {
                                    CharacterName = file.Name,
                                    MetaFile = file
                                }
                            });
                        });
                    }

                    Dispatcher.Invoke(() =>
                    {
                        pBar.Visibility = Visibility.Hidden;
                        PrintSel.IsEnabled = true;
                        Status.Content = "Select Files To Print";
                    });
                });
            }
            else
            {
                Status.Content = "Connecting...";
                Fetch.IsEnabled = false;
                Characters.Clear();
                pBar.Visibility = Visibility.Visible;
                var T = new Task(async () =>
                {
                    var files = await GHandler.FetchFolders();
                    foreach (var file in files)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Characters.Add(new CheckedListItem<MetaCharacterdisplay>
                            {
                                Item = new MetaCharacterdisplay
                                {
                                    CharacterName = file.Name,
                                    MetaFile = file
                                }
                            });
                        });
                    }

                    Dispatcher.Invoke(()=>
                    {
                        pBar.Visibility = Visibility.Hidden;
                        Fetch.Content = "Find Files";
                        Status.Content = "Select folders to search";
                        Fetch.IsEnabled = true;
                        isFetchingFolders = false;
                    });
                });
                T.Start();

            }
            
        }

        private void Scroll_List(object sender, MouseWheelEventArgs e)
        {
            ScrollView.ScrollToVerticalOffset(ScrollView.VerticalOffset - e.Delta * 0.5);
            e.Handled = true;
        }
    }
}
