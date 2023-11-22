using DocumentFormat.OpenXml.Office2016.Drawing.Charts;
using System.Net;
using System.Net.Mail;
using TrackingApp.Entities;
using TrackingApp.Interface;
using Microsoft.Extensions.Logging;


namespace TrackingApp.Repository
{
    public class EmailServices : IEmailServices
    {
        private readonly TrackingDbContext _context;
        private readonly ILogger<EmailServices> _logger;

        public EmailServices(TrackingDbContext context, ILogger<EmailServices> logger)
        {
            _context = context;
            _logger = logger;
        }

        string smtpServer = "smtp.office365.com";
        string smtpPort = "587";
        string smtpUsername = "";   //Enter your Microsoft Email Id
        string smtpPassword = "";   // Enter the password associated with the above Microsoft Id

        public bool SendMail()
        {
            ResetWarningCodes(); // Reset WarningCodes before sending emails

            return FirstWarningEmail() && SecondWarningEmail() && ThirdWarningEmail();
        }

        public bool FirstWarningEmail()
        {
            bool result = false;
            SmtpClient client = ConfigureSmtpClient();

            var list = _context.TrackingDB.ToList();
            var records = list.Where(x => x.WarningCode == 0 && (DateTime.Now - x.ExamDate).TotalHours > 24);

            if (records.Any())
            {
                foreach (var record in records)
                {
                    try
                    {
                        var mailAddress = new MailAddress(record.Email);

                        var message = new MailMessage
                        {
                            From = new MailAddress(""),//Enter the mail here which you give above
                            Subject = "First Level Warning",
                            Body = "This is the first level warning.\n Warnings:\n1. First Level: You haven't updated your status for more than 24 hours.\n2. Second Level: You haven't updated your status for more than 72 hours.\n3. Third Level: You haven't updated your status for more than 96 hours(This warning will be escalated)."
                        };

                        message.To.Add(mailAddress.Address);

                        client.Send(message);
                        record.WarningCode = 1;
                        _context.SaveChanges();
                        result = true;
                    }
                    catch (FormatException ex)
                    {
                        _logger.LogError($"Invalid email address: {record.Email}. Error: {ex.Message}");
                    }
                }
            }
            else
            {
                result = true;
            }

            return result;
        }

        public bool SecondWarningEmail()
        {
            bool result = false;
            SmtpClient client = ConfigureSmtpClient();

            var list = _context.TrackingDB.ToList();
            var records = list.Where(x => x.WarningCode == 1 && (DateTime.Now - x.ExamDate).TotalHours > 48);

            if (records.Any())
            {
                foreach (var record in records)
                {
                    try
                    {
                        var mailAddress = new MailAddress(record.Email);

                        var message = new MailMessage
                        {
                            From = new MailAddress(""),//Enter the mail here which you give above
                            Subject = "Second Level Warning",
                            Body = "This is the second level warning.\n Warnings:\n1. First Level: You haven't updated your status for more than 24 hours.\n2. Second Level: You haven't updated your status for more than 72 hours.\n3. Third Level: You haven't updated your status for more than 96 hours(This warning will be escalated)."
                        };

                        message.To.Add(mailAddress.Address);

                        client.Send(message);
                        record.WarningCode = 2;
                        _context.SaveChanges();
                        result = true;
                    }
                    catch (FormatException ex)
                    {
                        _logger.LogError($"Invalid email address: {record.Email}. Error: {ex.Message}");
                    }
                }
            }
            else
            {
                result = true;
            }

            return result;
        }

        public bool ThirdWarningEmail()
        {
            bool result = false;
            SmtpClient client = ConfigureSmtpClient();

            var list = _context.TrackingDB.ToList();
            var records = list.Where(x => x.WarningCode == 2 && (DateTime.Now - x.ExamDate).TotalHours > 96);

            if (records.Any())
            {
                foreach (var record in records)
                {
                    try
                    {
                        var mailAddress = new MailAddress(record.Email);

                        var message = new MailMessage
                        {
                            From = new MailAddress(""),//Enter the mail here which you give above
                            Subject = "Third Level And Final Warning",
                            Body = "This is the third level warning.\n Warnings:\n1. First Level: You haven't updated your status for more than 24 hours.\n2. Second Level: You haven't updated your status for more than 72 hours.\n3. Third Level: You haven't updated your status for more than 96 hours(This warning will be escalated)."
                        };

                        message.To.Add(mailAddress.Address);

                        // Add stakeholders to CC
                        message.CC.Add("");//Enter stakeholder mail here 

                        client.Send(message);
                        record.WarningCode = 3;
                        record.Status = "Escalated";
                        record.Remarks = $"Escalated, and the final warning sent on {DateTime.Now} ";
                        _context.SaveChanges();
                        result = true;
                    }
                    catch (FormatException ex)
                    {
                        _logger.LogError($"Invalid email address: {record.Email}. Error: {ex.Message}");
                    }
                }
            }
            else
            {
                result = true;
            }

            return result;
        }

        private void ResetWarningCodes()
        {
            // Reset WarningCodes to 0 for all records
            var allRecords = _context.TrackingDB.ToList();
            foreach (var record in allRecords)
            {
                record.WarningCode = 0;
            }
            _context.SaveChanges();
        }

        private SmtpClient ConfigureSmtpClient()
        {
            return new SmtpClient(smtpServer, int.Parse(smtpPort))
            {
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = true
            };
        }
    }
}