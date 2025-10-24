using AutoMapper;
using MiniERP.DTOs;
using MiniERP.Models;

namespace MiniERP;

/// <summary>
/// AutoMapper profile for mapping between entities and DTOs.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Product mappings
        CreateMap<Product, ProductDto>();
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Customer mappings
        CreateMap<Customer, CustomerDto>();
        CreateMap<CreateCustomerDto, Customer>();
        CreateMap<UpdateCustomerDto, Customer>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Order mappings
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.CustomerName, 
                opt => opt.MapFrom(src => $"{src.Customer.FirstName} {src.Customer.LastName}"));

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.ProductName, 
                opt => opt.MapFrom(src => src.Product.Name));
    }
}
