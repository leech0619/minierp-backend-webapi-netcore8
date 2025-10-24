using System.ComponentModel.DataAnnotations;

namespace MiniERP.DTOs;

/// <summary>
/// DTO for creating a new order.
/// </summary>
public class CreateOrderDto
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    public List<CreateOrderItemDto> OrderItems { get; set; } = new();
}

/// <summary>
/// DTO for creating an order item within an order.
/// </summary>
public class CreateOrderItemDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
}

/// <summary>
/// DTO for order responses.
/// </summary>
public class OrderDto
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<OrderItemDto> OrderItems { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for order item responses.
/// </summary>
public class OrderItemDto
{
    public int OrderItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}

/// <summary>
/// DTO for updating order status.
/// </summary>
public class UpdateOrderStatusDto
{
    [Required]
    [RegularExpression("^(Pending|Completed|Cancelled)$", ErrorMessage = "Status must be Pending, Completed, or Cancelled")]
    public string Status { get; set; } = string.Empty;
}
