using System;

namespace ProviderCancellationRule
{
    class ConsoleLogger : ILogger
    {
        public void Info(string message)
        {
            Console.WriteLine("{0} INF: {1}", DateTime.Now, message);
        }

        public void Warning(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine( "{0} WRN: {1}", DateTime.Now, message );
            Console.ResetColor();
        }

        public void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("{0} ERR: {1}", DateTime.Now, message);
            Console.ResetColor();
        }
    }
}