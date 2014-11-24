using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Woker
{
    class Program
    {
        static void Main(string[] args)
        {
            WorkerRole role = new WorkerRole();
            role.OnStart();
            role.Run();
        }
    }
}
