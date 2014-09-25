using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections;

using log4net;

namespace MSGorilla.IMAPServer.Command
{
    public class CommandParser
    {
        private static readonly Hashtable handlers;
        protected static readonly ILog log = LogManager.GetLogger(typeof(CommandParser));

        static CommandParser()
        {
            handlers = new Hashtable();

            // Search the current assembly for valid IMAP commands
            Type[] types = Assembly.GetExecutingAssembly().GetExportedTypes();

            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(CommandName), false);
                if (attrs.Length == 1)
                {
                    // We have found a Command!
                    CommandName attr = ((CommandName)attrs[0]);

                    if (!type.IsSubclassOf(typeof(BaseCommand)))
                    {
                        throw new Exception("Command Token Attribute not allowed on random classes!");
                    }

                    handlers[attr.Name.Trim().ToUpper()] = type;
                }
            }
        }

        public BaseCommand Parse(string line, out Type type)
        {
            BaseCommand cmd = null;
            type = null;

            // Ensure the line is valid
            if (line == null || line.Trim() == string.Empty)
            {
                return null;
            }

            try
            {
                string[] tokens = line.Split(new char[] { ' ' }, 3);

                if (tokens[1].Equals("UID", StringComparison.InvariantCultureIgnoreCase))
                {
                    tokens = line.Split(new char[] { ' ' }, 4);
                    type = (Type)handlers[tokens[2].Trim().ToUpper()];
                    cmd = (BaseCommand)Activator.CreateInstance(type);
                    cmd.Parse(tokens[0], tokens.Length == 3 ? String.Empty : tokens[3]);
                    if (cmd is IUIDCommand)
                    {
                        ((IUIDCommand)cmd).IsUIDCommand = true;
                    }
                }
                else
                {
                    type = (Type)handlers[tokens[1].Trim().ToUpper()];
                    cmd = (BaseCommand)Activator.CreateInstance(type);
                    cmd.Parse(tokens[0], tokens.Length == 2 ? String.Empty : tokens[2]);
                    
                }
                return cmd;
            }
            catch (Exception e)
            {
                log.Debug(e.Message, e);
                return null;
            }
        }
    }
}
