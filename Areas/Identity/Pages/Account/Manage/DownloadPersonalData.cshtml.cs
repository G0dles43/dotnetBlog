using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Text;
using System.Threading.Tasks;
using QuestPDF.Helpers;

namespace BlogApp.Areas.Identity.Pages.Account.Manage
{
    public class DownloadPersonalDataModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public DownloadPersonalDataModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var pdfBytes = GenerateUserPdf(user);
            return File(pdfBytes, "application/pdf", "PersonalData.pdf");
        }

        private byte[] GenerateUserPdf(IdentityUser user)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.Content().Column(column =>
                {
                    column.Item().Text("Dane osobowe").FontSize(20).Bold();
                    column.Item().Text($"Nazwa użytkownika: {user.UserName}");
                    column.Item().Text($"Email: {user.Email}");
                    column.Item().Text($"hasło hashowane: {user.PasswordHash}");
                });
            });
        });

            return document.GeneratePdf();
        }
    }
}
