using System;
using System.Threading.Tasks;
using Volo.Abp.Uow;

namespace WordGame.Extensions;

public static class UnitOfWorkExtension
{
    public static async Task ExecuteInUnitOfWork(this IUnitOfWorkManager unitOfWorkManager,Func<Task> action)
    {
        using var uow = unitOfWorkManager.Begin(true);
        await action();
        await uow.SaveChangesAsync();
        await uow.CompleteAsync();
    }
}