using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Identity;
using Volo.Abp.Settings;
using Volo.Abp.Threading;
using WordGame.Constants;
using WordGame.Extensions;
using WordGame.Localization;
using WordGame.Repositories;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace WordGame.Managers;

public class ExtendedIdentityUserManager : IdentityUserManager
{
    private readonly IExtendedIdentityUserRepository _extendedIdentityUserRepository;
    private readonly IStringLocalizer<WordGameResource> _stringLocalizer;

    public ExtendedIdentityUserManager(
        IdentityUserStore store,
        IIdentityRoleRepository roleRepository,
        IIdentityUserRepository userRepository,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<IdentityUser> passwordHasher,
        IEnumerable<IUserValidator<IdentityUser>> userValidators,
        IEnumerable<IPasswordValidator<IdentityUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<IdentityUserManager> logger,
        ICancellationTokenProvider cancellationTokenProvider,
        IOrganizationUnitRepository organizationUnitRepository,
        ISettingProvider settingProvider,
        IExtendedIdentityUserRepository extendedIdentityUserRepository,
        IStringLocalizer<WordGameResource> stringLocalizer
    )
        : base(store, roleRepository, userRepository, optionsAccessor, passwordHasher, userValidators,
            passwordValidators, keyNormalizer, errors, services, logger, cancellationTokenProvider,
            organizationUnitRepository, settingProvider)
    {
        _extendedIdentityUserRepository = extendedIdentityUserRepository;
        _stringLocalizer = stringLocalizer;
    }

    public virtual async Task<bool> ExistsBy(
        Expression<Func<IdentityUser, bool>> predicate,
        string exceptionMessage,
        bool throwIfNotExists = false,
        CancellationToken cancellationToken = default)
    {
        var isExist = await _extendedIdentityUserRepository.AnyAsync(
            predicate,
            cancellationToken
        );
        if (throwIfNotExists && !isExist)
        {
            throw new UserFriendlyException(
                _stringLocalizer[exceptionMessage]);
        }

        return isExist;
    }

    public virtual async Task<bool> CheckAlreadyExists(
        Expression<Func<IdentityUser, bool>> predicate,
        string exceptionMessage,
        bool throwIfExists = false,
        CancellationToken cancellationToken = default
    )
    {
        var isExist = await _extendedIdentityUserRepository.AnyAsync(predicate, cancellationToken);
        if (throwIfExists && isExist)
        {
            throw new UserFriendlyException(_stringLocalizer[exceptionMessage]);
        }

        return isExist;
    }

    public async Task<IdentityUser> GetByAsync(
        Expression<Func<IdentityUser, bool>> predicate,
        bool includeDetails = false,
        CancellationToken cancellationToken = default
    )
    {
        var entity = await _extendedIdentityUserRepository
            .GetByAsync(
                predicate,
                includeDetails: includeDetails,
                cancellationToken: cancellationToken);
        if (entity == null)
        {
            throw new UserFriendlyException(LoginResultType.InvalidUserNameOrPassword.GetDescription());
        }

        return entity;
    }

    public async Task<List<IdentityUser>> GetListByAsync(
        Expression<Func<IdentityUser, bool>> predicate,
        bool includeDetails = false,
        CancellationToken cancellationToken = default
    )
    {
        return await _extendedIdentityUserRepository
            .GetListByAsync(
                predicate,
                includeDetails: includeDetails,
                cancellationToken: cancellationToken);
    }
}