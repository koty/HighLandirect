using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using HighLandirect.Foundations;
using Livet;

namespace HighLandirect.Domains
{
    public partial class Customer : IDataErrorInfo, IFormattable
    {
        private static readonly Regex emailValidationRegex = new Regex(@"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$", 
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public const string HeaderCsv = "CustNo	Furigana	CustName	Keisho	CityName	PostalCD	PrefectureCD	PrefectureName	RegionCD	RegionName	Address1	Address2	Address3	Address4	Phone	Fax	Phone2	MailAddress	Memo	Label	LatestSend	LatestResceive	Delete";

        [NonSerialized]
        private readonly DataErrorSupport dataErrorSupport;

        
        public Customer(int CustNo) : this()
        {
            this.CustNo = CustNo;
        }

        public Customer()
        {
            dataErrorSupport = new DataErrorSupport(this);
                //.AddValidationRule("Email", ValidateEmail);
            this.PostalCD = "";
            this.Keisho = "様";
            this.CustName = "";
            this.Furigana = "";
            this.Phone = "";
            this.Phone2 = "";
        }


        string IDataErrorInfo.Error { get { return dataErrorSupport.Error; } }

        string IDataErrorInfo.this[string memberName] { get { return dataErrorSupport[memberName]; } }

        protected DataErrorSupport DataErrorSupport { get { return dataErrorSupport; } }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            throw new NotImplementedException();
            //return string.Format(formatProvider, Resources.PersonToString, Firstname, Lastname);
        }


        //private string ValidateEmail(object objectInstance, string memberName)
        //{
        //    if (!string.IsNullOrEmpty(Email))
        //    {
        //        if (Email.Length > 100)
        //        {
        //            return string.Format(CultureInfo.CurrentCulture, Resources.EmailMaxLength, 100);
        //        }
        //        if (!emailValidationRegex.IsMatch(Email))
        //        {
        //            return Resources.EmailInvalid;
        //        }
        //    }
        //    return "";
        //}

        public void SetAddress(PostalCDGetter postalConverter)
        {
            this.PrefectureName = postalConverter.Prefecture;
            //this.CityName = postalConverter.City;
            this.Address1 = postalConverter.City;
            this.Address2 = postalConverter.Address;
        }

        public static Customer GetCustomerFromCsv(string strCsvLine, int CustNo)
        {
            var items = strCsvLine.Split('\t');
            var customer = CreateCustomer(0, "");
            int CustNoForImport;
            int i = 0;
            if (!int.TryParse(items[i].Replace(".00", ""), out CustNoForImport))
                CustNoForImport = CustNo; //parseに失敗したら外からもらう
            customer.CustNo = CustNoForImport; i++;
            customer.Furigana = GetItem(items, "Furigana", i); i++;
            customer.CustName = GetItem(items, "CustName", i); i++;
            customer.Keisho = GetItem(items, "Keisho", i); i++;
            customer.CityName = GetItem(items, "customer", i); i++;
            customer.PostalCD = GetItem(items, "CityName", i); i++;
            customer.PrefectureCD = GetItem(items, "PrefectureCD", i); i++;
            customer.PrefectureName = GetItem(items, "PrefectureName", i); i++;
            customer.RegionCD = GetItem(items, "RegionCD", i); i++;
            customer.RegionName = GetItem(items, "RegionName", i); i++;
            customer.Address1 = GetItem(items, "Address1", i); i++;
            customer.Address2 = GetItem(items, "Address2", i); i++;
            customer.Address3 = GetItem(items, "Address3", i); i++;
            customer.Address4 = GetItem(items, "Address4", i); i++;
            customer.Phone = GetItem(items, "Phone", i); i++;
            customer.Fax = GetItem(items, "Phone", i); i++;
            customer.Phone2 = GetItem(items, "Phone", i); i++;
            customer.MailAddress = GetItem(items, "Phone", i); i++;
            customer.Memo = GetItem(items, "Phone", i); i++;
            customer.Label = GetBoolItem(items, "Label", i); i++;
            customer.Delete = false; //規定値
            try
            {
                /*customer.Delete = GetBoolItem(items, "Delete", i);*/ i++;
                customer.LatestSend = GetDateItem(items, "LatestSend", i); i++;
                customer.LatestResceive = GetDateItem(items, "LatestResceive", i); i++;
            }
            catch (Exception)
            {
            }
            return customer;
        }

