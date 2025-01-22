using EfCoreDeleteExample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EfCoreDeleteExample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController2 : ControllerBase
{
    private readonly AppDbContext _context;
    public ProductController2(AppDbContext context)
    {
        _context = context;
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteById(int id)
    {
        var result = await _context.Products.DeleteByIdAsync(id);
        if (result)
        {
            await _context.SaveChangesAsync();
            return Ok($"Product with id {id} deleted successfully");
        }

        return NotFound($"Product with id {id} not found");
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteById2(int id)
    {
        var result = await _context.Products.DeleteByIdAsync(id);
        if (result)
        {
            await _context.SaveChangesAsync();
            return Ok($"Product with id {id} deleted successfully");
        }
    
        return NotFound($"Product with id {id} not found");
    }

    // public IActionResult Get()
    // {
    //     return Ok();
    //     return Ok();
    //     throw new Exception();
    // }
    // [HttpDelete]
    // public async Task<IActionResult> DeleteWhere(string name)
    // {
    //     var result = await _context.Products.Where(it => it.Name == name).ExecuteDeleteAsync();
    //     return Ok();
    // }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _context.Products.ToListAsync();
        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = product.Id }, product);
    }
}