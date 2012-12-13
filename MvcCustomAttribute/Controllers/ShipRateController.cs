using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MvcCustomAttribute.DataAccess;
using MvcCustomAttribute.Models;

namespace MvcCustomAttribute.Controllers {

    public class ShipRateController : Controller {

        //
        // GET: /ShipRate/

        public ActionResult Index() {
            ShipRate rate = new ShipRate();

            // Add carriers
            rate.Carriers = new ShipRateLookup().GetCarrierList();

            return View(rate);
        }

        [HttpGet]
        public JsonResult GetShipMethodsByCarrier(string id) {
            var list = new ShipRateLookup().GetShipMethods(id);
            var data = list.Select(m => new SelectListItem() {
                Text = m.Text,
                Value = m.Value
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ShipRateLookup(ShipRate rate) {
            var detail = new ShipRateDetail();
            int totalWeight = rate.Weight.GetValueOrDefault();
            const int MAX_BOX_WEIGHT = 35;
            int totalBoxes = totalWeight % MAX_BOX_WEIGHT == 0 ? totalWeight / MAX_BOX_WEIGHT : (totalWeight / MAX_BOX_WEIGHT) + 1;
            switch (rate.Carrier) {
                case "POS":
                    if (!ModelState.IsValid) {
                        return PartialView("_Detail");
                    }

                    detail = new ShipRateLookup().GetPosRates(totalWeight);
                    break;

                default:
                    List<int> weights = new List<int>();
                    if (totalBoxes == 1) {
                        weights.Add(totalWeight);
                    } else {

                        // Add "full" boxes
                        for (int i = 1; i < totalBoxes; i++) {
                            weights.Add(MAX_BOX_WEIGHT);
                        }

                        // Add "remainder" box
                        weights.Add(totalWeight % MAX_BOX_WEIGHT);
                    }

                    // No matter how many boxes there are, there will only ever be 2 items returned: the price of the
                    // "full" box and the price of the "remainder" box.
                    var list = new ShipRateLookup().GetUpsRates(rate.ShipMethod, weights, rate.ZipCode);
                    if (list.Count > 1) {

                        // To get the total cost, we'll add "full" rate times the number of full boxes
                        // (total - 1) plus rate of the "remainder" box.
                        double totalCost = 0;

                        // Cost of "full" boxes
                        var full = (from l in list
                                    where l.Weight == MAX_BOX_WEIGHT
                                    select l).FirstOrDefault();
                        if (full != null) totalCost += full.TotalCost * (totalBoxes - 1);

                        // Cost of "remainder" box
                        var remainder = (from l in list
                                         where l.Weight < MAX_BOX_WEIGHT
                                         select l).FirstOrDefault();
                        if (remainder != null) totalCost += remainder.TotalCost;

                        detail.TotalCost = totalCost;
                    } else {
                        detail.TotalCost = list[0].TotalCost;
                    }

                    if (rate.Surcharge > 0) {
                        detail.TotalCost = Math.Round(detail.TotalCost + (detail.TotalCost * (double)(rate.Surcharge * .01M)), 2);
                    }

                    break;
            }

            // Set remaining detail values
            detail.Weight = totalWeight;
            detail.NumberOfBoxes = totalBoxes;
            detail.ZipCode = rate.ZipCode;

            return PartialView("_Detail", detail);
        }
    }
}