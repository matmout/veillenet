using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Models;
using VeilleNet.Services;

namespace VeilleNet.Pages;

public class RadarModel : PageModel
{
    private readonly IFrameworkVersionService _versionService;

    public List<FrameworkVersion> Versions { get; set; } = new();
    public List<FrameworkVersion> EndingSoonVersions { get; set; } = new();
    public List<string> Frameworks { get; set; } = new();

    public RadarModel(IFrameworkVersionService versionService)
    {
        _versionService = versionService;
    }

    public async Task OnGetAsync()
    {
        Versions = await _versionService.GetAllVersionsAsync();
        EndingSoonVersions = _versionService.GetEndingSoonVersions(6);
        Frameworks = _versionService.GetFrameworkNames();
    }
}
