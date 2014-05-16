using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSGorilla.Library.Models;

namespace MSGorilla.Library.Exceptions
{
    public class TwitterBaseException : System.Exception
    {
        public string Description { get; set; }
        public int Code { get; set; }

        public TwitterBaseException()
        {
            Description = "Unknown Exception.";
            Code = 1000;
        }

        public ActionResult toActionResult()
        {
            return new ActionResult(Code, Description);
        }
    }

    public class UserNotFoundException : TwitterBaseException
    {
        public UserNotFoundException(string userid)
        {
            Description = string.Format("User {0} doesn't exist.", userid);
            Code = 2000;
        }
    }

    public class UserAlreadyExistException : TwitterBaseException
    {
        public UserAlreadyExistException(string userid)
        {
            Description = string.Format("Userid {0} already exists.", userid);
            Code = 2001;
        }
    }

    public class LoginFailException : TwitterBaseException
    {
        public LoginFailException()
        {
            Description = string.Format("Login Fail. Wrong Password or Username.");
            Code = 2003;
        }
    }

    public class AccessDenyException : TwitterBaseException
    {
        public AccessDenyException()
        {
            Description = string.Format("Access Denied. Please Login.");
            Code = 2004;
        }
    }

    public class MessageTooLongException : TwitterBaseException
    {
        public MessageTooLongException()
        {
            Description = "Message is too long. Should be less than 256 byte";
            Code = 3001;
        }
    }

    public class MessageNotFoundException : TwitterBaseException
    {
        public MessageNotFoundException()
        {
            Description = "The specified tweed doesn't exist.";
            Code = 3002;
        }
    }

    public class RetweetARetweetException : TwitterBaseException
    {
        public RetweetARetweetException()
        {
            Description = "Can't retweet a retweet.";
            Code = 3003;
        }
    }
}
