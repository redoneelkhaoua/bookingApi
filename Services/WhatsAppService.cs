namespace server.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using server.Models;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

public class WhatsAppService
{
    private readonly IConfiguration _config;
    private readonly ILogger<WhatsAppService> _logger;
    private readonly string? _accountSid;
    private readonly string? _authToken;
    private readonly string? _fromNumber;

    public WhatsAppService(IConfiguration config, ILogger<WhatsAppService> logger)
    {
        _config = config;
        _logger = logger;
        _accountSid = _config["Twilio:AccountSid"];
        _authToken = _config["Twilio:AuthToken"];
        _fromNumber = _config["Twilio:WhatsAppFrom"];

        if (!string.IsNullOrEmpty(_accountSid) && !string.IsNullOrEmpty(_authToken) && _accountSid != "AC_dummy_account_sid")
        {
            TwilioClient.Init(_accountSid, _authToken);
        }
    }

    public async Task SendConfirmationAsync(Appointment appointment)
    {
        var message = $@"✅ Rendez-vous confirmé!
📋 Service: {appointment.Service?.Name ?? "Consultation"}
📅 Date: {appointment.StartTime:dd/MM/yyyy} à {appointment.StartTime:HH:mm}
👨‍⚕️ {appointment.Staff?.Name ?? "Non assigné"}
📍 {appointment.Business?.Name ?? "Demo Clinic"}

Pour annuler: http://localhost:5173/cancel/{appointment.CancelToken}";

        await SendMessageAsync(appointment.ClientPhone, message);
    }

    public async Task SendReminderAsync(Appointment appointment)
    {
        var message = $@"⏰ Rappel de rendez-vous demain!
📋 {appointment.Service?.Name ?? "Consultation"}
📅 Demain à {appointment.StartTime:HH:mm}
👨‍⚕️ {appointment.Staff?.Name ?? "Non assigné"}
📍 {appointment.Business?.Name ?? "Demo Clinic"}

Pour annuler: http://localhost:5173/cancel/{appointment.CancelToken}";

        await SendMessageAsync(appointment.ClientPhone, message);
    }

    public async Task SendCancellationAsync(Appointment appointment)
    {
        var message = $@"❌ Votre rendez-vous a été annulé.
📋 {appointment.Service?.Name ?? "Consultation"} prévu le {appointment.StartTime:dd/MM/yyyy} à {appointment.StartTime:HH:mm} a été annulé.";

        await SendMessageAsync(appointment.ClientPhone, message);
    }

    private async Task SendMessageAsync(string toPhone, string message)
    {
        if (string.IsNullOrEmpty(_accountSid) || _accountSid == "AC_dummy_account_sid")
        {
            _logger.LogInformation($"[MOCK WHATSAPP] To: {toPhone}\nMessage:\n{message}");
            return;
        }

        try
        {
            // Ensure phone number has international format if needed, Twilio requires + prefix.
            var formattedPhone = toPhone.StartsWith("+") ? toPhone : $"+{toPhone}";
            
            await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_fromNumber),
                to: new PhoneNumber($"whatsapp:{formattedPhone}")
            );
            _logger.LogInformation($"WhatsApp message sent to {formattedPhone}");
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to send WhatsApp message");
        }
    }
}
