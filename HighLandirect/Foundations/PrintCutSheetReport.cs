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

            var fixedDoc = new FixedDocument();
            foreach (var order in Records)
            {
                var objReportToPrint = new T2();

                var ReportToPrint = objReportToPrint as UserControl;
                ReportToPrint.DataContext = order;

                var pageContent = new PageContent();
                var fixedPage = new FixedPage();

                //Create first page of document
                fixedPage.Children.Add(ReportToPrint);
                ((IAddChild)pageContent).AddChild(fixedPage);
                fixedDoc.Pages.Add(pageContent);
            }

            this.SendFixedDocumentToPrinter(fixedDoc);
        }

        private void SendFixedDocumentToPrinter(FixedDocument fixedDocument)
        {
            XpsDocumentWriter xpsdw
                         = PrintQueue.CreateXpsDocumentWriter(pq);
            xpsdw.Write(fixedDocument);
        }
    }
}
