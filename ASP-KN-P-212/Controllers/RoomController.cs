﻿using ASP_KN_P_212.Data.DAL;
using ASP_KN_P_212.Data.Entities;
using ASP_KN_P_212.Middleware;
using ASP_KN_P_212.Models.Content.Location;
using ASP_KN_P_212.Models.Content.Room;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP_KN_P_212.Controllers
{
    [Route("api/room")]
    [ApiController]
    public class RoomController(DataAccessor dataAccessor, ILogger<RoomController> logger) : ControllerBase

    {
        private readonly DataAccessor _dataAccessor = dataAccessor;
        private readonly ILogger<RoomController> _logger = logger;

        [HttpGet("all/{id}")]
        public List<Room> GetRooms(String id)
        {
            // var location = _dataAccessor.ContentDao.GetLocationBySlug(id);
            List<Room> rooms;
            {
                rooms = _dataAccessor.ContentDao.GetRooms(id);
            }
            return rooms;
        }

        [HttpGet("{id}")]
        public Room? GetRoom([FromRoute] String id/*, 
            [FromQuery] int? year, [FromQuery] int? month*/)
        {
            Room? room = _dataAccessor.ContentDao.GetRoomBySlug(id);
            if(room == null)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                return null;
            }
            room.Reservations = room.Reservations
                .Where(r => r.Date >= DateTime.Today).ToList();

            // room.Reservations.ForEach(r => { r.Room = null!; r.User = null!; });

            return room;
        }

        [HttpPost]
        public String DoPost(RoomFormModel model)
        {
            try
            {
                String? fileName = null;
                if (model.Photo != null)
                {
                    String ext = Path.GetExtension(model.Photo.FileName);
                    String path = Directory.GetCurrentDirectory() + "/wwwroot/images/content/";
                    String pathName;
                    do
                    {
                        fileName = Guid.NewGuid() + ext;
                        pathName = path + fileName;
                    }
                    while (System.IO.File.Exists(pathName));
                    using FileStream? stream = System.IO.File.OpenWrite(pathName);
                    model.Photo.CopyTo(stream);
                }
                if (fileName == null)
                {
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    return "File Image required";
                }
                _dataAccessor.ContentDao.AddRoom(
                    name: model.Name,
                    description: model.Description,
                    photoUrl: fileName,
                    slug: model.Slug,
                    locationId: model.LocationId,
                    stars: model.Stars,
                    dailyPrice: model.DailyPrice);
                Response.StatusCode = StatusCodes.Status201Created;
                return "Added";
            }
            catch (Exception ex)
            {
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return ex.Message;
            }            
        }

        [HttpGet("reserve/{id}")]
        public List<Reservation> GetReservations(String id)
        {
            Room? room;
            lock (this)
            {
                room = _dataAccessor.ContentDao.GetRoomBySlug(id);
            }
            return room?.Reservations;
        }

        [HttpPost("reserve")]
        public String ReserveRoom([FromBody] ReserveRoomFormModel model)
        {
            // TODO: перевірити, що кімната вільна на дату бронювання
            // а також дата бронювання не є у минулому
            // ? видаляти "м'яко видалені" замовлення на цю дату

            // Первинна автентифікація - за сесією.
            // якщо вона є, то іде робота з сайтом через Razor
            if (!(User.Identity?.IsAuthenticated ?? false))
            {
                // Якщо немає первинної авторизації - перевіряємо токен
                var identity = User.Identities
                    .FirstOrDefault(i => i.AuthenticationType == nameof(AuthTokenMiddleware));
                if (identity == null)
                {
                    // якщо авторизація не пройдена, то повідомлення в Items
                    Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return HttpContext.Items[nameof(AuthTokenMiddleware)]?.ToString() ?? "";
                }
            }
            try
            {
                _dataAccessor.ContentDao.ReserveRoom(model);
                Response.StatusCode = StatusCodes.Status201Created;
                return "Reserved";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return ex.Message;
            }
        }

        [HttpDelete("reserve")]
        public String DropReservation([FromQuery] Guid reserveId)
        {
            if (reserveId == default)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return "Guid parse error";
            }
            try
            {
                _dataAccessor.ContentDao.DeleteReservation(reserveId);
                Response.StatusCode = StatusCodes.Status202Accepted;
                return "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                return ex.Message;
            }
        }

        [HttpPatch]
        public Room? DoPatch(String slug)
        {
            return _dataAccessor.ContentDao.GetRoomBySlug(slug);
        }
    }
}
/*
 Д.З. Забезпечити валідацію даних форми, що надходять до 
    RoomController::DoPost
*/