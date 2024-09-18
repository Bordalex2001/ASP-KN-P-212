using ASP_KN_P_212.Data.Entities;
using ASP_KN_P_212.Models.Content.Room;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ASP_KN_P_212.Data.DAL
{
    public class ContentDao
    {
        private readonly DataContext _context;
        private readonly Object _dbLocker;

        public ContentDao(DataContext context, object dbLocker)
        {
            _context = context;
            _dbLocker = dbLocker;
        }
        
        public void AddRoom(String name, String description, 
            String photoUrl, String slug, Guid locationId, int stars,
            Double dailyPrice)
        {
            lock (_dbLocker)
            {
                _context.Rooms.Add(new()
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = description,
                    DeletedDt = null,
                    PhotoUrl = photoUrl,
                    Slug = slug,
                    LocationId = locationId,
                    Stars = stars,
                    DailyPrice = dailyPrice
                });
                _context.SaveChanges();
            }
        }
        
        public void AddCategory(String name, String description, String? photoUrl, String? slug = null)
        {
            lock (_dbLocker)
            {
                _context.Categories.Add(new()
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = description,
                    DeletedDt = null,
                    PhotoUrl = photoUrl,
                    Slug = slug ?? name
                });
                _context.SaveChanges();
            }
        }
        
        public List<Category> GetCategories(bool includeDeleted = false)
        {
            List<Category> list;
            lock (_dbLocker) {
                list = _context
                    .Categories
                    .Where(c => includeDeleted || c.DeletedDt == null)
                    .ToList();
            }
            return list;
        }
        
        public Category? GetCategoryBySlug(String slug)
        {
            Category? ctg;
            lock (_dbLocker)
            {
                ctg = _context.Categories.FirstOrDefault(c => c.Slug == slug);
            }
            return ctg;
        }
        
        public Category? GetCategoryById(Guid id)
        {
            Category? ctg;
            lock (_dbLocker)
            {
                ctg = _context.Categories.Find(id);
            }
            return ctg;
        }

        public void UpdateCategory(Category category)
        {
            Category? ctg;
            lock (_dbLocker)
            {
                ctg = _context.Categories.Find(category.Id);
            }
            if (ctg != null && ctg != category)
            {
                ctg.Name = category.Name;
                ctg.Description = category.Description;
                ctg.DeletedDt = category.DeletedDt;
                ctg.PhotoUrl = category.PhotoUrl;
            }
            lock (_dbLocker)
            {
                _context.SaveChanges();
            }
        }
        
        public void DeleteCategory(Guid id)
        {
            Category? ctg = _context
                .Categories
                .Find(id);
            if (ctg != null && ctg.DeletedDt == null)
            {
                ctg.DeletedDt = DateTime.Now;
                lock (_dbLocker)
                {
                    _context.SaveChanges();
                }
            }
        }

        public void RestoreCategory(Guid id)
        {
            Category? ctg = _context
                .Categories
                .Find(id);

            if (ctg != null && ctg.DeletedDt != null)
            {
                ctg.DeletedDt = null;
                lock (_dbLocker)
                {
                    _context.SaveChanges();
                }
            }
        }

        public void DeleteCategory(Category category)
        {
            DeleteCategory(category.Id);
        }

        public void AddLocation(String name, String description, Guid CategoryId,
            int? Stars = null, Guid? CountryId = null, 
            Guid? CityId = null, string? Address = null, String? PhotoUrl = null, String? slug = null)
        {
            lock (_dbLocker)
            {
                _context.Locations.Add(new()
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = description,
                    CategoryId = CategoryId,
                    Stars = Stars,
                    CountryId = CountryId,
                    CityId = CityId,
                    Address = Address,
                    DeletedDt = null,
                    PhotoUrl = PhotoUrl,
                    Slug = slug ?? name
                });
                _context.SaveChanges();
            }
        }
        
        public List<Location> GetLocations(String categorySlug)
        {
            Category? ctg = GetCategoryBySlug(categorySlug);
            if (ctg == null)
            {
                return new List<Location>();
            }
            IQueryable<Location>? query = _context
                .Locations
                .Where(loc => 
                    loc.DeletedDt == null && 
                    loc.CategoryId == ctg.Id);  
            
            return query.ToList();
        }
        
        public Location? GetLocationBySlug(String slug)
        {
            Location? ctg;
            lock (_dbLocker)
            {
                ctg = _context.Locations.FirstOrDefault(c => c.Slug == slug);
            }
            return ctg;
        }

        public List<Room> GetRooms(String locationSlug)
        {
            Location? location;
            lock (_dbLocker)
            {
                location = _context.Locations
                    .FirstOrDefault(loc => loc.Slug == locationSlug);
            }
            if(location == null)
            {
                throw new Exception("Slug belongs to NO location");
            }
            return GetRooms(location.Id);
        }
        
        public List<Room> GetRooms(Guid locationId)
        {
            List<Room> res;
            lock (_dbLocker)
            {
                /* spread operator
                x = [1,2]   
                y = [3,4]
                [x, y] -> [ [1,2], [3,4] ]
                [..x, ..y] -> [1,2,3,4]
                */
                res = [.._context.Rooms.Where(r => r.LocationId == locationId)];
            }
            return res;
        }

        public Room? GetRoomBySlug(String slug)
        {
            Guid? id;
            try { id = Guid.Parse(slug); }
            catch { id = null; }
            Func<Room, bool>? slugSelector = (Room c) => c.Slug == slug;
            var idSelector = (Room c) => c.Id == id || c.Slug == slug;

            Room? room;
            lock (_dbLocker)
            {
                room = _context.Rooms
                    .Include(r => r.Reservations)
                    .FirstOrDefault(id == null ? slugSelector : idSelector);
            }
            if (room != null)
            {
                room.Reservations =
                    room.Reservations.Where(r => r.DeletedDt == null).ToList();
            }
            return room;
        }

        public void ReserveRoom(ReserveRoomFormModel model)
        {
            ArgumentNullException.ThrowIfNull(model, nameof(model));
            if (model.Date < DateTime.Today)
            {
                throw new ArgumentException("Date must not be in past");
            }
            Room? room;
            lock (_dbLocker)
            {
                room = _context.Rooms.Find(model.RoomId);
            }
            if (room == null)
            {
                throw new ArgumentException("Room not found for id = " + model.RoomId);
            }
            _context.Reservations.Add(new()
            {
                Id = Guid.NewGuid(),
                Date = model.Date,
                RoomId = model.RoomId,
                UserId = model.UserId,
                Price = room.DailyPrice,
                OrderDt = DateTime.Now
            });
            lock (_dbLocker)
            {
                _context.SaveChanges();
            }
        }

        public void DeleteReservation(Guid id)
        {
            Reservation? reservation;
            lock (_dbLocker)
            {
                reservation = _context.Reservations.Find(id);
            }
            if (reservation == null)
            {
                throw new ArgumentException("Passed id not found");
            }
            if (reservation.DeletedDt != null)
            {
                throw new ArgumentException("Passed id already deleted");
            }
            reservation.DeletedDt = DateTime.Now;
            lock (_dbLocker)
            {
                _context.SaveChanges();
            }
        }
    }
}
/* Завдання:
 * Перед бронюванням номера видати повідомлення-підтвердження
 * "Ви дійсно бажаєте забронювати кімнату 201 на 23.04.2024?"
 */