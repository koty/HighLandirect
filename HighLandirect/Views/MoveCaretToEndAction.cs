using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace HighLandirect.Views
{
    public class MoveCaretToEndAction : TriggerAction<TextBox>
    {
        protected override void Invoke(object parameter)
        {
            AssociatedObject.CaretIndex = AssociatedObject.Text.Length;
        }
    }
}
