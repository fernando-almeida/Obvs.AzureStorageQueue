using System;
using System.Text;

namespace Obvs.AzureStorageQueue.Tests.Extensions
{
    public static class StringExtensions
    {
        public static string ToBase64String(this string source) {
            return Convert.ToBase64String(Encoding.Unicode.GetBytes(source));
        }
    }
}