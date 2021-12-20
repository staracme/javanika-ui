using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FrontEnd.Models
{
    public class PaginationRequest
    {
        public int rowsPerPage { get; set; }
        public int pageNum { get; set; }
    }

    public class EventsViewModel
    {
        public int EventID { get; set; }
        public int VenueID { get; set; }
        public string VenueName { get; set; }
        public int LayoutID { get; set; }
        public string EventName { get; set; }
        public string EventType { get; set; }
        public string EventLanguage { get; set; }
        public string EventPlaytime { get; set; }
        public string EventDurationType { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ShowTime { get; set; }
        public string EventBanner { get; set; }
        public string EventMainBanner { get; set; }
        public string EventHomeBanner { get; set; }
        public string LocationTracker { get; set; }
        public string EventDescription { get; set; }
        public string EventTNC { get; set; }
        public string AgeGroup { get; set; }
        public DateTime CreatedDate { get; set; }
        public string BookingType { get; set; }
        public decimal ProcessingFee { get; set; }
        public int TicketStock { get; set; }
        public string Status { get; set; }
        public string EventStatus { get; set; }

    }

    public class EventsImageListViewModel
    {
        public int PastEventID { get; set; }
        public string EventName { get; set; }
        public string ImagePath { get; set; }
        public string Year { get; set; }
        public string Month { get; set; }
        public DateTime? CreatedDate { get; set; }
        public decimal? EventID { get; set; }
        public string ImageType { get; set; }

    }

    public class EventRequest : PaginationRequest
    {
        public int? EventId { get; set; }
    }

    public class EventsDTO
    {
        public int EventID { get; set; }
        public int VenueID { get; set; }
        public int LayoutID { get; set; }
        public string EventName { get; set; }
        public string EventType { get; set; }
        public string EventLanguage { get; set; }
        public string EventPlaytime { get; set; }
        public string EventDurationType { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ShowTime { get; set; }
        public string EventBanner { get; set; }
        public string EventMainBanner { get; set; }
        public string EventHomeBanner { get; set; }
        public string LocationTracker { get; set; }
        public string EventDescription { get; set; }
        public string EventTNC { get; set; }
        public string AgeGroup { get; set; }
        public DateTime CreatedDate { get; set; }
        public string BookingType { get; set; }
        public decimal ProcessingFee { get; set; }
        public int TicketStock { get; set; }
        public string Status { get; set; }

    }

    public class TiersViewModel
    {
        public int TierID { get; set; }
        public string TierName { get; set; }
        public int EventID { get; set; }
        public int BlockID { get; set; }
        public decimal Price { get; set; }
        public decimal EBPrice { get; set; }
    }

    public class SeatDetailViewModel
    {
        public decimal Price { get; set; }
        public decimal EBPrice { get; set; }
        public bool IsSeatBooked { get; set; }
        public string Status { get; set; }
        public int SeatID { get; set; }
        public string SeatNumber { get; set; }
        public string TierName { get; set; }
        public int EventId { get; set; }
        public int BlockID { get; set; }
        public string BlockNumber { get; set; }
        public int SeatRowID { get; set; }
    }

    public class SeatListViewModel
    {
        public List<SeatDetailViewModel> Seats { get; set; }
        public List<TiersViewModel> Tiers { get; set; }
    }
    
    public class CacheInput
    {
        public string Key { get; set; }
        public int cacheTimeInMinutes { get; set; }
    }
}