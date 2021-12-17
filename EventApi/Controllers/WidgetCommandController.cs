using Microsoft.AspNetCore.Mvc;
using EventApi.Services;
using EventApi.Models;

namespace EventApi.Controllers;

[ApiController]
[Route("api/cmd/widget")]
public class WidgetCommandController : ControllerBase
{

    private readonly EventService _eventService;


    public WidgetCommandController(EventService eventService)
    {
        _eventService = eventService;
    }

 
    [HttpPost("add")]
    public NewWidgetResult NewWidget(NewWidgetInput widget)
    {

        var uuid = Guid.NewGuid().ToString();
        Event evt = new Event {
            Timestamp = DateTime.Now,
            EventType = "Widget.New",
            EntityUuid = uuid,
            EventDetails = new Dictionary<string, object> {
                { "shelf", widget.Shelf },
                { "description", widget.Description }
            }
        };

        var evtResult = _eventService.CreateEvent(evt);

        return new NewWidgetResult
        {
            Ok = evtResult != null,
            Uuid = uuid
        };
    }

    public class NewWidgetInput
    {

        public int Shelf { get; set; } = -1;

        public string Description { get; set; } = "";
    }
    public class NewWidgetResult
    {
        public bool Ok { get; set; } = false;
        public string Uuid { get; set; } = "";
    }

    [HttpPost("move")]
    public MoveWidgetResult MoveWidget(MoveWidgetInput input)
    {

        Event evt = new Event {
            Timestamp = DateTime.Now,
            EventType = "Widget.Move",
            EntityUuid = input.Uuid,
            EventDetails = new Dictionary<string, object> {
                { "shelf", input.Shelf }
            }
        };
        var eventId = _eventService.CreateEvent(evt);
        return new MoveWidgetResult {
            Ok = eventId != null
        };
    }

    public class MoveWidgetInput
    {
        public string? Uuid { get; set; } = null;

        public int? Shelf { get; set; } = null;
    }

    public class MoveWidgetResult
    {
        public bool Ok { get; set; } = false;

    }

    [HttpPost("remove")]
    public RemoveWidgetResult RemoveWidget(RemoveWidgetInput input)
    {

        Event evt = new Event {
            Timestamp = DateTime.Now,
            EventType = "Widget.Remove",
            EntityUuid = input.Uuid,
            EventDetails = new Dictionary<string, object> {
                { "destination", input.Destination }
            }
        };

        var eventId = _eventService.CreateEvent(evt);

        return new RemoveWidgetResult {
            Ok = eventId != null
        };
    }

    public class RemoveWidgetInput
    {
        public string? Uuid { get; set;}

        public string Destination { get; set; } = "";
    }

    public class RemoveWidgetResult
    {
        public bool Ok { get; set; } = false;
    }
}