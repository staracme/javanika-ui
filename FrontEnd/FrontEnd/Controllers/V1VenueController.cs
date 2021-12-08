using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FrontEnd.Models;
using Newtonsoft.Json;
namespace FrontEnd.Controllers
{
    public class SelectedSeats
    {
        public string TierName { get; set; }
        public int NoOfSeats { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class SeatSelectionResponse
    {
        public string status { get; set; }
        public List<SelectedSeats> selected_seats {get; set;}
    }

    public class V1VenueController : Controller
    {
        // GET: V1Venue
        JAVADBEntities db = new JAVADBEntities();
        public ActionResult Index()
        {
            int eventID = Convert.ToInt32(Request["eventID"]);

            if (Request.Cookies["refresh-" + eventID] != null)
                DeleteSession(eventID, true);
            
            ViewData["Tiers"] = db.tblTiers.ToList();
            string sessionID = Common.GetSessionID();
            var seats = db.Database.SqlQuery<SelectedSeats>("select TierName, count(*) as 'NoOfSeats', Sum(Price) as 'TotalCost' from vw_seat_selection where EventID = " + eventID + " and sessionID = '" + sessionID + "' group by TierName").ToList();
            return View(seats);
        }

        //for adding seat
        public string SelectSeat()
        {
            int eventID = Convert.ToInt32(Request["eventID"]);
            int seatID = Convert.ToInt32(Request["seatID"]);
            string sessionID = Common.GetSessionID();

            string selectSeat = Common.SelectSeat(eventID, seatID);
            decimal totalCost = 0;

            SeatSelectionResponse response = new SeatSelectionResponse();
            List<SelectedSeats> lstSeats = new List<SelectedSeats>();

            if (selectSeat == "OK")
            {
                var seats = db.Database.SqlQuery<SelectedSeats>("select TierName, count(*) as 'NoOfSeats', Sum(Price) as 'TotalCost' from vw_seat_selection where EventID = " + eventID + " and sessionID = '" + sessionID + "' group by TierName").ToList();

                foreach(var seat in seats)
                {
                    lstSeats.Add(new SelectedSeats() {
                        TierName = seat.TierName,
                        NoOfSeats = seat.NoOfSeats,
                        TotalCost = seat.TotalCost
                    });

                    totalCost += seat.TotalCost;
                }

                //store amount in session for later usage
                Session["GrandTotal"] = totalCost;

                response.status = "OK";
                response.selected_seats = lstSeats;
            }
            string json = JsonConvert.SerializeObject(response);
            return json;
        }

        //for removing seat

        public string RemoveSeat()
        {
            int eventID = Convert.ToInt32(Request["eventID"]);
            int seatID = Convert.ToInt32(Request["seatID"]);
            string sessionID = Common.GetSessionID();

            string removeSeat = Common.RemoveSeat(eventID, seatID);
            decimal totalCost = 0;

            SeatSelectionResponse response = new SeatSelectionResponse();
            List<SelectedSeats> lstSeats = new List<SelectedSeats>();

            if (removeSeat == "OK")
            {
                var seats = db.Database.SqlQuery<SelectedSeats>("select TierName, count(*) as 'NoOfSeats', Sum(Price) as 'TotalCost' from vw_seat_selection where EventID = " + eventID + " and sessionID = '" + sessionID + "' group by TierName").ToList();

                foreach (var seat in seats)
                {
                    lstSeats.Add(new SelectedSeats()
                    {
                        TierName = seat.TierName,
                        NoOfSeats = seat.NoOfSeats,
                        TotalCost = seat.TotalCost
                    });

                    totalCost += seat.TotalCost;
                }

                //store amount in session for later usage
                Session["GrandTotal"] = totalCost;

                response.status = "OK";
                response.selected_seats = lstSeats;
            }
            string json = JsonConvert.SerializeObject(response);
            return json;
        }


        public void DeleteSession(int eventID, bool isRefresh)
        {
            string sessionID = Common.GetSessionID();

            //remove all the orphan(unconfirmed) seats associated with the current session
            var unconfirmed_seats = db.tblSeatSelections.Where(t => t.EventID == eventID && t.SessionID == sessionID && t.OrderID == null).ToList();

            foreach (var seat in unconfirmed_seats)
                db.tblSeatSelections.Remove(seat);
            db.SaveChanges();

            //destroy the current session id
            HttpCookie cookie2 = new HttpCookie("ASP.NET_SessionId", "");
            cookie2.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(cookie2);

            if (isRefresh == false)
                Response.Redirect("/Events?eventID=" + eventID);
        }
    }
}