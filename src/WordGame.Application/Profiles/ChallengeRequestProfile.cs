using AutoMapper;
using Volo.Abp.Identity;
using WordGame.Constants;
using WordGame.Dtos.Challenges;
using WordGame.Entities;
using WordGame.Extensions;

namespace WordGame.Profiles;

public class ChallengeRequestProfile : Profile
{
    public ChallengeRequestProfile()
    {
        CreateMap<ChallengeRequest, ChallengeRequestDto>()
            .ForMember(c => c.SenderUserName,
                a =>
                    a.Ignore())
            .ForMember(c => c.ReceiverUserName,
                a =>
                    a.Ignore())
            .ForMember(c => c.ChallengeRequestStatusName,
                a =>
                    a.MapFrom(c => ((ChallengeStatus)c.ChallengeStatusId).GetDescription()))
            .AfterMap<ChallengeRequestDtoMappingAction>();
    }

    public class ChallengeRequestDtoMappingAction : IMappingAction<ChallengeRequest, ChallengeRequestDto>
    {
        private readonly IdentityUserManager _identityUserManager;

        public ChallengeRequestDtoMappingAction(IdentityUserManager identityUserManager)
        {
            _identityUserManager = identityUserManager;
        }

        public void Process(ChallengeRequest source, ChallengeRequestDto destination, ResolutionContext context)
        {
            var senderUser = _identityUserManager.GetByIdAsync(source.SenderUserId).GetAwaiter().GetResult();
            var receiverUser = _identityUserManager.GetByIdAsync(source.ReceiverUserId).GetAwaiter().GetResult();
            destination.SenderUserName = senderUser.UserName;
            destination.ReceiverUserName = receiverUser.UserName;
        }
    }
}