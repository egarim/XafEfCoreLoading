using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XafEfCoreLoading.Module.BusinessObjects
{

    public static class TestLogger
    {
        public static void WriteLine(string value)
        {
            Debug.WriteLine(value);
            Console.WriteLine(value);
        }

        public static void WriteLine(string format, params object[] args)
        {
            var message = string.Format(format, args);
            Debug.WriteLine(message);
            Console.WriteLine(message);
        }

        public static void WriteHeader(string header)
        {
            var separator = new string('=', header.Length + 6);
            WriteLine(separator);
            WriteLine($"=== {header} ===");
            WriteLine(separator);
        }

        public static void WriteSubHeader(string subHeader)
        {
            WriteLine($"\n--- {subHeader} ---");
        }

        public static void WriteSqlQuery(string message)
        {
            if (message.Contains("Executed DbCommand"))
            {
                WriteLine($"🔍 SQL QUERY: {message}");
            }
            else
            {
                WriteLine(message);
            }
        }
    }
}
