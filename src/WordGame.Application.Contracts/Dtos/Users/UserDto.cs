using System;

namespace WordGame.Dtos.Users;

public class UserDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public int UserStatusTypeId { get; set; }
    public string UserStatusTypeName { get; set; }
}