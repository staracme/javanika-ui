using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FrontEnd.Models;

namespace FrontEnd.Infra
{
    public class AutoMapperWebProfile : AutoMapper.Profile
    {

        public AutoMapperWebProfile()
        {
            CreateMap<EventsDTO, EventsViewModel>();
            CreateMap<EventsViewModel, EventsDTO>();

            //Transfer Entity Model to View Model / DTOs
            CreateMap<FrontEnd.Models.tblEvent, EventsViewModel>();
            CreateMap<EventsViewModel, FrontEnd.Models.tblEvent>();
            CreateMap<EventsImageListViewModel, FrontEnd.Models.tblPastEvent>();
            CreateMap<FrontEnd.Models.tblPastEvent, EventsImageListViewModel>();

        }

        public static void Run()
        {
            AutoMapper.Mapper.Initialize(a => { a.AddProfile<AutoMapperWebProfile>(); });
        }
    }
}