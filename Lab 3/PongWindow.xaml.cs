using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PongGame
{
    public partial class PongWindow : Window
    {
        public PongWindow()
        {
            InitializeComponent();
            DataContext = new Game(); // Подключение логики игры
        }
    }
}