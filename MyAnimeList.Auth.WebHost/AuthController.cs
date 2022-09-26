using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace MyAnimeList.Auth.WebHost
{
    [Route( "api/[controller]" )]
    public class AuthController : ControllerBase
    {
        readonly AuthService _authService;

        public AuthController( AuthService authService )
        {
            _authService = authService;
        }

        [HttpGet( "getAuthentication" )]
        public IActionResult GetAuthentication()
        {
            return Redirect( _authService.GetAuthenticationUrl() );
        }

        /// <summary>
        /// The endpoint targeted by redirect URI.
        /// </summary>
        /// <param name="code">The authorization code returned from the initial request.</param>
        /// <param name="state">The state value you send the request with.</param>
        /// <returns>A redirect URI to the MyAnimeList authorization.</returns>
        [HttpGet( "authorize" )]
        public async Task<IActionResult> GetAuthorizeAsync( [FromQuery] string code, [FromQuery] string state )
        {
            try
            {
                await _authService.GetAccessTokenAsync( code, state );

                return Redirect( "/" );
            }
            catch( ArgumentException e )
            {
                return Unauthorized( e.Message );
            }
            catch
            {
                throw;
            }
        }

        [HttpGet( "refresh" )]
        public async Task GetRefreshToken( [FromQuery] string refreshToken )
        {
            await _authService.RefreshAccessTokenAsync( refreshToken );
        }
    }
}
