using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;

namespace HighLandirect.Behaviors
{
    /// <summary>
    /// http://blog.sharplab.net/computer/cprograming/wpf/1862/
    /// より拝借
    /// </summary>
    public class TextBoxBehavior
    {

        #region Command

        public static ICommand GetCommand(TextBox textBox)
        {
            return (ICommand)textBox.GetValue(CommandProperty);
        }

        public static void SetCommand(
          TextBox textBox, bool value)
        {
            textBox.SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(TextBoxBehavior),
            new UIPropertyMetadata(null, OnCommandPropertyChanged));

        static void OnCommandPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            TextBox textBox = depObj as TextBox;
            if (textBox == null)
                return;

            if (e.NewValue is ICommand == false)
                return;

            ICommand command = (ICommand)e.NewValue;

            textBox.KeyDown += new KeyEventHandler(OnTextBoxKeyDown);
        }

        static void OnTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = (TextBox)e.OriginalSource;
            ICommand command = TextBoxBehavior.GetCommand(textBox);
            if (e.Key == Key.Enter && command != null && command.CanExecute(TextBoxBehavior.GetCommandParameter(textBox)))
            {
                command.Execute(TextBoxBehavior.GetCommandParameter(textBox));
            }
        }

        #endregion

        #region CommandParameter

        public static object GetCommandParameter(TextBox textBox)
        {
            return textBox.GetValue(CommandParameterProperty);
        }

        public static void SetCommandParameter(
          TextBox textBox, object value)
        {
            textBox.SetValue(CommandParameterProperty, value);
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached(
            "CommandParameter",
            typeof(object),
            typeof(TextBoxBehavior),
            new UIPropertyMetadata(null));

        #endregion
    }
}
