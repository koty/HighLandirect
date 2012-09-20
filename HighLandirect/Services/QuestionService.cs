using System.Windows;
using System.Globalization;

namespace HighLandirect.Services
{
    public class QuestionService : IQuestionService
    {
        private static MessageBoxOptions MessageBoxOptions
        {
            get
            {
                return (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft) ? MessageBoxOptions.RtlReading : MessageBoxOptions.None;
            }
        }


        public bool? ShowQuestion(string message)
        {
            MessageBoxResult result = MessageBox.Show(message, "HighLandirect", MessageBoxButton.YesNoCancel, 
                MessageBoxImage.Question, MessageBoxResult.Cancel, MessageBoxOptions);

            if (result == MessageBoxResult.Yes) { return true; }
            else if (result == MessageBoxResult.No) { return false; }

            return null;
        }

        public bool ShowYesNoQuestion(string message)
        {
            MessageBoxResult result = MessageBox.Show(message, "HighLandirect", MessageBoxButton.YesNo,
                MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions);

            return result == MessageBoxResult.Yes;
        }
    }
}
