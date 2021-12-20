using System;
using System.Collections.Generic;
using System.Linq;

using System.Web.Http.Filters;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;

namespace SA.Caching.Helpers
{
    public class CacheClientAttribute : ActionFilterAttribute
    {
        public int Duration { get; set; }
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            actionContext.Response.Headers.CacheControl = new CacheControlHeaderValue
            {
                MaxAge = TimeSpan.FromSeconds(Duration),
                MustRevalidate = true,
                Public = true
            };
        }
    }
}
