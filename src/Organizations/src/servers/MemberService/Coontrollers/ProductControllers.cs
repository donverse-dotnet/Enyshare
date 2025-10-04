using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MemberService.Services;
using MemberService.Repositories;

namespace MemberService.Controllers 
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IMemberRepository _repo;

        public ProductController(IMemberRepository repo)
        {
            _repo = repo;
        }

        // GET : api/product
        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetAll()
        {
            var products = await _repo.GetAllAsync();
            return Ok(products);
        }

        // GET : api/product/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<List<Product>>> GetById(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return BadRequest("Invalid ObjectId format");

            var product = await _repo.GetByIdAsync(objectId);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        // POST : api/product
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] Product product)
        {
            await _repo.InsertAsync(product);
            return CreatedAtAction(nameof(GetById), new { id = product.Id.ToString() }, product);
        }

        // PUT : api/product/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(string id, [FromBody] Product updatedProduct)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return BadRequest("Invalid ObjectId format");

            updatedProduct.Id = objectId;
            await _repo.UpdateAsync(updatedProduct);
            return NotContent();
        }

        // DELETE: api/product/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return BadRequest("Invalid ObjectId fprmaat");

            await _repo.DeleteAsync(objectId);
            return NoContent();
        }
    }
}