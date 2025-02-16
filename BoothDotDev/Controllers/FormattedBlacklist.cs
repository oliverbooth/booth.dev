using System.Text;
using BoothDotDev.Common.Data.Web;
using BoothDotDev.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace BoothDotDev.Controllers;

[Controller]
[Route("contact/blacklist/formatted")]
public class FormattedBlacklistController : Controller
{
    private readonly IContactService _contactService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FormattedBlacklistController" /> class.
    /// </summary>
    /// <param name="contactService">The <see cref="IContactService" />.</param>
    public FormattedBlacklistController(IContactService contactService)
    {
        _contactService = contactService;
    }

    [HttpGet("{format}")]
    public IActionResult OnGet([FromRoute] string format)
    {
        IReadOnlyCollection<IBlacklistEntry> blacklist = _contactService.GetBlacklist();

        switch (format)
        {
            case "json":
                return Json(blacklist);

            case "csv":
                var builder = new StringBuilder();
                builder.AppendLine("EmailAddress,Name,Reason");
                foreach (IBlacklistEntry entry in blacklist)
                {
                    builder.AppendLine($"{entry.EmailAddress},{entry.Name},{entry.Reason}");
                }

                return Content(builder.ToString(), "text/csv", Encoding.UTF8);
        }

        return NotFound();
    }
}