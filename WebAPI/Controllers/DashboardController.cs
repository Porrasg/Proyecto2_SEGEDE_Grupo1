using CoreApp;
using Entities_DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        [HttpGet]
        [Route("Admin")]
        public ActionResult Admin()
        {
            try
            {
                
                var turbineManager = new TurbineManager();
                var centralBankManager = new CentralBankManager();
                var forecastManager = new ForecastManager();
                var distributionManager = new DistributionManager();
                var flushManager = new FlushManager();
                var invoiceManager = new InvoiceManager();

                var turbines = turbineManager.RetrieveAllTurbines();
                var centralBanks = centralBankManager.RetrieveAllCentralBanks();
                var forecasts = forecastManager.RetrieveAllForecasts();
                var distributions = distributionManager.RetrieveAllDistributions();
                var flushes = flushManager.RetrieveAllFlushes();
                var invoices = invoiceManager.RetrieveAllInvoices();

                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var currentMonthForecasts = forecasts.Where(f => f.ForecastMonth == currentMonth && f.ForecastYear == currentYear).ToList();
                var currentMonthDistributions = distributions.Where(d => d.DistributionDate.Month == currentMonth && d.DistributionDate.Year == currentYear).ToList();
                var currentMonthInvoices = invoices.Where(i => i.IssueDate.Month == currentMonth && i.IssueDate.Year == currentYear).ToList();

                var result = new
                {
                    totalTurbines = turbines.Count,
                    activeTurbines = turbines.Count(t => (t.Status ?? string.Empty) == "Active"),
                    centralBankInventory = centralBanks.Sum(c => c.CurrentInventoryMWh),
                    effectiveCapacity = turbines.Where(t => (t.Status ?? string.Empty) == "Active").Sum(t => t.NominalWeeklyCapacityMWh),
                    monthForecasts = currentMonthForecasts.Count,
                    monthTotalDemand = currentMonthForecasts.Sum(f => f.RequestedEnergyMWh),
                    monthTotalBilled = currentMonthInvoices.Sum(i => i.TotalAmount),
                    lastFlush = flushes.OrderByDescending(f => f.ExecutedAt).FirstOrDefault()?.ExecutedAt,
                    monthTotalDistributed = currentMonthDistributions.Sum(d => d.AssignedEnergyMWh)
                };

                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("Engineer")]
        public ActionResult Engineer()
        {
            try
            {
                var turbineManager = new TurbineManager();
                var centralBankManager = new CentralBankManager();
                var maintenanceManager = new MaintenanceManager();
                var failureManager = new FailureManager();
                var flushManager = new FlushManager();

                var turbines = turbineManager.RetrieveAllTurbines();
                var centralBanks = centralBankManager.RetrieveAllCentralBanks();
                var maintenances = maintenanceManager.RetrieveAllMaintenances();
                var failures = failureManager.RetrieveAllFailures();
                var flushes = flushManager.RetrieveAllFlushes();

                var result = new
                {
                    totalTurbines = turbines.Count,
                    activeTurbines = turbines.Count(t => (t.Status ?? string.Empty) == "Active"),
                    turbinesUnderMaintenance = turbines.Count(t => (t.Status ?? string.Empty) == "Maintenance"),
                    damagedTurbines = turbines.Count(t => (t.Status ?? string.Empty) == "Damaged"),
                    suspendedTurbines = turbines.Count(t => (t.Status ?? string.Empty) == "Inactive"),
                    centralBankInventory = centralBanks.Sum(c => c.CurrentInventoryMWh),
                    overdueMaintenanceAlerts = maintenances.Count(m => (m.Status ?? string.Empty) == "Scheduled" && m.EstimatedEndDate < DateTime.Now),
                    lastFlushEnergy = flushes.OrderByDescending(f => f.ExecutedAt).FirstOrDefault()?.TransferredEnergyMWh ?? 0,
                    lastFlushDate = flushes.OrderByDescending(f => f.ExecutedAt).FirstOrDefault()?.ExecutedAt,
                    totalFailures = failures.Count
                };

                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("Buyer")]
        public ActionResult Buyer([FromQuery] int buyerUserId)
        {
            try
            {
                if (buyerUserId <= 0)
                {
                    return BadRequest(new { message = "El identificador del comprador es requerido." });
                }

                var forecastManager = new ForecastManager();
                var distributionManager = new DistributionManager();
                var invoiceManager = new InvoiceManager();

                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var forecasts = forecastManager.RetrieveByBuyerId(buyerUserId);
                var distributions = distributionManager.RetrieveByBuyerId(buyerUserId);
                var invoices = invoiceManager.RetrieveAllInvoices().Where(i => i.BuyerId == buyerUserId).ToList();

                var currentMonthForecasts = forecasts.Where(f => f.ForecastMonth == currentMonth && f.ForecastYear == currentYear).ToList();
                var currentMonthDistributions = distributions.Where(d => d.DistributionDate.Month == currentMonth && d.DistributionDate.Year == currentYear).ToList();
                var currentMonthInvoices = invoices.Where(i => i.IssueDate.Month == currentMonth && i.IssueDate.Year == currentYear).ToList();

                var result = new
                {
                    monthRequestedMWh = currentMonthForecasts.Sum(f => f.RequestedEnergyMWh),
                    lastAssignment = currentMonthDistributions.OrderByDescending(d => d.DistributionDate).FirstOrDefault()?.AssignedEnergyMWh ?? 0,
                    totalBilledAccumulated = invoices.Sum(i => i.TotalAmount),
                    activeForecasts = forecasts.Count(f => (f.Status ?? string.Empty) == "Pending"),
                    lastStatementDate = currentMonthInvoices.OrderByDescending(i => i.IssueDate).FirstOrDefault()?.IssueDate,
                    totalDistributions = distributions.Count,
                    currentMonthBilled = currentMonthInvoices.Sum(i => i.TotalAmount)
                };

                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}