using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrackingApp.DTO;
using TrackingApp.Entities;
using TrackingApp.Interface;

namespace TrackingApp.Repository
{
    public class TrackApp : ITrackApp
    {
        private readonly TrackingDbContext _context;
        private readonly ILogger<TrackApp> _logger;

        public TrackApp(TrackingDbContext context, ILogger<TrackApp> logger)
        {
            _context = context;
            _logger = logger;
        }

        public bool ImportExcelData(string path)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (string.IsNullOrEmpty(path))
            {
                _logger.LogError("File path is null or empty.");
                return false;
            }

            try
            {
                _logger.LogInformation("Logging SQL statements...");

                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream, new ExcelReaderConfiguration { FallbackEncoding = Encoding.UTF8 }))
                    {
                        reader.Read(); // Assume the first row is the header

                        while (reader.Read())
                        {
                            var email = reader.GetValue(0)?.ToString();
                            var name = reader.GetValue(1)?.ToString();
                            var mentor = reader.GetValue(2)?.ToString();
                            var course = reader.GetValue(3)?.ToString();
                            var status = reader.GetValue(4)?.ToString();
                            var remarks = reader.GetValue(5)?.ToString();
                            var examDateString = reader.GetValue(6)?.ToString();

                            if (DateTime.TryParseExact(examDateString, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var examDate))
                            {
                                try
                                {
                                    var existingEntry = _context.TrackingDB.FirstOrDefault(t => t.Email == email);

                                    if (existingEntry == null)
                                    {
                                        var newEntry = new TrackingDB
                                        {
                                            Email = email,
                                            Name = name,
                                            Mentor = mentor,
                                            Course = course,
                                            Status = status,
                                            Remarks = remarks,
                                            ExamDate = examDate
                                        };

                                        _context.Entry(newEntry).State = EntityState.Added;

                                        // Set WarningCode based on conditions
                                        if ((string.IsNullOrEmpty(status) || status.ToLower() == "no") && DateTime.UtcNow - examDate >= TimeSpan.FromHours(24))
                                        {
                                            newEntry.WarningCode = DateTime.UtcNow - examDate >= TimeSpan.FromHours(48) ? 1 : 0;
                                        }

                                        _context.SaveChanges();
                                        _logger.LogInformation($"Number of changes saved to the database: 1");
                                    }
                                    else
                                    {
                                        // Your existing code...

                                        // Set WarningCode based on conditions
                                        if ((string.IsNullOrEmpty(status) || status.ToLower() == "no") && DateTime.UtcNow - examDate >= TimeSpan.FromHours(24))
                                        {
                                            existingEntry.WarningCode = DateTime.UtcNow - examDate >= TimeSpan.FromHours(48) ? 1 : 0;
                                        }

                                        _context.SaveChanges();
                                        _logger.LogInformation($"Number of changes saved to the database: 1");
                                    }
                                }
                                catch (DbUpdateException dbEx)
                                {
                                    _logger.LogError($"Database update error: {dbEx.Message}");
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"Error processing Excel row: {ex.Message}");
                                }
                            }
                            else
                            {
                                _logger.LogError($"Error parsing date: {examDateString}");
                            }
                        }

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error importing data from Excel: {ex.Message}");
                _logger.LogError($"StackTrace: {ex.StackTrace}");
                return false;
            }
        }


        public bool AddOrUpdateViaForm(TrackingDTO trackingDTO)
        {
            try
            {
                var existingEntry = _context.TrackingDB.FirstOrDefault(t => t.Email == trackingDTO.Email);

                if (existingEntry == null)
                {
                    var newEntry = new TrackingDB
                    {
                        Email = trackingDTO.Email,
                        Name = trackingDTO.Name,
                        Mentor = trackingDTO.Mentor,
                        Course = trackingDTO.Course,
                        Status = trackingDTO.Status,
                        Remarks = trackingDTO.Remarks,
                        ExamDate = trackingDTO.ExamDate
                    };

                    newEntry.WarningCode = (string.IsNullOrEmpty(newEntry.Status) || newEntry.Status.ToLower() == "no") && DateTime.UtcNow - newEntry.ExamDate >= TimeSpan.FromHours(24) ?
                        (DateTime.UtcNow - newEntry.ExamDate >= TimeSpan.FromHours(48) ? 1 : 0) :
                        0;

                    // Set WarningDateTime when WarningCode is set
                    newEntry.WarningDateTime = DateTime.UtcNow + TimeSpan.FromHours(newEntry.WarningCode == 1 ? 48 : (newEntry.WarningCode == 0 ? 24 : 0));

                    _context.Entry(newEntry).State = EntityState.Added;
                    _context.SaveChanges();
                    _logger.LogInformation($"Number of changes saved to the database: 1");
                }
                else
                {
                    // Your existing code...

                    // Set WarningCode based on conditions
                    existingEntry.WarningCode = (string.IsNullOrEmpty(trackingDTO.Status) || trackingDTO.Status.ToLower() == "no") && DateTime.UtcNow - trackingDTO.ExamDate >= TimeSpan.FromHours(24) ?
                        (DateTime.UtcNow - trackingDTO.ExamDate >= TimeSpan.FromHours(48) ? 1 : 0) :
                        0;

                    // Set WarningDateTime when WarningCode is set
                    existingEntry.WarningDateTime = DateTime.UtcNow + TimeSpan.FromHours(existingEntry.WarningCode == 1 ? 48 : (existingEntry.WarningCode == 0 ? 24 : 0));

                    _context.SaveChanges();
                    _logger.LogInformation($"Number of changes saved to the database: 1");
                }

                return true;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError($"Database update error: {dbEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding or updating tracking data: {ex.Message}");
                return false;
            }
        }

        public List<TrackingDB> ShowRecordByStatus(string status)
        {
            var result = _context.TrackingDB.Where(x => x.Status == status).ToList();
            return result;
        }

        public TrackingDB ShowRecordByEmail(string email)
        {
            var result = _context.TrackingDB.FirstOrDefault(x => x.Email == email);
            return result;
        }

        public bool UpdateRecord(string email, TrackingDTO trackingDTO)
        {
            try
            {
                var existingEntry = _context.TrackingDB.FirstOrDefault(t => t.Email == email);

                if (existingEntry == null)
                {
                    _logger.LogError($"Record with email {email} not found for updating.");
                    return false;
                }

                existingEntry.Name = trackingDTO.Name;
                existingEntry.Mentor = trackingDTO.Mentor;
                existingEntry.Course = trackingDTO.Course;
                existingEntry.Status = trackingDTO.Status;
                existingEntry.Remarks = trackingDTO.Remarks;
                existingEntry.ExamDate = trackingDTO.ExamDate;

                // Set WarningCode based on conditions
                existingEntry.WarningCode = (string.IsNullOrEmpty(existingEntry.Status) || existingEntry.Status.ToLower() == "no") && DateTime.UtcNow - existingEntry.ExamDate >= TimeSpan.FromHours(24) ?
                    (DateTime.UtcNow - existingEntry.ExamDate >= TimeSpan.FromHours(48) ? 1 : 0) :
                    0;

                // Set WarningDateTime when WarningCode is set
                existingEntry.WarningDateTime = DateTime.UtcNow + TimeSpan.FromHours(existingEntry.WarningCode == 1 ? 48 : (existingEntry.WarningCode == 0 ? 24 : 0));

                _context.SaveChanges();
                _logger.LogInformation($"Number of changes saved to the database: 1");

                return true;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError($"Database update error: {dbEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating tracking data: {ex.Message}");
                return false;
            }
        }


        public bool DeleteRecord(string email)
        {
            try
            {
                var existingEntry = _context.TrackingDB.FirstOrDefault(t => t.Email == email);

                if (existingEntry == null)
                {
                    _logger.LogError($"Record with email {email} not found for deletion.");
                    return false;
                }

                _context.TrackingDB.Remove(existingEntry);
                _context.SaveChanges();
                _logger.LogInformation($"Record with email {email} deleted successfully.");

                return true;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError($"Database update error: {dbEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting tracking data: {ex.Message}");
                return false;
            }
        }

    }
}
