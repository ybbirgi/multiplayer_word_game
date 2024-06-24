using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Volo.Abp.AspNetCore.SignalR;
using WordGame.Constants.Signal_R;
using WordGame.Dtos.Users;
using WordGame.Extensions;
using WordGame.Models.Lobbies;

namespace WordGame.Hubs.Helpers;

public static class HubMessageHelper
{
    public static async Task UpdateActiveUserListAsync(this Channel channel,IHubContext<AbpHub> lobbyHubContext)
    {
        var activeUsers = channel.Users.Select(p => new UserDto
        {
            UserId = p.IdentityUser.Id,
            UserName = p.IdentityUser.UserName,
            UserStatusTypeId = (int)p.UserStatusTypes,
            UserStatusTypeName = p.UserStatusTypes.GetDescription()
        }).ToList();

        await lobbyHubContext.Clients.Group(channel!.ChannelName).SendAsync(
            SignalRConstants.GameHubConstants.Methods.ActiveUserList,
            activeUsers
        );
    }
    
    public static async Task UpdateActiveUserListAsync(this Channel channel,IHubCallerClients clients)
    {
        var activeUsers = channel.Users.Select(p => new UserDto
        {
            UserId = p.IdentityUser.Id,
            UserName = p.IdentityUser.UserName,
            UserStatusTypeId = (int)p.UserStatusTypes,
            UserStatusTypeName = p.UserStatusTypes.GetDescription()
        }).ToList();

        await clients.Group(channel!.ChannelName).SendAsync(
            SignalRConstants.GameHubConstants.Methods.ActiveUserList,
            activeUsers
        );
    }
    
    
}