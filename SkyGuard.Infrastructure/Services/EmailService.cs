using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using SkyGuard.Core.Models;
using SkyGuard.Core.Services;
using System.Net;
using System.Net.Mail;

namespace SkyGuard.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        [Obsolete]
        private readonly IHostingEnvironment _env;

        [Obsolete]
        public EmailService(IConfiguration config, IHostingEnvironment env)
        {
            _config = config;
            _env = env;
        }

        public async Task SendIncidentAssignedEmail(User securityUser, Incident incident)
        {
            var emailSubject = "New Incident Assigned to You";
            var appBaseUrl = _config["AppBaseUrl"];
            var incidentUrl = $"{appBaseUrl}/incidents/{incident.Id}";

            var emailBody = $"""
                <html>
                <body style="font-family: 'HelvLight', Helvetica, Arial, sans-serif; font-size: 10pt; color: darkblue;">
                    <p>Dear {securityUser.Name},</p>

                    <p>
                        You have been assigned a new incident for immediate attention. Kindly review the incident details below and take the necessary actions as soon as possible.
                    </p>

                    <div style="margin: 20px 0; padding: 15px; border-left: 4px solid #007acc; background-color: #f4f9ff;">
                        <h2 style="margin-top: 0; font-size: 12pt; color: darkblue;">{incident.Title}</h2>
                        <p><strong>Priority:</strong> {incident.Priority}</p>
                        <p><strong>Location:</strong> {incident.PathLine}</p>
                        <p><strong>Reported Date:</strong> {incident.ReportedAt:dddd, MMMM d, yyyy h:mm tt}</p>
                    </div>

                    <p>
                        Please click the button below to view and manage this incident:
                    </p>

                    <a href="{incidentUrl}" style="display: inline-block; padding: 10px 20px; background-color: #007acc; color: white; text-decoration: none; border-radius: 4px; font-size: 10pt;">
                        View Incident
                    </a>

                    <p style="margin-top: 30px;">
                        If you have any questions or need further clarification, kindly reach out to the Incident Management Team.
                    </p>

                    <p style="color: #555; font-size: 9pt;">
                        This is an automated message. Please do not reply to this email.
                    </p>

                    <p style="margin-top: 40px;">Best regards,<br/>Sentinel Monitoring System</p>
                </body>
                </html>
                """;

            await SendEmail(securityUser.Email, emailSubject, emailBody);
        }


        public async Task SendResponseSubmittedEmail(string recipientEmail, string recipientName, Guid incidentId)
        {
            var emailSubject = "Incident Response Submitted";
            var appBaseUrl = _config["AppBaseUrl"];
            var incidentUrl = $"{appBaseUrl}/incidents/{incidentId}";

            var emailBody = $"""
                <html>
                <body style="font-family: 'HelvLight', Helvetica, Arial, sans-serif; font-size: 10pt; color: darkblue;">
                    <p>Dear {recipientName},</p>

                    <p>
                        A response has been successfully submitted for the incident with ID: <strong>{incidentId}</strong>.
                    </p>

                    <p>
                        You may review the submitted response and follow up if necessary by clicking the link below:
                    </p>

                    <a href="{incidentUrl}" style="display: inline-block; padding: 10px 20px; background-color: #007acc; color: white; text-decoration: none; border-radius: 4px; font-size: 10pt;">
                        View Incident Response
                    </a>

                    <p style="margin-top: 30px;">
                        If you have any questions or concerns, please contact the Incident Management Team.
                    </p>

                    <p style="margin-top: 40px;">Best regards,<br/>SkyGuard SecureFlow Team</p>

                    <p style="color: #555; font-size: 9pt; margin-top: 20px;">
                        This is an automated message. Please do not reply to this email.
                    </p>
                </body>
                </html>
                """;

            await SendEmail(recipientEmail, emailSubject, emailBody);
        }


        private async Task SendEmail(string toEmail, string subject, string body)
        {
            var smtpHost = _config["Email:SmtpHost"];
            var smtpPort = int.Parse(_config["Email:SmtpPort"]!);
            var smtpUser = _config["Email:SmtpUser"];
            var smtpPass = _config["Email:SmtpPass"];
            var fromEmail = _config["Email:FromEmail"];
            var fromName = _config["Email:FromName"];

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
    }
}