        private static string GetItem(string[] items, string ColumnName, int i)
        {
            try
            {
                return items[i].Replace(".00", "").Trim('　').Trim(' ');
            }
            catch (IndexOutOfRangeException IOoRex)
            {
                IOoRex.Source = ColumnName + ":" + i.ToString() + "列目";
                throw;
            }
            catch (InvalidCastException InvalidCastEx)
            {
                InvalidCastEx.Source = ColumnName + ":" + i.ToString() + "列目:" + items[i];
                throw;
            }
        }
        private static bool GetBoolItem(string[] items, string ColumnName, int i)
        {
            try
            {
                if (items[i].Trim().Length == 0)
                    return false;
                if (items[i] == "0") return false;
                if (items[i] == "1") return true;

                return bool.Parse(items[i]);
            }
            catch (IndexOutOfRangeException IOoRex)
            {
                IOoRex.Source = ColumnName;
                throw;
            }
            catch (InvalidCastException ICEx)
            {
                ICEx.Source = ColumnName;
                throw;
            }
        }

        private static DateTime? GetDateItem(string[] items, string ColumnName, int i)
        {
            try
            {
                if (items[i].Trim().Length == 0)
                    return null;
                return DateTime.Parse(items[i]);
            }
            catch (IndexOutOfRangeException IOoRex)
            {
                IOoRex.Source = ColumnName;
                throw;
            }
            catch (InvalidCastException ICEx)
            {
                ICEx.Source = ColumnName;
                throw;
            }
        }

        public static string GetCsvFromCustomer(Customer customer)
        {
            var Line = new System.Text.StringBuilder();
            Line.Append(customer.CustNo).Append("\t")
                .Append(customer.Furigana).Append("\t")
                .Append(customer.CustName).Append("\t")
                .Append(customer.Keisho).Append("\t")
                .Append(customer.CityName).Append("\t")
                .Append(customer.PostalCD).Append("\t")
                .Append(customer.PrefectureCD).Append("\t")
                .Append(customer.PrefectureName).Append("\t")
                .Append(customer.RegionCD).Append("\t")
                .Append(customer.RegionName).Append("\t")
                .Append(customer.Address1).Append("\t")
                .Append(customer.Address2).Append("\t")
                .Append(customer.Address3).Append("\t")
                .Append(customer.Address4).Append("\t")
                .Append(customer.Phone).Append("\t")
                .Append(customer.Fax).Append("\t")
                .Append(customer.Phone2).Append("\t")
                .Append(customer.MailAddress).Append("\t")
                .Append(customer.Memo).Append("\t")
                .Append(customer.Label).Append("\t")
                .Append(customer.LatestSend).Append("\t")
                .Append(customer.LatestResceive).Append("\t")
                .Append(customer.Delete);
            return Line.ToString();
        }

        public string PostalCD1
        {
            get
            {
                return SubStringPostalCD(this.PostalCD, 0);
            }
        }
        public string PostalCD2
        {
            get
            {
                return SubStringPostalCD(this.PostalCD, 1);
            }
        }
        public string PostalCD3
        {
            get
            {
                return SubStringPostalCD(this.PostalCD, 2);
            }
        }
        public string PostalCD4
        {
            get
            {
                return SubStringPostalCD(this.PostalCD, 3);
            }
        }
        public string PostalCD5
        {
            get
            {
                return SubStringPostalCD(this.PostalCD, 4);
            }
        }
        public string PostalCD6
        {
            get
            {
                return SubStringPostalCD(this.PostalCD, 5);
            }
        }
        public string PostalCD7
        {
            get
            {
                return SubStringPostalCD(this.PostalCD, 6);
            }
        }

        private static string SubStringPostalCD(string PostalCD, int Position)
        {
            if (PostalCD.Length > Position)
                return PostalCD.Substring(Position, 1);
            else
                return "";
        }
    }
}
