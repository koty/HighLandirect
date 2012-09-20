using System;
using System.ComponentModel;
using HighLandirect.Foundations;

namespace HighLandirect.Domains
{
    public partial class Store : IDataErrorInfo, IFormattable
    {

        [NonSerialized]
        private readonly DataErrorSupport dataErrorSupport;

        public Store()
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
            return this.StoreName;
        }

        public override bool Equals(object obj)
        {
            var arg = obj as Store;
            if (arg == null)
                return base.Equals(obj);
            if (this.id != arg.id)
                return false;
            if (this.StoreId1 != arg.StoreId1)
                return false;
            if (this.StoreId2 != arg.StoreId2)
                return false;
            if (this.StoreName != arg.StoreName)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
