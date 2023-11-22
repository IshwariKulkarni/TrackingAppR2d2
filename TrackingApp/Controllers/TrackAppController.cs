using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TrackingApp.DTO;
using TrackingApp.Entities;
using TrackingApp.Interface;
using TrackingApp.Repository;

namespace TrackingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExcelImportController : ControllerBase
    {
        private readonly ITrackApp _trackApp;

        public ExcelImportController(ITrackApp trackApp)
        {
            _trackApp = trackApp;
           

        }

        [HttpPost("import")]
        public IActionResult ImportExcelData([FromBody] string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return BadRequest("File path is null or empty.");
            }

            bool importResult = _trackApp.ImportExcelData(filePath);

            if (importResult)
            {
                return Ok("Data imported successfully.");
            }

            return BadRequest("Error importing data from Excel.");
        }

        [HttpPost("AddorUpdate")]
        public IActionResult AddOrUpdateViaForm([FromBody] TrackingDTO trackingDTO)
        {
            bool result = _trackApp.AddOrUpdateViaForm(trackingDTO);

            if (result)
            {
                return Ok("Data added or updated successfully.");
            }
            else
            {
                return BadRequest("Error adding or updating data.");
            }
        }

        [HttpGet("searchByStatus")]
        public IActionResult SearchRecordsByStatus([FromQuery] string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                return BadRequest("Invalid status");
            }

            var result = _trackApp.ShowRecordByStatus(status);

            if (result != null && result.Any())
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("No records found for the given status.");
            }
        }

        [HttpGet("searchByEmail")]
        public IActionResult SearchRecordByEmail([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Invalid email");
            }

            var result = _trackApp.ShowRecordByEmail(email);

            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("No record found for the given email.");
            }
        }

        [HttpPut("updateRecord")]
        public IActionResult UpdateRecord([FromQuery] string email, [FromBody] TrackingDTO trackingDTO)
        {
            if (string.IsNullOrEmpty(email) || trackingDTO == null)
            {
                return BadRequest("Invalid data");
            }

            bool result = _trackApp.UpdateRecord(email, trackingDTO);

            if (result)
            {
                return Ok("Record updated successfully.");
            }
            else
            {
                return BadRequest("Error updating record.");
            }
        }


        [HttpDelete("deleteRecord")]
        public IActionResult DeleteRecord([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Invalid email");
            }

            bool result = _trackApp.DeleteRecord(email);

            if (result)
            {
                return Ok("Record deleted successfully.");
            }
            else
            {
                return BadRequest("Error deleting record.");
            }
        }
        [HttpGet]
[Route("SendMail")]
public IActionResult SendMail()
{
    bool result = _emailServices.SendMail();
    if (result)
    {
        return Ok("Warning Emails Sent Successfully");
    }
    else
    {
        return BadRequest("Error sending warning emails.");
    }
}
    }
}
