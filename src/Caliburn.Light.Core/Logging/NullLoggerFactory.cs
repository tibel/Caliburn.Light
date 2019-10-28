using System;

namespace Caliburn.Light
{
    internal sealed class NullLoggerFactory : ILoggerFactory
    {
        public static readonly NullLoggerFactory Instance = new NullLoggerFactory();
        private static readonly ILogger _nullLoger = new NullLogger();

        private NullLoggerFactory()
        {
        }

        public ILogger GetLogger(Type type)
        {
            return _nullLoger;
        }

        private sealed class NullLogger : ILogger
        {
            public void Info(string format, params object[] args)
            {
            }

            public void Warn(string format, params object[] args)
            {
            }

            public void Error(string format, params object[] args)
            {
            }
        }
    }
}
