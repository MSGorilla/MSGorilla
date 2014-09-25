using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.IMAPServer.Command
{
    public interface IUIDCommand
    {
        bool IsUIDCommand { get; set; }
    }
}
