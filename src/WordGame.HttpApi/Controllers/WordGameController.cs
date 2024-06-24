using WordGame.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace WordGame.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class WordGameController : AbpControllerBase
{
    protected WordGameController()
    {
        LocalizationResource = typeof(WordGameResource);
    }
}
