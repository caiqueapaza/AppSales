using APISales.Application.DTOs.Customers;
using APISales.Application.DTOs.Employees;
using APISales.Application.DTOs.Products;
using APISales.Application.DTOs.Sales;
using APISales.Application.DTOs.ServiceItens;
using APISales.Domain.Customers;
using APISales.Domain.Employees;
using APISales.Domain.Products;
using APISales.Domain.Sales;
using APISales.Domain.ServiceItens;
using AutoMapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductResponseDto>();
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();
        CreateMap<ServiceItem, ServiceItemResponseDto>();
        CreateMap<CreateServiceItemDto, ServiceItem>();
        CreateMap<UpdateServiceItemDto, ServiceItem>();
        CreateMap<Employee, EmployeeResponseDto>();
        CreateMap<CreateEmployeeDto, Employee>();
        CreateMap<UpdateEmployeeDto, Employee>();
        CreateMap<Customer, CustomerResponseDto>();
        CreateMap<CreateCustomerDto, Customer>();
        CreateMap<UpdateCustomerDto, Customer>();
        CreateMap<CreateSaleItemDto, SaleItem>();
        CreateMap<CreateSaleEntryItemDto, SaleEntryItem>();
        CreateMap<CreateSaleEntryItemServiceDto, SaleEntryItemService>();
        CreateMap<SaleItem, SaleItemResponseDto>()
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Quantity * src.UnitPrice));
        CreateMap<SaleEntryItemService, SaleEntryItemServiceResponseDto>()
            .ForMember(dest => dest.ServiceItemName, opt => opt.MapFrom(src => src.ServiceItem != null ? src.ServiceItem.Name : null))
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Quantity * src.UnitPrice));
        CreateMap<SaleEntryItem, SaleEntryItemResponseDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));
        CreateMap<SalePayment, SalePaymentResponseDto>();
        CreateMap<Sale, SaleResponseDto>()
            .ForMember(dest => dest.SubTotal, opt => opt.MapFrom(src => src.SubTotalAmount))
            .ForMember(dest => dest.BalanceAmount, opt => opt.MapFrom(src => src.TotalAmount - src.AmountPaid))
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalAmount));
    }
}
