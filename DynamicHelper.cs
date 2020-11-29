using System;

namespace FinancialControl.Web.Common.Extensions
{
    public static class DynamicHelper
    {
        internal static string GetProperty(this object obj, string name)
        {
            try
            {
                string value = "";
                var objValue = obj.GetType().GetProperty(name).GetValue(obj);
                if (objValue != null)
                    value = objValue.ToString();

                return value;
            }
            catch (Exception)
            {
                throw new Exception(string.Format("The specified property does not exist"));
            }
        }
    }
}
