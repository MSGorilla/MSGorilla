using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSGorilla.Library.Models;

namespace MSGorilla.Library.Exceptions
{
    public class MSGorillaBaseException : System.Exception
    {
        public string Description { get; set; }
        public int Code { get; set; }

        public MSGorillaBaseException()
        {
            Description = "Unknown Exception.";
            Code = 1000;
        }

        public ActionResult toActionResult()
        {
            return new ActionResult(Code, Description);
        }
    }

    public class InvalidIDException : MSGorillaBaseException
    {
        public InvalidIDException()
        {
            Description = "Invalid ID. ID should be [0-9a-zA-Z]+ .";
            Code = 1001;
        }

        public InvalidIDException(string type)
        {
            Description = string.Format("Invalid {0} ID. Please refer to http://msdn.microsoft.com/library/azure/dd179338.aspx", type);
            Code = 1001;
        }
    }

    public class UserNotFoundException : MSGorillaBaseException
    {
        public UserNotFoundException(string userid)
        {
            Description = string.Format("User {0} doesn't exist.", userid);
            Code = 2000;
        }
    }

    public class UserAlreadyExistException : MSGorillaBaseException
    {
        public UserAlreadyExistException(string userid)
        {
            Description = string.Format("Userid {0} already exists.", userid);
            Code = 2001;
        }

        public UserAlreadyExistException(string userid, string Description)
        {
            Code = 2001;
            this.Description = Description;
        }
    }

    public class LoginFailException : MSGorillaBaseException
    {
        public LoginFailException()
        {
            Description = string.Format("Login Fail. Wrong Password or Username.");
            Code = 2003;
        }
    }

    public class AccessDenyException : MSGorillaBaseException
    {
        public AccessDenyException()
        {
            Description = string.Format("Access Denied. Please Login.");
            Code = 2004;
        }
    }

    public class MessageTooLongException : MSGorillaBaseException
    {
        public MessageTooLongException()
        {
            Description = "Message is too long. Should be less than 256 byte";
            Code = 3001;
        }
    }

    public class MessageNotFoundException : MSGorillaBaseException
    {
        public MessageNotFoundException()
        {
            Description = "The specified message doesn't exist.";
            Code = 3002;
        }
    }

    public class MessageNullException : MSGorillaBaseException
    {
        public MessageNullException()
        {
            Description = "Message can't be null";
            Code = 3003;
        }
    }

    public class InvalidMessageIDException : MSGorillaBaseException
    {
        public InvalidMessageIDException()
        {
            Description = "Invalid Message ID";
            Code = 3004;
        }
    }


    //public class RetweetARetweetException : TwitterBaseException
    //{
    //    public RetweetARetweetException()
    //    {
    //        Description = "Can't retweet a retweet.";
    //        Code = 3003;
    //    }
    //}
    public class SchemaNotFoundException : MSGorillaBaseException
    {
        public SchemaNotFoundException()
        {
            Description = "The specified schema doesn't exist.";
            Code = 4001;
        }
    }

    public class SchemaAlreadyExistException : MSGorillaBaseException
    {
        public SchemaAlreadyExistException()
        {
            Description = "The specified schema already exists. Change the schema ID";
            Code = 4002;
        }
    }
}
