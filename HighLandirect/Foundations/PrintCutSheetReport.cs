using System.Collections.Generic;
using System.Windows.Controls;
using System.Printing;
using System.Windows.Xps;
using System.Windows.Documents;
using System.Windows.Markup;

namespace HighLandirect.Foundations
{
    public class PrintCutSheetReport<T1, T2> where T2 : UserControl, new()
    {
        private readonly PrintQueue pq;
        public PrintCutSheetReport(PrintQueue pq)
        {
            this.pq = pq;
        }


        public void Print(IEnumerable<T1> Records)
        {
            //Set up the WPF Control to be printed
            return;
        }

        private void SendFixedDocumentToPrinter(FixedDocument fixedDocument)
        {
            XpsDocumentWriter xpsdw;

            if (pq == null)
            {
                PrintDocumentImageableArea imgArea = null;
                //こちらのオーバーロードだと、プリンタ選択ダイアログが出る。
                xpsdw = PrintQueue.CreateXpsDocumentWriter(ref imgArea);
            }
            else
            {
                xpsdw = PrintQueue.CreateXpsDocumentWriter(pq);
            }
            xpsdw.Write(fixedDocument);
        }
    }
}
