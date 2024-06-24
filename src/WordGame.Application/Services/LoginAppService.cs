using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.Identity.AspNetCore;
using Volo.Abp.Settings;
using WordGame.Constants;
using WordGame.Dtos.Accounts;
using WordGame.ExceptionCodes.Login;
using WordGame.Localization;
using WordGame.Managers;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace WordGame.Services;

public class LoginAppService : AbpSignInManager, ILoginAppService, ITransientDependency
{
    private readonly IConfiguration _configuration;
    private readonly IStringLocalizer<WordGameResource> _stringLocalizer;
    private readonly ExtendedIdentityUserManager _extendedIdentityUserManager;

    public LoginAppService(
        IdentityUserManager userManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<IdentityUser> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<IdentityUser>> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<IdentityUser> confirmation,
        IOptions<AbpIdentityOptions> options,
        ISettingProvider settingProvider,
        IConfiguration configuration,
        IStringLocalizer<WordGameResource> stringLocalizer,
        ExtendedIdentityUserManager extendedIdentityUserManager
    )
        : base(
            userManager,
            contextAccessor,
            claimsFactory,
            optionsAccessor,
            logger,
            schemes,
            confirmation,
            options,
            settingProvider
        )
    {
        _configuration = configuration;
        _stringLocalizer = stringLocalizer;
        _extendedIdentityUserManager = extendedIdentityUserManager;
    }

    public async Task<LoginResultDto> LoginAsync(
        LoginDto loginDto,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _extendedIdentityUserManager.GetByAsync(
            c =>
                c.UserName.Equals(loginDto.UserName) || c.Email.Equals(loginDto.UserName),
            cancellationToken: cancellationToken
        );
        await CanSignInAsync(user);
        var result = await PasswordSignInAsync(user, loginDto.Password, false, false);
        var loginResult = GetLoginResult(result);
        loginResult.UserId = user.Id;
        var token = await GetTokenAsync(loginDto);
        var tokenDto = new TokenDto
        {
            AccessToken = token.AccessToken,
            TokenType = token.TokenType,
            ExpiresIn = token.ExpiresIn
        };
        return new LoginResultDto
        {
            PasswordLoginResult = loginResult,
            AuthenticationRequired = false,
            Token = tokenDto,
        };
    }

    
    private PasswordLoginResult GetLoginResult(SignInResult result)
    {
        if (result.IsLockedOut)
        {
            return new PasswordLoginResult(LoginResultType.LockedOut);
        }

        if (result.RequiresTwoFactor)
        {
            return new PasswordLoginResult(LoginResultType.RequiresTwoFactor);
        }

        if (result.IsNotAllowed)
        {
            return new PasswordLoginResult(LoginResultType.NotAllowed);
        }

        if (!result.Succeeded)
        {
            return new PasswordLoginResult(LoginResultType.InvalidUserNameOrPassword);
        }

        return new PasswordLoginResult(LoginResultType.Success);
    }
    
    private async Task<TokenResponse> GetTokenAsync(LoginDto loginDto)
    {
        var discoveryCache = new DiscoveryCache(_configuration["AuthServer:Authority"]);
        var documentResponse = await discoveryCache.GetAsync();

        var httpClient = new Lazy<HttpClient>(() => new HttpClient());
        var tokenRequest = new PasswordTokenRequest
        {
            Address = documentResponse.TokenEndpoint,
            ClientId = "WordGame_App",
            UserName = loginDto.UserName,
            Password = loginDto.Password,
            Scope = "WordGame",
        };

        var response = await httpClient.Value.RequestPasswordTokenAsync(tokenRequest);
        return response.IsError
            ? throw new UserFriendlyException(
                _stringLocalizer[LoginExceptionCodes.ErrorDuringTokenRequest])
            : response;
    }
}