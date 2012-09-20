using System.Collections.Generic;
using System.Linq;
using HighLandirect.Domains;

namespace HighLandirect.Resources
{
    public static class AdditionalData
    {
        public static IList<Store> GetStores()
        {
            var customerEntities = new MyDataEntities();
            return customerEntities.Stores.ToList();
        }
        public static IList<ReportMemo> GetReportMemos()
        {
            var customerEntities = new MyDataEntities();
            return customerEntities.ReportMemos.ToList();
        }
    }
}
