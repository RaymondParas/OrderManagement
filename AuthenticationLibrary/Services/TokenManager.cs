using AuthenticationLibrary.Models;
using AuthenticationLibrary.SymmetricCrypt;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace AuthenticationLibrary.Services
{
    public static class TokenManager
    {
        private static readonly string LoginDirectory = "Identification";
        private static readonly string LoginFile = "Login.json";
        private const string SecretKey = "This key is the secret to all secret keys";
        public static readonly SymmetricSecurityKey SigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));

        public static bool IsIdentityRecognized(TokenIdentification identification)
        {
            var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
            var filePath = Path.Combine(basePath.Substring(6), LoginDirectory, LoginFile);

            string validUsername, validPassword;
            using (StreamReader r = File.OpenText(filePath))
            {
                string json = r.ReadToEnd();
                TokenIdentification credentials = JsonConvert.DeserializeObject<TokenIdentification>(json);
                validUsername = EncryptionHelper.Decrypt(credentials.Username);
                validPassword = EncryptionHelper.Decrypt(credentials.Password);
            }

            if (identification.Username.Equals(validUsername) && identification.Password.Equals(validPassword))
                return true;

            return false;
        }
        public static string GenerateToken(TokenIdentification identification, int expireMinutes = 20)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var now = DateTime.UtcNow;
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, identification.Username)
                }),

                Expires = now.AddMinutes(Convert.ToInt32(expireMinutes)),

                SigningCredentials = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256Signature)
            };

            IdentityModelEventSource.ShowPII = true;
            var stoken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(stoken);

            return token;
        }

        public static ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                    return null;

                var validationParameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = SigningKey
                };

                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);

                return principal;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
