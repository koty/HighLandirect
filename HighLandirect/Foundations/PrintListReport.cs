using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Printing;
using System.Windows.Xps;
using System.Windows.Documents;
using System.Windows.Markup;

namespace HighLandirect.Foundations
{
    public class PrintListReport<T1, T2> where T2 : UserControl, new()
    {
        private readonly int ListCountPerPage;
        private readonly PrintQueue pq;
        public PrintListReport(int ListCountPerPage, PrintQueue pq)
        {
            this.ListCountPerPage = ListCountPerPage;
            this.pq = pq;
        }


        public void Print(IEnumerable<T1> Records)
        {
            //Set up the WPF Control to be printed
            //ListCountPerPageごとにリストを作ってDataContextに渡す

            FixedDocument fixedDoc = new FixedDocument();
            for(int i = 0 ; i < Records.Count() ; i += ListCountPerPage)
            {
                var RecordsPerPage = new List<T1>();
                int j = 0;
                foreach (var Record in Records)
                {
                    if (i <= j && j < i + ListCountPerPage)
                        RecordsPerPage.Add(Record);
                    j++;
                }

                var objReportToPrint = new T2();

                var ReportToPrint = objReportToPrint as UserControl;
                ReportToPrint.DataContext = RecordsPerPage;

                PageContent pageContent = new PageContent();
                FixedPage fixedPage = new FixedPage();

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
