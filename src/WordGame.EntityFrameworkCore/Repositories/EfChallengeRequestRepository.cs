using System;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using WordGame.Entities;
using WordGame.EntityFrameworkCore;

namespace WordGame.Repositories;

public class EfChallengeRequestRepository : EfCoreRepository<WordGameDbContext,ChallengeRequest,Guid>,IChallengeRequestRepository
{
    public EfChallengeRequestRepository(IDbContextProvider<WordGameDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }
}