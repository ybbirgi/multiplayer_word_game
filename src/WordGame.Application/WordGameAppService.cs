using WordGame.Localization;
using Volo.Abp.Application.Services;

namespace WordGame;

/* Inherit your application services from this class.
 */
public abstract class WordGameAppService : ApplicationService
{
    protected WordGameAppService()
    {
        LocalizationResource = typeof(WordGameResource);
    }
}
