using System.Globalization;
using System.Net;
using BuyMyHouse.Domain.Entities;
using MortgageSender.Application.Services;

namespace MortgageSender.Application.Offers;

public static class OfferEmailComposer
{
    public static EmailMessage Compose(User user, MortgageApplication application, Uri offerLink, DateTimeOffset linkExpiresOn)
    {
        var name = string.IsNullOrWhiteSpace(user.FullName) ? "applicant" : user.FullName.Trim();
        var applied = application.CreatedAt.ToString("d MMMM yyyy", CultureInfo.InvariantCulture);
        // Storage SAS expiry is in UTC, so say so rather than guess the recipient's time zone.
        var expires = linkExpiresOn.UtcDateTime.ToString("d MMMM yyyy 'at' HH:mm 'UTC'", CultureInfo.InvariantCulture);
        var link = offerLink.ToString();

        var text =
            $"""
             Dear {name},

             Your mortgage offer for the application you submitted on {applied} is ready.

             View your offer (PDF): {link}

             For your privacy, this link works until {expires}. After that, please contact us for a new one.
             The offer is indicative; the final mortgage depends on verification of your details and the property.

             Kind regards,
             BuyMyHouse

             Application reference: {application.RowKey}
             """;

        var html =
            $"""
             <!DOCTYPE html>
             <html>
             <body style="font-family: Arial, Helvetica, sans-serif; font-size: 14px; color: #222; line-height: 1.5;">
               <p>Dear {Html(name)},</p>
               <p>Your mortgage offer for the application you submitted on {Html(applied)} is ready.</p>
               <p style="margin: 24px 0;">
                 <a href="{Html(link)}" style="background: #1976d2; color: #fff; padding: 10px 18px; border-radius: 4px; text-decoration: none;">View your mortgage offer</a>
               </p>
               <p>For your privacy, this link works until <strong>{Html(expires)}</strong>. After that, please contact us for a new one.</p>
               <p style="color: #666; font-size: 12px;">The offer is indicative; the final mortgage depends on verification of your details and the property.</p>
               <p>Kind regards,<br>BuyMyHouse</p>
               <p style="color: #999; font-size: 11px;">Application reference: {Html(application.RowKey)}</p>
             </body>
             </html>
             """;

        return new EmailMessage(user.Email, name, "Your BuyMyHouse mortgage offer is ready", html, text);
    }

    private static string Html(string value) => WebUtility.HtmlEncode(value);
}
