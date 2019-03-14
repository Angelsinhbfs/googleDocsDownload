using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Shapes;
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

            DisplayList.ItemsSource = Characters;
        }

        private void PrintSelOnClick(object sender, RoutedEventArgs e)
        {
            var pdfs = GHandler.GetPdfDocuments(Characters.Where(c => c.IsChecked).Select(c => c.Item.MetaFile).ToList());
            Console.WriteLine("test");
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

        }
    }
}
