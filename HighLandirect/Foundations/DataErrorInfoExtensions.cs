using System;
using System.ComponentModel;

namespace HighLandirect.Foundations
{
    public static class DataErrorInfoExtensions
    {
        public static string Validate(this IDataErrorInfo obj)
        {
            if (obj == null) { throw new ArgumentNullException("obj"); }

            return obj.Error;
        }
        
        public static string Validate(this IDataErrorInfo obj, string memberName)
        {
            if (obj == null) { throw new ArgumentNullException("obj"); }

            return obj[memberName];
        }
    }
}
