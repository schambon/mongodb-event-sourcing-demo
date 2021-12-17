using Microsoft.AspNetCore.Mvc;
using EventApi.Services;
using EventApi.Models;

namespace EventApi.Controllers;

[ApiController]
[Route("api/query/widget")]
public class WidgetQueryController : ControllerBase
{

    private readonly WidgetService _widgetService;


    public WidgetQueryController(WidgetService widgetService)
    {
        _widgetService = widgetService;
    }

    [HttpGet("{uuid}")]
    public Widget FindById(string uuid)
    {
        return _widgetService.FindById(uuid);
    }

    [HttpGet("all")]
    public IEnumerable<Widget> All()
    {
        return _widgetService.All();
    }
}