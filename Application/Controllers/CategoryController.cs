using APISales.Context;
using APISales.Domain.Products;
using APISales.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APISales.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _repository;
        private readonly ILogger<CategoryController> _logger;
        public CategoryController(ICategoryRepository repository, ILogger<CategoryController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Category>> Get()
        {
            var categories = _repository.GetCategories();
            return Ok(categories);
        }

        [HttpGet("{id:int}", Name = "GetCategory")]
        public ActionResult<Category> Get(int id)
        {
            var category = _repository.GetCategory(id);
            if (category == null)
            {
                _logger.LogWarning($"Categoria com id= {id} não encontrada!");
                return NotFound("Categoria com id= {id} não encontrada!");
            }
            
            return Ok(category);
        }

        [HttpPost]
        [Authorize(Roles = "ADM")]
        public ActionResult Post(Category category)
        {
            if(category == null)
            {
                _logger.LogWarning($"Dados invalidos!");
                return BadRequest($"Dados invalidos!");
            }
            var categoryCreated = _repository.Create(category);

            return new CreatedAtRouteResult("GetCategory", new { id = categoryCreated.Id }, categoryCreated);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "ADM")]
        public ActionResult Put(int id, Category category)
        {
            if (id != category.Id)
            {
                _logger.LogWarning($"Dados invalidos!");
                return BadRequest($"Dados invalidos!");
            }
            _repository.Update(category);

            return Ok(category);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "ADM")]
        public ActionResult Delete(int id)
        {
            var category = _repository.GetCategory(id);
            if (category == null)
            {
                _logger.LogWarning($"Categoria com id= {id} não encontrada!");
                return NotFound("Categoria com id= {id} não encontrada!");
            }

            var categoryDeleted = _repository.Delete(id);
            return Ok(categoryDeleted);
        }

    }
}
