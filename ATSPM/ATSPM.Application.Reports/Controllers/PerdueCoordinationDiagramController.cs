using ATSPM.IRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ATSPM.Application.Reports.Controllers
{
    public class PerdueCoordinationDiagramController : Controller
    {
        private readonly ILogger<LeftTurnReport> _logger;
        private readonly IPhasePedAggregationRepository _phasePedAggregationRepository;
        private readonly IApproachRepository _approachRepository;
        private readonly IApproachCycleAggregationRepository _approachCycleAggregationRepository;
        private readonly IPhaseTerminationAggregationRepository _phaseTerminationAggregationRepository;
        private readonly ISignalsRepository _signalRepository;
        private readonly IDetectorRepository _detectorRepository;
        private readonly IDetectorEventCountAggregationRepository _detectorEventCountAggregationRepository;
        private readonly IPhaseLeftTurnGapAggregationRepository _phaseLeftTurnGapAggregationRepository;
        private readonly IApproachSplitFailAggregationRepository _approachSplitFailAggregationRepository;


        // GET: PerdueCoordinationDiagramController
        public ActionResult Index()
        {
            return View();
        }

        // GET: PerdueCoordinationDiagramController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: PerdueCoordinationDiagramController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PerdueCoordinationDiagramController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: PerdueCoordinationDiagramController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: PerdueCoordinationDiagramController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: PerdueCoordinationDiagramController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PerdueCoordinationDiagramController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
