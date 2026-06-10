using APISales.Context;
using APISales.Domain.ServiceItens;
using Microsoft.EntityFrameworkCore;

namespace APISales.Infrastructure.Repositories
{
    public class ServiceItemRepository : IServiceItemRepository
    {
        private readonly AppDbContext _context;
        public ServiceItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<ServiceItem> GetServices()
        {
            return _context.ServiceItens.ToList();
        }
        public ServiceItem GetServiceById(int id)
        {
            return _context.ServiceItens.Find(id);
        }
        public ServiceItem GetServeceByName(string name)
        {
            var service = _context.ServiceItens.FirstOrDefault(x => x.Name == name);

            if (service == null)
                return null;

            return service;
        }

        public ServiceItem Create(ServiceItem service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            service.CreatedAt = DateTime.UtcNow;
            service.UpdatedAt = DateTime.UtcNow;

            _context.ServiceItens.Add(service);
            _context.SaveChanges();

            return service;
        }

        public ServiceItem Update(ServiceItem service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            service.UpdatedAt = DateTime.UtcNow;

            _context.Entry(service).State = EntityState.Modified;
            _context.Entry(service).Property(x => x.CreatedAt).IsModified = false;
            _context.SaveChanges();

            return service;
        }

        public ServiceItem Delete(int id)
        {
            var service = _context.ServiceItens.Find(id);

            if (service == null)
                throw new ArgumentNullException(nameof(service));

            _context.ServiceItens.Remove(service);
            _context.SaveChanges();

            return service;
        }
    }
}
