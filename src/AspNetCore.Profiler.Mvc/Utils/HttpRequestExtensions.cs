using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace AspNetCore.Profiler.Mvc.Utils
{
    public static class HttpRequestExtensions
    {
        public static bool CanSeeMiniProfiler(this HttpRequest request)
        {
            if (!request.Path.ToString().StartsWith("/profiler"))
            {
                return true;
            }

            // Get JWT from header
            var bearerTokenPrefix = "Bearer";
            var accessToken = string.Empty;
            var authorizationHeader = request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith(bearerTokenPrefix, StringComparison.OrdinalIgnoreCase))
            {
                accessToken = authorizationHeader.Replace(bearerTokenPrefix, string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
            }
            else
            {
                return false;
            }


            var baseAuthUrl = "https://localhost:6001";
            JsonWebKeySetResponse jwks = null;

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(baseAuthUrl);
                DiscoveryDocumentResponse discoResponse = httpClient.GetDiscoveryDocumentAsync().Result;
                jwks = httpClient.GetJsonWebKeySetAsync(
                    new JsonWebKeySetRequest
                    {
                        Address = discoResponse.JwksUri,
                        ClientId = "MyBackend",
                        ClientSecret = "secret"
                    }).Result;
            }

            if (jwks.IsError)
                return false;

            var validationParameters = new TokenValidationParameters
            {
                ValidIssuer = baseAuthUrl,
                ValidAudiences = new[] { "MyBackendApi1" },
                IssuerSigningKeys = getSecurityKeys(jwks.KeySet)
            };

            var handler = new JwtSecurityTokenHandler();

            try
            {
                var user = handler.ValidateToken(accessToken, validationParameters, out SecurityToken validatedToken);
                return (user.Identities != null && user.Identities.Any(x => x.IsAuthenticated));
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                // The JWT is invalid
                return false;
            }
        }

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
