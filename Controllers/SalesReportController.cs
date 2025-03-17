using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SmartStock.DTO.Report;
using SmartStock.Services.DatabaseServices;

namespace SmartStock.Controllers
{
    public class SalesReportController : Controller
    {
        private readonly SalesReportDbService _salesReportDbService;
        public SalesReportController(SalesReportDbService salesReportDbService)
        {
            _salesReportDbService = salesReportDbService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employees")]
        public async Task<IActionResult> Reports()
        {
            var reports = await _salesReportDbService.GetAllReports();
            return View(reports);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employees")]
        public IActionResult CreateReport()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employees")]
        public async Task<IActionResult> CreateReport(Report report)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _salesReportDbService.CreateReport(report.startDate, report.endDate);
                    return RedirectToAction("Reports");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(report);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employees")]
        public async Task<IActionResult> DeleteReport(int reportId)
        {
            await _salesReportDbService.DeleteReport(reportId);
            return RedirectToAction("Reports");
        }

    }
}
