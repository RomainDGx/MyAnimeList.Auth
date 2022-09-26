using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyAnimeList.Auth.WebHost
{
    public class AuthService
    {
        readonly IOptions<AuthServiceOptions> _options;
        readonly IHttpClientFactory _httpClientFactory;
        //readonly ITokenStoreService _tokenStoreService;
        const string _state = "AuthenticationState";

        // TODO: Find how to persist code verifier...
        static string _codeVerifier = string.Empty;

        public AuthService(
            IOptions<AuthServiceOptions> options,
            //ITokenStoreService tokenStoreService,
            IHttpClientFactory httpClientFactory )
        {
            _options = options;
            _httpClientFactory = httpClientFactory;
            //_tokenStoreService = tokenStoreService;
        }

        public string GetAuthenticationUrl()
        {
            // In "plain" code challenge method, the code challenger is the code verifier:
            // https://www.rfc-editor.org/rfc/rfc7636#section-4.2
            string codeChallenge = _codeVerifier = GenerateCodeVerifier();

            Dictionary<string, string?> parameters = new()
            {
                { "response_type", "code" },
                { "client_id", _options.Value.ClientId },
                { "state", _state },
                { "redirect_uri", _options.Value.RedirectUri },
                { "code_challenge", codeChallenge },
                { "code_challenge_method", "plain" }
            };

            return QueryHelpers.AddQueryString( new Uri( _options.Value.AuthServer!, "authorize" ).ToString(), parameters );
        }

        /// <summary>
        /// Generate code verifier according the following specification: <see href="https://datatracker.ietf.org/doc/html/rfc7636#section-4.1"/>.
        /// </summary>
        /// <returns>A code verifier.</returns>
        static string GenerateCodeVerifier()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~";
            Random random = new();
            var codeVerifier = new char[128];

            for( int i = 0; i < codeVerifier.Length; i++ )
            {
                codeVerifier[i] = chars[random.Next( chars.Length )];
            }
            return new string( codeVerifier );
        }

        public async Task GetAccessTokenAsync( string authorizationCode, string state )
        {
            if( state != _state )
            {
                throw new ArgumentException( "Invalid state." );
            }

            using HttpRequestMessage request = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri( _options.Value.AuthServer!, "token"),
                Content = new FormUrlEncodedContent( new Dictionary<string, string>
                {
                    { "client_id", _options.Value.ClientId },
                    { "client_secret", _options.Value.ClientSecret },
                    { "grant_type", "authorization_code" },
                    { "code", authorizationCode },
                    { "redirect_uri", _options.Value.RedirectUri! },
                    // TODO: How to get the code verifier generated in GetAuthenticationUrl()?
                    { "code_verifier", _codeVerifier }
                } )
            };

            using HttpResponseMessage response = await _httpClientFactory.CreateClient().SendAsync( request );

            response.EnsureSuccessStatusCode();

            TokenResponse? token = await JsonSerializer.DeserializeAsync<TokenResponse>( await response.Content.ReadAsStreamAsync() );
        }

        public async Task RefreshAccessTokenAsync( string refreshToken )
        {
            using HttpRequestMessage request = new()
            {
                Method = HttpMethod.Post,
                Headers = { Authorization = new AuthenticationHeaderValue( "Basic", "exempleEXEMPLEExaMpLeExAmPlE" ) },
                RequestUri = new Uri( _options.Value.AuthServer!, "token" ),
                Content = new FormUrlEncodedContent( new Dictionary<string, string>
                {
                    { "grant_type", "refresh_token" },
                    { "refresh_token", refreshToken }
                } )
            };

            using HttpResponseMessage response = await _httpClientFactory.CreateClient().SendAsync( request );

            response.EnsureSuccessStatusCode();

            TokenResponse? token = await JsonSerializer.DeserializeAsync<TokenResponse>( await response.Content.ReadAsStreamAsync() );
        }

        public sealed class AuthServiceOptions
        {
            public string ClientId { get; set; } = string.Empty;
            public string ClientSecret { get; set; } = string.Empty;
            public Uri? AuthServer { get; set; }
            /// <summary>
            ///  If you registered only one redirection URI in advance, you can omit this parameter.
            ///  If you set this, the value must exactly match one of your pre-registered URIs.
            /// </summary>
            public string? RedirectUri { get; set; }
        }
    }
}
