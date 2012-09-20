using System.Net;
using System.Xml;

namespace HighLandirect.Domains
{
    public class PostalCDGetter
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public PostalCDGetter()
        {
        }
        private const string ApiUri = "http://zip.cgis.biz/xml/zip.php?zn={0}";
        public void GetAddress(string postalCD)
        {
            var req = HttpWebRequest.Create(string.Format(ApiUri, postalCD.Replace("-", "")));
            req.Method = "GET";

            var res = req.GetResponse();
            var doc = new XmlDocument();
            using (var s = res.GetResponseStream())
            {
                doc.Load(s);
            }

            foreach (XmlNode n in doc.GetElementsByTagName("value"))
            {
                switch(n.Attributes[0].Name)
                {
                    case "state":
                        this.Prefecture = n.Attributes[0].Value;
                        break;
                    case "city":
                        this.City = n.Attributes[0].Value;
                        break;
                    case "address":
                        this.Address = n.Attributes[0].Value;
                        break;
                    default:
                        break;
                }
            }
        }

        public string Prefecture{get;set;}
        public string City{get;set;}
        public string Address{get;set;}
    }
}
