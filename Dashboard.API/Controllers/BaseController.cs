using System;
using Base.DTOs;
using ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PagingExtensions;

namespace Dashboard.API.Controllers
{
    [ProducesResponseType(500, Type = typeof(ErrorResponse))]
    public class BaseController : ControllerBase
    {
        protected void AddPagingResponse(PageOutput output)
        {
            if (output != null)
            {
                Response.Headers.Add("Access-Control-Expose-Headers", "X-Paging-PageNo, X-Paging-PageSize, X-Paging-PageCount, X-Paging-TotalRecordCount");
                Response.Headers.Add("X-Paging-PageNo", output.Page.ToString());
                Response.Headers.Add("X-Paging-PageSize", output.PageSize.ToString());
                Response.Headers.Add("X-Paging-PageCount", output.PageCount.ToString());
                Response.Headers.Add("X-Paging-TotalRecordCount", output.RecordCount.ToString());
            }
        }
    }
}