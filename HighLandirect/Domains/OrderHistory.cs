using System;
using System.ComponentModel;
using HighLandirect.Foundations;

namespace HighLandirect.Domains
{
    public partial class OrderHistory : IDataErrorInfo, IFormattable
    {

        [NonSerialized]
        private readonly DataErrorSupport dataErrorSupport;

        public OrderHistory()
        {
            // SQL Server Compact does not support entities with server-generated keys or values when it is used 
            // with the Entity Framework. Therefore, we need to create the keys ourselves.
            // See also: http://technet.microsoft.com/en-us/library/cc835494.aspx
            //Id = Guid.NewGuid();

            dataErrorSupport = new DataErrorSupport(this);
        }

        string IDataErrorInfo.Error { get { return dataErrorSupport.Error; } }

        string IDataErrorInfo.this[string memberName] { get { return dataErrorSupport[memberName]; } }

        protected DataErrorSupport DataErrorSupport { get { return dataErrorSupport; } }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            throw new NotImplementedException();
            //return string.Format(formatProvider, Resources.PersonToString, Firstname, Lastname);
        }

    }
}
