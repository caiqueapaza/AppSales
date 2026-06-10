using APISales.Application.DTOs.Products;
using APISales.Context;
using APISales.Domain.Products;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace APISales.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        public ProductController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> Get()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<ProductResponseDto>>(products));
        }

        [HttpGet("{id:int}", Name ="GetProduct")]
        public async Task<ActionResult<ProductResponseDto>> Get(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if(product is null)
                return NotFound("Produto não encontrado!");
            
            return Ok(_mapper.Map<ProductResponseDto>(product));
        }

        [HttpPost]
        [Authorize(Roles = "ADM")]
        public async Task<ActionResult> Post(CreateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = _mapper.Map<Product>(dto);
            
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var productDto = _mapper.Map<ProductResponseDto>(product);

            return CreatedAtRoute("GetProduct", new { id = product.Id }, productDto);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "ADM")]
        public async Task<ActionResult> Put(int id, UpdateProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);

            if (product is null)
                return NotFound("Produto não encontrado!");

            _mapper.Map(dto, product);

            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<ProductResponseDto>(product));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "ADM")]
        public async Task<ActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is null)
                return NotFound("Produto não encontrado!");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<ProductResponseDto>(product));
        }
    }
}
