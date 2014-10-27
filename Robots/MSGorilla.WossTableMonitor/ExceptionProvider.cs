using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSGorilla.Library.Models.SqlModels;

namespace MSGorilla.WossTableMonitor
{
    public class ExceptionProvider
    {
        public enum FunctionName{
            Execute,
            ExecuteQuerySegmented,
            ExecuteRetriveOperation
        }

        public static int GetExceptionCount(FunctionName func)
        {
            string funcName = func.ToString();
            if (funcName.Equals("ExecuteQuerySegmented"))
            {
                funcName += "<TElement>";
            }

            using (var ctx = new MSGorillaEntities())
            {
                return ctx.AWExceptions.Where(e => e.Function == funcName).Count();
            }
        }
    }
}
