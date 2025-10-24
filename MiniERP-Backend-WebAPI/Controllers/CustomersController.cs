using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniERP.Data;
using MiniERP.DTOs;
using MiniERP.Models;

namespace MiniERP.Controllers;

/// <summary>
/// Handles CRUD operations for customers.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize] // Requires authentication
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public CustomersController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all customers.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
    {
        var customers = await _context.Customers.ToListAsync();
        return Ok(_mapper.Map<List<CustomerDto>>(customers));
    }

    /// <summary>
    /// Get a single customer by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
            return NotFound(new { message = "Customer not found" });

        return Ok(_mapper.Map<CustomerDto>(customer));
    }

    /// <summary>
    /// Create a new customer.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,User")]
    public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CreateCustomerDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customer = _mapper.Map<Customer>(dto);
        customer.CreatedAt = DateTime.UtcNow;

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var customerDto = _mapper.Map<CustomerDto>(customer);
        return CreatedAtAction(nameof(GetCustomer), new { id = customer.CustomerId }, customerDto);
    }

    /// <summary>
    /// Update an existing customer.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return NotFound(new { message = "Customer not found" });

        _mapper.Map(dto, customer);
        customer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(_mapper.Map<CustomerDto>(customer));
    }

    /// <summary>
    /// Delete a customer (Admin only).
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return NotFound(new { message = "Customer not found" });

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Customer deleted successfully" });
    }
}
