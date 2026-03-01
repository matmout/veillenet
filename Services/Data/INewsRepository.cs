using VeilleNet.Models;
using VeilleNet.Models.Entities;

namespace VeilleNet.Services.Data;

/// <summary>
/// Composite facade interface. Prefer injecting the specific sub-interface
/// (IArticleRepository, IAiSummaryRepository, ISubscriberRepository, INewsletterRepository)
/// in new code.
/// </summary>
public interface INewsRepository : IArticleRepository, IAiSummaryRepository, ISubscriberRepository, INewsletterRepository
{
}
