using OV.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace OV.Pyramid
{
    /// <summary>
    /// Interaction logic for YGOView.xaml
    /// </summary>
    public partial class YGOView : UserControl
    {
        public YGOView()
        {
            InitializeComponent();
            FirstLoad();
        }

        private void FirstLoad()
        {
            Frame.Source = Images.GetImage(Utilities.GetLocationPath() + @"\Template\Frame\Normal.png");
            if (Frame.Source == null)
            {
                MessageBox.Show("here");
            }
        }
    }
}
