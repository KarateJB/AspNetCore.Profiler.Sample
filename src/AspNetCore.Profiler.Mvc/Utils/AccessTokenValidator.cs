using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;

namespace AspNetCore.Profiler.Mvc.Utils
{
    /// <summary>
    /// Access token validator
    /// </summary>
    public class AccessTokenValidator
    {
        /// <summary>
        /// Validate JWT with Auth Server (e.q. Idsrv4)
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <returns>True(valid)/False(Invalid)</returns>
        public static async Task<bool> ValidateAsync(string accessToken)
        {
            const string BASE_AUTH_URL = "https://localhost:6001";
            const string CLIENT_ID = "MyBackend";
            const string CLIENT_SECRET = "secret";

            JsonWebKeySetResponse jwks = null;

            #region Get JWKs

            /*
             * JWKs is available on https://auth_server/.well-known/openid-configuration/jwks
             */

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(BASE_AUTH_URL);
                DiscoveryDocumentResponse discoResponse = httpClient.GetDiscoveryDocumentAsync().Result;
                jwks = httpClient.GetJsonWebKeySetAsync(
                    new JsonWebKeySetRequest
                    {
                        Address = discoResponse.JwksUri,
                        ClientId = CLIENT_ID,
                        ClientSecret = CLIENT_SECRET
                    }).Result;
            }

            if (jwks.IsError)
                return await Task.FromResult(false);

            #endregion

            #region Access token validation

            var validationParameters = new TokenValidationParameters
            {
                ValidIssuer = BASE_AUTH_URL,
                ValidAudiences = new[] { "MyBackendApi1" },
                IssuerSigningKeys = getSecurityKeys(jwks.KeySet)
            };

            var handler = new JwtSecurityTokenHandler();

            try
            {
                var user = handler.ValidateToken(accessToken, validationParameters, out SecurityToken validatedToken);
                var isAuthenticated = user.Identities != null && user.Identities.Any(x => x.IsAuthenticated);
                return await Task.FromResult(isAuthenticated);
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                // The JWT is invalid
                return await Task.FromResult(false);
            }
            #endregion
        }

        /// <summary>
        /// Convert JsonWebKeySet to List<SecurityKey>
        /// </summary>
        /// <param name="jsonWebKeySet">IdentityModel.Jwk.JsonWebKeySet</param>
        /// <returns>SecurityKey list</returns>
        private static List<SecurityKey> getSecurityKeys(IdentityModel.Jwk.JsonWebKeySet jsonWebKeySet)
        {
            var keys = new List<SecurityKey>();

            foreach (var key in jsonWebKeySet.Keys)
            {
                if (key.Kty == "RSA")
                {
                    if (key.X5c != null && key.X5c.Count() > 0)
                    {
                        string certificateString = key.X5c[0];
                        var certificate = new X509Certificate2(Convert.FromBase64String(certificateString));

                        var x509SecurityKey = new X509SecurityKey(certificate)
                        {
                            KeyId = key.Kid
                        };

                        keys.Add(x509SecurityKey);
                    }
                    else if (!string.IsNullOrWhiteSpace(key.E) && !string.IsNullOrWhiteSpace(key.N))
                    {
                        byte[] exponent = fromBase64Url(key.E);
                        byte[] modulus = fromBase64Url(key.N);

                        var rsaParameters = new RSAParameters
                        {
                            Exponent = exponent,
                            Modulus = modulus
                        };

                        var rsaSecurityKey = new RsaSecurityKey(rsaParameters)
                        {
                            KeyId = key.Kid
                        };

                        keys.Add(rsaSecurityKey);
                    }
                    else
                    {
                        throw new Exception("JWK data is missing in token validation");
                    }
                }
                else
                {
                    throw new NotImplementedException("Only RSA key type is implemented for token validation");
                }
            }

            return keys;
        }

        private static byte[] fromBase64Url(string base64Url)
        {
            string padded = base64Url.Length % 4 == 0
                ? base64Url : base64Url + "====".Substring(base64Url.Length % 4);
            string base64 = padded.Replace("_", "/")
                                  .Replace("-", "+");
            var s = Convert.FromBase64String(base64);
            return s;
        }
    }
}
