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

namespace boby_multitools.Resources.Global_Resources
{
    /// <summary>
    /// Logique d'interaction pour Window.xaml
    /// </summary>
    public partial class Windows : UserControl
    {
        public Windows()
        {
            InitializeComponent();
        }

        public string Title
        {
            get
            {
                return this.title.Content.ToString();
            }
            set
            {
                this.title.Content = value.ToUpper();
                this.title_double_shadow.Content = value.ToUpper();
            }
        }

        private void title_bar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tmp = (Grid)this.Parent;
            Window win = tmp.Parent as Window;
            win.DragMove();
        }
    }
}
