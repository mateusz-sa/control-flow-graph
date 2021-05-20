using PathfindingInControlFlowGraph.Models;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PathfindingInControlFlowGraph
{
    /// <summary>
    /// Logika interakcji dla klasy ImageViewer.xaml
    /// </summary>
    public partial class ImageViewer : Window
    {
        public ImageViewer()
        {
            InitializeComponent();
            Load();
            
        }
        private void Load()
        {
            Uri _imageUri = new Uri(@"H:\Projekty\ControlFlowGraphGenerator\PathfindingInControlFlowGraph\PathfindingInControlFlowGraph\Files\graf.png");
            src.Source = _imageUri.GetBitmapImage(BitmapCacheOption.OnLoad);
        }
        
    }
}
