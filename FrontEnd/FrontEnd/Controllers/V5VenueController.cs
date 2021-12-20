using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using FrontEnd.Models;
using Newtonsoft.Json;
using SA.Caching.Helpers;

namespace FrontEnd.Controllers
{
    public class SelectedSeatsV5
    {
        public string SessionID { get; set; }
        public int NoOfSeats { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal ActualPrice { get; set; }
        public decimal DiscountedAmount { get; set; }
        public decimal Price { get; set; }
        public decimal EBPrice { get; set; }
    }

    public class SelectedSeatsV5Response
    {
        public string status { get; set; }
        public int NoOfSeats { get; set; }
        public string coupon_code { get; set; }
        public decimal discount_amount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal Percentage { get; set; }
        public decimal TotalPriceAfterDiscount { get; set; }
    }

    public class Tier
    {
        public int tierID { get; set; }
    }


    public class V5VenueController : Controller
    {
        private readonly int _cacheTime;
        private readonly int _cacheTimeInHours;
        private readonly string _constr;
        private bool IsCacheToReferesh;
        public V5VenueController()
        {
            IsCacheToReferesh = false;
            if (Session != null)
            {
                IsCacheToReferesh = (Convert.ToString(Session["IsCacheToReferesh"]) == null) ? false : (bool)Session["IsCacheToReferesh"];
            }
            _cacheTime = Convert.ToInt32(ConfigurationManager.AppSettings["cacheTimeInMinutes"]);
            _cacheTimeInHours = Convert.ToInt32(ConfigurationManager.AppSettings["cacheTimeInHours"]);
            _constr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
        }
        // GET: V5Venue
        JAVADBEntities db = new JAVADBEntities();
        public ActionResult Index()
        {
            if (Request.Cookies["IsRefreshed"] != null)
                Response.Cookies["IsRefreshed"].Expires = DateTime.Now.AddDays(-1);

            int eventID = Convert.ToInt32(Request["eventID"]);

            if (Request.Cookies["refresh-" + eventID] != null)
                DeleteSession(eventID, true);

            //ViewData["Tiers"] = db.tblTiers.ToList();
            ViewData["EventsData"] = getEventsFromCache(eventID, IsCacheToReferesh);
            ViewData["Tiers"] = db.tblTiers.ToList();
            ViewData["SeatDetails"] = getSeatDetailsFromCache(eventID, IsCacheToReferesh);
            ViewData["TiersData"] = getTiersFromCache(eventID, IsCacheToReferesh);
            return View();
        }
        
