namespace ASP_KN_P_212.Services.Hash
{
    public class Md5HashService : IHashService
    {
        public String Digest(String input) => Convert.ToHexString(
            System.Security.Cryptography.MD5.HashData(
                System.Text.Encoding.UTF8.GetBytes(input)));
        
    }
}
