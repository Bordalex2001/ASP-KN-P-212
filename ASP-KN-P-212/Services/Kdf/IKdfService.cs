namespace ASP_KN_P_212.Services.Kdf
{
    public interface IKdfService
    {
        String DerivedKey(String salt, String password);
    }
}
/* Key Derivation service by RFC 2898 
 * 
 */
