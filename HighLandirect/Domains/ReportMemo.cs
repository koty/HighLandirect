using System;
using System.ComponentModel;
using HighLandirect.Foundations;

namespace HighLandirect.Domains
{
    public partial class ReportMemo : IDataErrorInfo, IFormattable
    {

        [NonSerialized]
        private readonly DataErrorSupport dataErrorSupport;

        public ReportMemo()
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
            return this.MemoName + this.ReportMemo1;
        }

        public override bool Equals(object obj)
        {
            var arg = obj as ReportMemo;
            if (arg == null)
                return base.Equals(obj);
            if (this.ReportMemoId != arg.ReportMemoId)
                return false;
            if (this.ReportMemo1 != arg.ReportMemo1)
                return false;
            if (this.MemoName != arg.MemoName)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