        public string SelectSeat()
        {
            int eventID = Convert.ToInt32(Request["eventID"]);
            int seatID = Convert.ToInt32(Request["seatID"]);
            string sessionID = Common.GetSessionID();

            string selectSeat = Common.SelectSeat(eventID, seatID);
            
            SelectedSeatsV5Response response = new SelectedSeatsV5Response();

            if (selectSeat == "OK")
            {
                var seats = db.Database.SqlQuery<SelectedSeatsV5>("select SessionID, count(*) as 'NoOfSeats', sum(Price) as 'TotalPrice' from tblSeatSelections where EventID = " + eventID + " and SessionID = '" + sessionID + "' group by SessionID").ToList();
               
                //store amount in session for later usage
                Session["GrandTotal"] = seats.Select(s=>s.TotalPrice);

                decimal amount = seats.Sum(i => i.TotalPrice);

                decimal priceAfterDiscount = amount;

                var couponsUsed = (db
                            .tblOrderCoupons
                            .Where(c => c.SessionID == sessionID && c.OrderID == null)
                            .SingleOrDefault());

                if(couponsUsed != null)
                {
                    decimal coupon_discount = Convert.ToDecimal(couponsUsed.tblCoupon.Discount);
                    
                    decimal discount_amount = ((amount * coupon_discount / 100));
                    
                    priceAfterDiscount = priceAfterDiscount - ((priceAfterDiscount * coupon_discount / 100));

                    response.coupon_code = couponsUsed.tblCoupon.CouponCode;
                    response.discount_amount = discount_amount;
                    response.Percentage = coupon_discount;

                }

                response.status = "OK";
                response.TotalPrice = amount;
                response.NoOfSeats = Convert.ToInt32(seats.Sum(s => s.NoOfSeats));
                
                response.TotalPrice = amount;
                response.TotalPriceAfterDiscount = priceAfterDiscount;
            }

            string json = JsonConvert.SerializeObject(response);
            return json;
        }
        public string RemoveSeat()
        {
            int eventID = Convert.ToInt32(Request["eventID"]);
            int seatID = Convert.ToInt32(Request["seatID"]);
            string sessionID = Common.GetSessionID();

            string removeSeat = Common.RemoveSeat(eventID, seatID);

            SelectedSeatsV5Response response = new SelectedSeatsV5Response();

            if (removeSeat == "OK")
            {
                var seats = db.Database.SqlQuery<SelectedSeatsV5>("select SessionID, count(*) as 'NoOfSeats', sum(Price) as 'TotalPrice' from tblSeatSelections where EventID = " + eventID + " and SessionID = '" + sessionID + "' group by SessionID").ToList();
             
                //store amount in session for later usage
                Session["GrandTotal"] = seats.Select(s => s.TotalPrice);

                response.status = "OK";
                response.NoOfSeats = Convert.ToInt32(seats.Sum(s => s.NoOfSeats));
                response.TotalPrice = Convert.ToDecimal(seats.Sum(s => s.TotalPrice));
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


            //remove all the orphan(unconfirmed) coupons associated with the current session
            var unconfirmed_coupons = db.tblOrderCoupons.Where(t => t.tblCoupon.EventID == eventID && t.SessionID == sessionID && t.OrderID == null).ToList();

            foreach (var coupon in unconfirmed_coupons)
                db.tblOrderCoupons.Remove(coupon);
            db.SaveChanges();

            ////destroy the current session id
            //HttpCookie cookie2 = new HttpCookie("ASP.NET_SessionId", "");
            //cookie2.Expires = DateTime.Now.AddDays(-1);
            //Response.Cookies.Add(cookie2);

            if (isRefresh == false)
                Response.Redirect("/Events?eventID=" + eventID);
        }

        //checks whether the coupon code exists and sets the values in session accdingly
        public string CheckCouponCode(string code)
        {           
            string sessionID = Common.GetSessionID();

            int eventID = Convert.ToInt32(Request["eventID"]);

            var coupon = db.tblCoupons.Where(c => c.CouponCode == code && c.EventID == eventID).SingleOrDefault();

            SelectedSeatsV5Response response = new SelectedSeatsV5Response();

            if (coupon != null)
            {
                var coupon_group = db.tblCouponGroups.Where(c => c.CouponGroupID == coupon.CouponGroupID).SingleOrDefault();

                string tiers = coupon_group.Tiers;

                var getTiers = db.Database.SqlQuery<Tier>("select distinct TierID from tblSeatSelections ts inner join tblSeat s on ts.SeatID = s.SeatID inner join tblSeatRows r on s.SeatRowID = r.SeatRowID   inner join tblEventLayoutBlocks eb on eb.BlockID = r.BlockID where ts.SessionID = '" + sessionID  + "' and ts.OrderID is null and ts.EventID  = " + eventID + "  and TierID is not null").ToList();

                bool isEligibleForCoupon = false;

                foreach (var tier in getTiers)
                {
                    int tierID = tier.tierID;

                    if (isEligibleForCoupon == false)
                    {
                        foreach (var t in tiers.Split(','))
                        {
                            if (Convert.ToInt32(t) == tierID)
                            {
                                isEligibleForCoupon = true;
                                break;
                            }
                        }
                    }
                }

                if (isEligibleForCoupon == true)
                {
                    if (coupon.IsUsed == null && coupon.UsedDate == null)
                    {
                        //check whether it is within the expiry date
                        var checkCoupon = db.tblOrderCoupons.Where(c => c.CouponID == coupon.CouponID && c.SessionID == sessionID && c.OrderID == null).SingleOrDefault();

                        if (checkCoupon == null)
                        {
                            tblOrderCoupon orderCoupon = new tblOrderCoupon();
                            orderCoupon.CouponID = coupon.CouponID;
                            orderCoupon.SessionID = sessionID;
                            db.tblOrderCoupons.Add(orderCoupon);
                            db.SaveChanges();

                            var seats = db.Database.SqlQuery<SelectedSeatsV5>("select SessionID, count(*) as 'NoOfSeats', sum(Price) as 'TotalPrice' from tblSeatSelections where EventID = " + eventID + " and SessionID = '" + sessionID + "' group by SessionID").ToList();

                            decimal amount = seats.Sum(i => i.TotalPrice);

                            decimal priceAfterDiscount = amount;
                            decimal coupon_discount = Convert.ToDecimal(coupon.Discount);
                            decimal discount_amount = ((amount * coupon_discount / 100));

                            priceAfterDiscount = priceAfterDiscount - ((priceAfterDiscount * coupon_discount / 100));

                            response.coupon_code = coupon.CouponCode;
                            response.discount_amount = discount_amount;
                            response.Percentage = coupon_discount;

                            response.status = "OK";
                            response.TotalPrice = amount;
                            response.NoOfSeats = Convert.ToInt32(seats.Sum(s => s.NoOfSeats));
                            response.TotalPriceAfterDiscount = priceAfterDiscount;

                            string json = JsonConvert.SerializeObject(response);

                            return json;
                        }
                        else
                        {
                            response.status = "You have already used this coupon in your shopping cart.";
                            string json = JsonConvert.SerializeObject(response);

                            return json;
                        }
                    }
                    else
                    {
                        response.status = "This coupon has been already used.";
                        string json = JsonConvert.SerializeObject(response);

                        return json;
                    }
                }
                else
                {
                    response.status = "Sorry, This coupon is not applicable on these seats.";
                    string json = JsonConvert.SerializeObject(response);

                    return json;
                }
            }
            else
            {
                response.status = "Sorry! This Coupon does not exist.";
                string json = JsonConvert.SerializeObject(response);

                return json;
            }
        }
        public string CheckBooking()
        {
            string sessionID = Common.GetSessionID();
            int eventID = Convert.ToInt32(Request["eventID"]);
            var tempBooked = db.tblSeatSelections.Where(ts => ts.SessionID == sessionID && ts.OrderID == null && ts.EventID == eventID).ToList();

            int occupied_seats = 0;

            foreach(var booked in tempBooked)
            {
                //selected & confirmed
                var isBooked = db.tblSeatSelections.Where(ts => ts.SessionID == "" && ts.SeatID == booked.SeatID && ts.OrderID == null && ts.EventID == eventID).ToList();

                //selected but not confirmed
                var isSelected = db.tblSeatSelections.Where(ts => (!string.IsNullOrEmpty(ts.SessionID) && ts.SessionID != sessionID) && ts.SeatID == booked.SeatID && ts.OrderID == null && ts.EventID == eventID).ToList();

                if (isBooked.Any() || isSelected.Any())
                    occupied_seats++;
            }

            if (occupied_seats > 0)
            {
                //remove current selection and prompt the user to reselect
                foreach (var booked in tempBooked)
                    db.tblSeatSelections.Remove(booked);
                db.SaveChanges();

                return "OCCUPIED";
            }
            else
                return "NOT OCCUPIED";
        }

        #region Private Functions
        private List<TiersViewModel> getTiers(int EventId)
        {
            List<TiersViewModel> tiersVM = null;
            //string constr = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            using (SqlConnection con = new SqlConnection(_constr))
            {
                tiersVM = new List<TiersViewModel>();
                var parameters = new DynamicParameters();
                parameters.Add("@EventId", EventId);

                using (SqlMapper.GridReader multi = con.QueryMultiple("GetTiers"
                    , parameters
                    , commandType: System.Data.CommandType.StoredProcedure))
                {
                    tiersVM = multi.Read<TiersViewModel>().ToList();
                }
            }
            return tiersVM;
        }
        private SeatListViewModel getSeatDetails(int EventId)
        {
            SeatListViewModel seatVM = null;
            List<SeatDetailViewModel> seatDetailsVM = null;
            List<TiersViewModel> tiersVM = null;
            //string constr = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            using (SqlConnection con = new SqlConnection(_constr))
            {
                seatVM = new SeatListViewModel();
                seatDetailsVM = new List<SeatDetailViewModel>();
                tiersVM = new List<TiersViewModel>();
                var parameters = new DynamicParameters();
                parameters.Add("@EventId", EventId);

                using (SqlMapper.GridReader multi = con.QueryMultiple("GetV5SeatDetails"
                    , parameters
                    , commandType: System.Data.CommandType.StoredProcedure))
                {
                    seatDetailsVM = multi.Read<SeatDetailViewModel>().ToList();
                    tiersVM = multi.Read<TiersViewModel>().ToList();
                }
                seatVM.Seats = seatDetailsVM;
                seatVM.Tiers = tiersVM;
            }
            return seatVM;
        }
        #endregion

        #region Caching Functions
        private object getEventsFromCache(int eventID, bool IsCacheToReferesh)
        {
            string ckKey = "cvEvent_" + eventID.ToString();
            object cvKey = MemoryCacher.GetValue(ckKey);
            object oData = null;
            if (MemoryCacher.GetValue(ckKey) == null || IsCacheToReferesh)
            {
                oData = db.tblEvents.Where(t => t.EventID == eventID).SingleOrDefault();
                if (IsCacheToReferesh)
                {
                    MemoryCacher.Delete(ckKey);
                }
                MemoryCacher.Add(ckKey, oData, DateTimeOffset.UtcNow.AddHours(_cacheTimeInHours));
            }
            else
            {
                oData = cvKey;
            }
            return oData;
        }
        private object getTiersFromCache(int eventID, bool IsCacheToReferesh)
        {
            string ckKey = "cvTiers_" + eventID.ToString();
            object cvKey = MemoryCacher.GetValue(ckKey);
            object tiersData = null;
            if (MemoryCacher.GetValue(ckKey) == null || IsCacheToReferesh)
            {
                tiersData = getTiers(eventID);
                if (IsCacheToReferesh)
                {
                    MemoryCacher.Delete(ckKey);
                }
                MemoryCacher.Add(ckKey, tiersData, DateTimeOffset.UtcNow.AddMinutes(_cacheTime));
            }
            else
            {
                tiersData = cvKey;
            }
            return tiersData;
        }
        private object getSeatDetailsFromCache(int eventID, bool IsCacheToReferesh)
        {
            string ckKey = "cvSeatDetails_" + eventID.ToString();
            object cvKey = MemoryCacher.GetValue(ckKey);
            object data = null;
            if (MemoryCacher.GetValue(ckKey) == null || IsCacheToReferesh)
            {
                data = getSeatDetails(eventID);
                if (IsCacheToReferesh)
                {
                    MemoryCacher.Delete(ckKey);
                }
                MemoryCacher.Add(ckKey, data, DateTimeOffset.UtcNow.AddMinutes(_cacheTime));
            }
            else
            {
                data = cvKey;
            }
            return data;
        }
        #endregion
    }
}