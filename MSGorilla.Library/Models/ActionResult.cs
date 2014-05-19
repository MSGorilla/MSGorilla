using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using MSGorilla.Library.Exceptions;
namespace MSGorilla.Library.Models
{
    public class ActionResult
    {
        public int ActionResultCode { get; set; }
        public string Message { get; set; }

        public ActionResult()
        {
            ActionResultCode = 0;
            Message = "success";
        }

        public ActionResult(int code, string message)
        {
            ActionResultCode = code;
            Message = message;
        }

        public ActionResult(MSGorillaBaseException e){
            ActionResultCode = e.Code;
            Message = e.Message;
        }

        public static ActionResult Success()
        {
            return new ActionResult(0, "Success.");
        }
    }
}