namespace HighLandirect.Domains.Reports
{
    public class OrderSource
    {
        public string ReportMemo { get; set; }
        public string CustomerCD { get; set; }
        public string StoreId1 { get; set; }
        public string StoreId2 { get; set; }

        public Order Order { get; set; }

        public OrderSource(Order order)
        {
            this.ReportMemo = "";
            this.CustomerCD = "";
            this.StoreId1 = "";
            this.StoreId2 = "";
            this.Order = order;
        }
    }
}
