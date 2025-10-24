using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniERP.Data;
using MiniERP.DTOs;
using MiniERP.Models;

namespace MiniERP.Controllers;

/// <summary>
/// Handles CRUD operations for orders.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize] // Requires authentication
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public OrdersController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all orders with customer and product details.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
    {
        var orders = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .ToListAsync();

        return Ok(_mapper.Map<List<OrderDto>>(orders));
    }

    /// <summary>
    /// Get a single order by ID with details.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order == null)
            return NotFound(new { message = "Order not found" });

        return Ok(_mapper.Map<OrderDto>(order));
    }

    /// <summary>
    /// Create a new order with order items.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,User")]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Verify customer exists
        var customer = await _context.Customers.FindAsync(dto.CustomerId);
        if (customer == null)
            return BadRequest(new { message = "Customer not found" });

        // Verify all products exist and calculate totals
        var order = new Order
        {
            CustomerId = dto.CustomerId,
            OrderNumber = GenerateOrderNumber(),
            OrderDate = DateTime.UtcNow,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        decimal totalAmount = 0;

        foreach (var itemDto in dto.OrderItems)
        {
            var product = await _context.Products.FindAsync(itemDto.ProductId);
            if (product == null)
                return BadRequest(new { message = $"Product with ID {itemDto.ProductId} not found" });

            if (product.StockQuantity < itemDto.Quantity)
                return BadRequest(new { message = $"Insufficient stock for product {product.Name}" });

            var orderItem = new OrderItem
            {
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                UnitPrice = product.Price,
                Subtotal = product.Price * itemDto.Quantity
            };

            order.OrderItems.Add(orderItem);
            totalAmount += orderItem.Subtotal;

            // Update stock
            product.StockQuantity -= itemDto.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
        }

        order.TotalAmount = totalAmount;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Reload order with related data
        var createdOrder = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstAsync(o => o.OrderId == order.OrderId);

        var orderDto = _mapper.Map<OrderDto>(createdOrder);
        return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, orderDto);
    }

    /// <summary>
    /// Update order status (e.g., Pending to Completed).
    /// </summary>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return NotFound(new { message = "Order not found" });

        order.Status = dto.Status;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = $"Order status updated to {dto.Status}" });
    }

    /// <summary>
    /// Delete an order (Admin only).
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order == null)
            return NotFound(new { message = "Order not found" });

        // Restore stock for cancelled orders
        foreach (var item in order.OrderItems)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product != null)
            {
                product.StockQuantity += item.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
            }
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Order deleted successfully" });
    }

    /// <summary>
    /// Generates a unique order number.
    /// </summary>
    private string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}";
    }
}
