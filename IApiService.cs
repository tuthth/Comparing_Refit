using Refit;

namespace Comparing_Refit
{
    public interface IApiService
    {
        [Get("/csscolornames/colors")]
        Task<List<CSSColorName>> GetAll();
    }
}
