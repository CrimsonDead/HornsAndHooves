using API.ViewModel.Account;
using DBL.DTOs;
using DBL.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("auth/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<UserDTO> _logger;
        private readonly AuthorizationManager<UserDTO> _authorizationManager;

        public AccountController(
            ILogger<UserDTO> logger,
            AuthorizationManager<UserDTO> authorizationManager)
        {
            _logger = logger;
            _authorizationManager = authorizationManager;
        }

        [HttpPost("signinuser/", Name = "SigninUser")]
        public async Task<ActionResult<TokensOut>> SigninUsrAsync([FromBody] UserAccountSigninIn userCridentials)
        {
            try
            {
                var signInResult = await _authorizationManager.NoPasswordSignInAsync(userCridentials.FirstName, userCridentials.MiddleName, userCridentials.Phone);

                if (signInResult.Succeded)
                {
                    await _authorizationManager.SaveChangesAsync();

                    return Ok(new TokensOut
                    {
                        Refresh = signInResult.RefreshToken,
                        Access = signInResult.AccessToken
                    });
                }
                else
                    return BadRequest(signInResult.Error);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost("signinadmin/", Name = "SigninAdmin")]
        public async Task<ActionResult<TokensOut>> SigninAdminAsync([FromBody] AdminAccountSigninIn userCridentials)
        {
            try
            {
                var signInResult = await _authorizationManager.PasswordSignInAsync(userCridentials.UserName, userCridentials.Password);

                if (signInResult.Succeded)
                {
                    await _authorizationManager.SaveChangesAsync();

                    return Ok(new TokensOut
                    {
                        Refresh = signInResult.RefreshToken,
                        Access = signInResult.AccessToken
                    });
                }
                else
                    return BadRequest(signInResult.Error);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [Authorize]
        [HttpGet("refresh/", Name = "refresh")]
        public async Task<ActionResult<TokensOut>> Refresh()
        {
            try
            {
                var token = HttpContext.Request.Headers.Authorization;

                var refreshResult = await _authorizationManager.RefreshTokenAsync(token);

                if (refreshResult.Succeded)
                {
                    await _authorizationManager.SaveChangesAsync();

                    return Ok(new TokensOut
                    {
                        Refresh = refreshResult.RefreshToken,
                        Access = refreshResult.AccessToken
                    });
                }
                else
                    return BadRequest(refreshResult.Error);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("signout/", Name = "signout")]
        public async Task<ActionResult> Signout()
        {
            try
            {
                var token = HttpContext.Request.Headers.Authorization;

                var signoutResult = await _authorizationManager.SignoutAsync(token);

                if (signoutResult.Succeded)
                {
                    await _authorizationManager.SaveChangesAsync();

                    return Ok();
                }
                else
                    return BadRequest(signoutResult.Error);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
