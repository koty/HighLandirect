using System.ComponentModel;
using System.Windows;
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

        public OrderSource()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                this.ReportMemo = "□□□□　111-222-3333";
                this.CustomerCD = "0260000000-001(001)";
                this.StoreId1 = "001";
                this.StoreId2 = "023";
                this.Order = new Order();
                this.Order.CustomerMasterReceive = new Customer()
                {
                    PrefectureName = "□■□県",
                    Address1 = "あいうえおかきくけこ",
                    Address2 = "さしすせそたちつてと",
                    Address3 = "なにぬねのはひふへほ",
                    Address4 = "まみむめもやゆよわを",
                    CustName = "長野　太郎あいうえおかきく",
                    Phone = "026-888-8888",
                    PostalCD = "1234567"
                };
                this.Order.CustomerMasterSend = new Customer()
                {
                    PrefectureName = "□■□県",
                    Address1 = "あいうえおかきくけこ",
                    Address2 = "さしすせそたちつてと",
                    Address3 = "なにぬねのはひふへほ",
                    Address4 = "まみむめもやゆよわを",
                    CustName = "長野　太郎あいうえおかきく",
                    Phone = "026-888-8888",
                    PostalCD = "1234567"
                };
            }
        }
    }
}
