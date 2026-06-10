using APISales.Domain.Products;

namespace APISales.Repositories
{
    public interface ICategoryRepository
    {
        IEnumerable<Category> GetCategories();
        Category GetCategory(int id);
        Category Create(Category category);
        Category Update(Category category);
        Category Delete(int id);

    }
}
