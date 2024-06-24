using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Identity;
using WordGame.Constants;
using WordGame.Constants.Signal_R;
using WordGame.Dtos.Users;
using WordGame.Extensions;
using WordGame.Hubs.Helpers;
using WordGame.Models.Hubs;
using WordGame.Models.Lobbies;

namespace WordGame.Hubs;

[Authorize]
public class LobbyHub : AbpHub
{
    public static readonly List<LobbyHubConnectionModel> Connections = new();
    private readonly IdentityUserManager _identityUserManager;

    public LobbyHub(IdentityUserManager identityUserManager)
    {
        _identityUserManager = identityUserManager;
    }

    public override async Task OnConnectedAsync()
    {
        RemoveObsoleteConnections();

        var channelId = Context.GetHttpContext().Request.Query["channel_id"].ToString();
        var relatedChannel = Lobby.Channels.FirstOrDefault(p => p.Id.Equals(int.Parse(channelId)));

        if (await AddUserToChannelAsync(relatedChannel))
        {
            return;
        }

        Connections.TryAdd(new LobbyHubConnectionModel
        {
            UserId = CurrentUser.Id.GetValueOrDefault(),
            ConnectionId = Context.ConnectionId,
            ChannelId = relatedChannel.Id
        });

        await Groups.AddToGroupAsync(Context.ConnectionId, relatedChannel!.ChannelName);
        await relatedChannel.UpdateActiveUserListAsync(Clients);

        await base.OnConnectedAsync();
    }

    private async Task<bool> AddUserToChannelAsync(Channel? relatedChannel)
    {
        var user = await _identityUserManager.GetByIdAsync((Guid)CurrentUser.Id!);
        if (relatedChannel == null)
        {
            return true;
        }

        relatedChannel.Users.Add(
            new UserWithStatus
            {
                IdentityUser = user,
                UserStatusTypes = UserStatusTypes.Online
            });
        return false;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connection = Connections.Find(c => c.ConnectionId == Context.ConnectionId);
        if (connection is not null)
        {
            var relatedChannel = Lobby.Channels.First(p => p.Id.Equals(connection!.ChannelId));
            await Groups.RemoveFromGroupAsync(connection.ConnectionId, relatedChannel.ChannelName);
            relatedChannel.Users = relatedChannel.Users.Where(p => p.IdentityUser.Id != connection.UserId).ToList();
            Connections.Remove(connection);

            await relatedChannel.UpdateActiveUserListAsync(Clients);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private void RemoveObsoleteConnections()
    {
        var obsoleteConnections = Connections
            .Where(c =>
                c.UserId == CurrentUser.Id.GetValueOrDefault() &&
                c.ConnectionId != Context.ConnectionId
            )
            .ToList();
        if (!obsoleteConnections.IsNullOrEmpty())
        {
            obsoleteConnections.ForEach(obsoleteConnection => { Connections.Remove(obsoleteConnection); });
        }
    }
}