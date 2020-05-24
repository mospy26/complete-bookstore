using Microsoft.AspNet.Identity;

namespace BookStore.WebClient.CustomAuth
{
    public class CustomHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            return Common.Cryptography.sha512encrypt(password);
        }

        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            var genHash = HashPassword(providedPassword);
            if (hashedPassword == genHash)
            {
                return PasswordVerificationResult.Success;
            }
            else
            {
                return PasswordVerificationResult.Failed;
            }
        }
    }
}