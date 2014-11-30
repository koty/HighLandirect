using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Printing;
using System.Windows.Xps;
using System.Windows.Documents;
using System.Windows.Markup;
using HighLandirect.Domains.Reports;
using HighLandirect.Domains;
using System;

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


        public void Print(IEnumerable<T1> Records, Func<IEnumerable<T1>, object> createDataContext)
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
                ReportToPrint.DataContext = createDataContext(RecordsPerPage);

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
