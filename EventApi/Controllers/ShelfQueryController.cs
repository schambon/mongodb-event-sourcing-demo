using Microsoft.AspNetCore.Mvc;
using EventApi.Services;
using EventApi.Models;

namespace EventApi.Controllers;

[ApiController]
[Route("api/query/shelf")]
public class ShelfQueryController : ControllerBase
{

    private readonly ShelfService _service;


    public ShelfQueryController(ShelfService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public Shelf FindById(int id)
    {
        return _service.FindById(id);
    }


    [HttpGet("all")]
    public IEnumerable<Shelf> All()
    {
        return _service.All();
    }
    
    [HttpGet("total")]
    public int GetTotalWidgetsInStore()
    {
        return _service.TotalWidgets();
    }

}