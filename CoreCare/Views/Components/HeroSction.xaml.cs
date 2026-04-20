using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CoreCare.Views.Components
{
    public partial class HeroSection : UserControl
    {
        public HeroSection()
        {
            InitializeComponent();
        }

        // 1. Esto equivale a tu "prop" hasUser: boolean
        public static readonly DependencyProperty HasUserProperty =
            DependencyProperty.Register("HasUser", typeof(bool), typeof(HeroSection), new PropertyMetadata(false));

        public bool HasUser
        {
            get { return (bool)GetValue(HasUserProperty); }
            set { SetValue(HasUserProperty, value); }
        }

        // 2. Esto equivale a tu "prop" onCategoryClick: () => void
        public static readonly DependencyProperty CategoryClickCommandProperty =
            DependencyProperty.Register("CategoryClickCommand", typeof(ICommand), typeof(HeroSection), new PropertyMetadata(null));

        public ICommand CategoryClickCommand
        {
            get { return (ICommand)GetValue(CategoryClickCommandProperty); }
            set { SetValue(CategoryClickCommandProperty, value); }
        }
    }
}