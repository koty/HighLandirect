using System;
using System.ComponentModel;
using HighLandirect.Foundations;

namespace HighLandirect.Domains
{
    public partial class Order : IDataErrorInfo, IFormattable
    {

        [NonSerialized]
        private readonly DataErrorSupport dataErrorSupport;

        public Order()
        {
            // SQL Server Compact does not support entities with server-generated keys or values when it is used 
            // with the Entity Framework. Therefore, we need to create the keys ourselves.
            // See also: http://technet.microsoft.com/en-us/library/cc835494.aspx
            //Id = Guid.NewGuid();

            dataErrorSupport = new DataErrorSupport(this);
        }

        public Order(long OrderID)
        {
            dataErrorSupport = new DataErrorSupport(this);

            this.OrderID = OrderID;
        }
        public Order(long OrderID, int SendCustNo, int ResceiveCustNo, int ProductID)
        {
            // SQL Server Compact does not support entities with server-generated keys or values when it is used 
            // with the Entity Framework. Therefore, we need to create the keys ourselves.
            // See also: http://technet.microsoft.com/en-us/library/cc835494.aspx
            //Id = Guid.NewGuid();

            dataErrorSupport = new DataErrorSupport(this);

            this.OrderID = OrderID;
            this.ReceiveCustID = ReceiveCustID;
            this.SendCustID = SendCustID;
            this.ProductID = ProductID;
        }

        string IDataErrorInfo.Error { get { return dataErrorSupport.Error; } }

        string IDataErrorInfo.this[string memberName] { get { return dataErrorSupport[memberName]; } }

        protected DataErrorSupport DataErrorSupport { get { return dataErrorSupport; } }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            //throw new NotImplementedException();
            //return string.Format(formatProvider, Resources.PersonToString, Firstname, Lastname);
            if (this.CustomerMasterSend == null || this.CustomerMasterReceive == null)
            {
                return "";
            }
            return this.CustomerMasterSend.CustName + " → " + this.CustomerMasterReceive.CustName;
        }


    }
}
