using System.Runtime.CompilerServices;
using Volo.Abp.Identity;
using WordGame.Constants;

namespace WordGame.Models.Lobbies;

public class UserWithStatus
{
    public IdentityUser IdentityUser { get; set; }
    public UserStatusTypes UserStatusTypes { get; set; }

    public void UpdateStatus(UserStatusTypes userStatusType)
    {
        UserStatusTypes = userStatusType;
    }
}