using PotatoNV_next.Utils;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PotatoNV_next.Controls
{
    public partial class ImageButton : UserControl
    {
        public event EventHandler ButtonClicked;

        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon",
            typeof(string),
            typeof(ImageButton),
            new PropertyMetadata(OnPropertyChangedCallback)
        );

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(ImageButton),
            new PropertyMetadata(OnPropertyChangedCallback)
        );

        private static void OnPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as ImageButton).OnChanged();
        }

        protected virtual void OnChanged()
        {
            if (Text == null || Icon == null)
            {
                return;
            }

            textNode.Text = Text;
            imageNode.Source = MediaConverter.ImageSourceFromBitmap(MediaConverter.GetBitmapByName(Icon));
        }

        public ImageButton()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, EventArgs e)
        {
            ButtonClicked?.Invoke(sender, e);
        }
    }
}
