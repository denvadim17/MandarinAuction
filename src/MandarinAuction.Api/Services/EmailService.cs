namespace MandarinAuction.Api.Services;

public class EmailSettings
{
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "noreply@mandarin-auction.com";
    public string FromName { get; set; } = "Mandarin Auction";
}

public class EmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(EmailSettings settings, ILogger<EmailService> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task SendBidOutbidNotificationAsync(string toEmail, string userName, string mandarinName, decimal newBid)
    {
        var subject = $"Ставка перебита: «{mandarinName}»";
        var body = $"""
            <p>Здравствуйте, {userName}.</p>
            <p>Вашу ставку на «{mandarinName}» перебили.</p>
            <p>Текущая ставка: <b>{newBid:C}</b></p>
            <p>Если хотите, можно сделать новую ставку в личном кабинете.</p>
            """;
        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendPurchaseReceiptAsync(string toEmail, string userName, string mandarinName,
        decimal price, decimal cashback)
    {
        var subject = $"Чек по покупке: «{mandarinName}»";
        var body = $"""
            <p>Здравствуйте, {userName}.</p>
            <p>Покупка оформлена успешно.</p>
            <hr/>
            <h3>Чек</h3>
            <table>
                <tr><td>Товар:</td><td>{mandarinName}</td></tr>
                <tr><td>Цена:</td><td>{price:C}</td></tr>
                <tr><td>Кэшбек:</td><td>{cashback:C}</td></tr>
            </table>
            <p>Спасибо.</p>
            """;
        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendAuctionWonAsync(string toEmail, string userName, string mandarinName, decimal winningBid)
    {
        var subject = $"Вы выиграли лот «{mandarinName}»";
        var body = $"""
            <p>Здравствуйте, {userName}.</p>
            <p>Аукцион по лоту «{mandarinName}» завершен в вашу пользу.</p>
            <p>Итоговая ставка: <b>{winningBid:C}</b></p>
            <p>Лот закреплен за вами.</p>
            """;
        await SendEmailAsync(toEmail, subject, body);
    }

    private async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        try
        {
            if (string.IsNullOrEmpty(_settings.SmtpUser))
            {
                _logger.LogWarning("Почта не настроена (пустой SmtpUser). Письмо не отправлено: {To} — {Subject}", to, subject);
                return;
            }

            using var message = new MimeKit.MimeMessage();
            message.From.Add(new MimeKit.MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(MimeKit.MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new MimeKit.BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.SmtpUser, _settings.SmtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Письмо отправлено: {To} — {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось отправить письмо: {To}", to);
        }
    }
}
