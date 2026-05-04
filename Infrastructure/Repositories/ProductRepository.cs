using APISales.Context;
using APISales.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace APISales.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;
        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Product> GetProducts()
        {
            return _context.Products.ToList();
        }
        public Product GetProductById(int id)
        {
            return _context.Products.Find(id);
        }
        public Product GetProductByName(string name)
        {
            var product = _context.Products.FirstOrDefault(x => x.Name == name);

            if (product == null)
                return null;
            
            return product;
        }

        public Product Create(Product product)
        {
            if(product == null)
                throw new ArgumentNullException(nameof(product));

            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            _context.Products.Add(product);
            _context.SaveChanges();

            return product;
        }

        public Product Update(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            product.UpdatedAt = DateTime.UtcNow;

            _context.Entry(product).State = EntityState.Modified;
            _context.Entry(product).Property(x => x.CreatedAt).IsModified = false;
            _context.SaveChanges();

            return product;
        }

        public Product Delete(int id)
        {
            var product = _context.Products.Find(id);

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            _context.Products.Remove(product);
            _context.SaveChanges();

            return product;
        }
    }
}
