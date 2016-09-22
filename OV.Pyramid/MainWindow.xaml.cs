using System.Windows;
using System.Windows.Documents;
using OV.Tools;
using System.Windows.Input;

namespace OV.Pyramid
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            FirstLoad();
        }

        private void FirstLoad()
        {
            //Set default global Style for Paragraph: Margin = 0            
            Style style = new Style(typeof(Paragraph));
            style.Setters.Add(new Setter(Paragraph.MarginProperty, new Thickness(0)));
            Resources.Add(typeof(Paragraph), style);
        }

        
    }
}
