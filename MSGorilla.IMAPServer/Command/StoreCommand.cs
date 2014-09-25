using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.IMAPServer.Command
{
    [CommandName("STORE")]
    public class StoreCommand : BaseCommand, IUIDCommand
    {
        public bool IsUIDCommand { get; set; }
    }
}
