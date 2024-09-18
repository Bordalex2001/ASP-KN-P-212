//using System;

namespace ASP_KN_P_212.Services.Random
{
    using System;

    public class BuiltInRandomService : IRandomService
    {
        private static readonly Random random = new();

        public string GenerateOtp(int length) 
        {
            const string digits = "0123456789";
            return new string(Enumerable.Repeat(digits, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public string GenerateFilename(int length) 
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            //const string reservedChars = "*?/\\|\":<>";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public string GenerateSalt(int length) 
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789~!@#$%^&*()_-+\\|/?<>";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}