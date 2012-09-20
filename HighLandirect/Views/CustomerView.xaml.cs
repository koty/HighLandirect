using System.Windows.Controls;

namespace HighLandirect.Views
{
    public partial class CustomerView : UserControl
    {
        public CustomerView()
        {
            InitializeComponent();
            this.CustName.TextCapturedPreferredConversion = Microsoft.International.Windows.Controls.PreferredConversion.ToHiragana;
        }
    }
}
