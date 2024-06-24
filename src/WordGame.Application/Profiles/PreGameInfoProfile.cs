using AutoMapper;
using WordGame.Constants;
using WordGame.Dtos.PreGameInfos;
using WordGame.Entities;
using WordGame.Extensions;

namespace WordGame.Profiles;

public class PreGameInfoProfile : Profile
{
    public PreGameInfoProfile()
    {
        CreateMap<PreGameInfo, PreGameInfoDto>()
            .ForMember(c => c.GameTypeName, a =>
                a.MapFrom(c => ((GameTypes)c.GameTypeId).GetDescription()));
    }
}