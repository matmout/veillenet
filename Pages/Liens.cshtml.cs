using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Models;
using VeilleNet.Services;

namespace VeilleNet.Pages;

public class LiensModel : PageModel
{
    public List<LinkCategory> Categories { get; set; } = new();

    public void OnGet()
    {
        Categories = LinkHelper.GetLinkCategories();
    }
}
