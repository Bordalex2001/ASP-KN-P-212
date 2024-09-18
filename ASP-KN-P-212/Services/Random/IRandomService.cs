namespace ASP_KN_P_212.Services.Random
{
    public interface IRandomService
    {
        string GenerateOtp(int length);
        string GenerateFilename(int length);
        string GenerateSalt(int length);
    }
}
