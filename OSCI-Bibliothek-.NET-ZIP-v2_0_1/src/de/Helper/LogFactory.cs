using System;

namespace Osci.Helper
{
    public abstract class LogFactory
    {
        public abstract string[] AttributeNames
        {
            get;
        }
        public static LogFactory Factory
        {
            get
            {
                return null;
            }

        }

        public static Log GetLog(Type type)
        {
            return new Log(type);
        }

        public static Log GetLog(string name)
        {
            return new Log(name);
        }
    }
}