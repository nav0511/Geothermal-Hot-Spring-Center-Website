using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Models;

namespace GHCW_BE.Utils.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<News, NewsDTO>();
            CreateMap<News, NewsDTO2>();
            CreateMap<News, NewsDTOForAdd>();
            CreateMap<News, NewsDTOForUpdate>();
            CreateMap<Discount, DiscountDTO>();
            CreateMap<Service, ServiceDTO>();
            CreateMap<Service, ServiceDTO2>();
            CreateMap<Service, ServiceDTOForUpdate>();
            CreateMap<Product, ProductDTO>();
            CreateMap<Product, ProductDTO2>();
            CreateMap<Product, ProductDTOImg>();
            CreateMap<Product, ProductDTOForUpdate>();
            CreateMap<Category, CategoryDTO>();

            CreateMap<Schedule, ScheduleDTO>()
                .ForMember(dest => dest.ReceptionistId, opt => opt.MapFrom(src => src.Receptionist.Id))
                .ForMember(dest => dest.ReceptionistName, opt => opt.MapFrom(src => src.Receptionist.Name));

            CreateMap<Ticket, TicketDTO>();
            CreateMap<Ticket, TicketDTOForPayment>();
            CreateMap<Ticket, TicketDTOForStaff>();
            CreateMap<TicketDetail, TicketDetailDTO>();
            CreateMap<TicketDetail, TicketDetailDTOForPayment>();
            CreateMap<Bill, BillDTO>();
            CreateMap<Bill, BillDTOForBuyProducts>();
            CreateMap<BillDetail, BillDetailDTO>();
            CreateMap<BillDetail, BillDetailDTOForBuyProducts>();


            CreateMap<NewsDTO, News>();
            CreateMap<NewsDTO2, News>();
            CreateMap<NewsDTOForAdd, News>();
            CreateMap<NewsDTOForUpdate, News>();
            CreateMap<DiscountDTO, Discount>();
            CreateMap<ServiceDTO, Service>();
            CreateMap<ServiceDTO2, Service>();
            CreateMap<ServiceDTOForUpdate, Service>();
            CreateMap<ProductDTO, Product>();
            CreateMap<ProductDTO2, Product>();
            CreateMap<ProductDTOImg, Product>();
            CreateMap<ProductDTOForUpdate, Product>();
            CreateMap<CategoryDTO, Category>();
            CreateMap<ScheduleDTO, Schedule>();
            CreateMap<TicketDTO, Ticket>();
            CreateMap<TicketDTOForPayment, Ticket>();
            CreateMap<TicketDTOForStaff, Ticket>();
            CreateMap<TicketDetailDTO, TicketDetail>();
            CreateMap<TicketDetailDTOForPayment, TicketDetail>();
            CreateMap<BillDTO, Bill>();
            CreateMap<BillDTOForBuyProducts, Bill>();
            CreateMap<BillDetailDTO, BillDetail>();
            CreateMap<BillDetailDTOForBuyProducts, BillDetail>();

            CreateMap<Customer, CustomerDTO>()
                .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.AccountId))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.DoB, opt => opt.MapFrom(src => src.Account.DoB))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Account.Gender))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Account.Address))
                .ForMember(dest => dest.IsEmailNotify, opt => opt.MapFrom(src => src.IsEmailNotify));

            CreateMap<CustomerDTO, Customer>()
                .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.AccountId))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.IsEmailNotify, opt => opt.MapFrom(src => src.IsEmailNotify));

            CreateMap<AddCustomerRequest, Customer>()
                .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.AccountId))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.IsEmailNotify, opt => opt.MapFrom(src => src.IsEmailNotify));

            CreateMap<Account, AccountDTO>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.DoB, opt => opt.MapFrom(src => src.DoB))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.RefreshToken, opt => opt.MapFrom(src => src.RefreshToken));

            CreateMap<AccountDTO, Account>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.DoB, opt => opt.MapFrom(src => src.DoB))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.RefreshToken, opt => opt.MapFrom(src => src.RefreshToken));

            CreateMap<AddRequest, Account>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.DoB, opt => opt.MapFrom(src => src.DoB))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            CreateMap<UpdateRequest, Account>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.DoB, opt => opt.MapFrom(src => src.DoB))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender));

            CreateMap<EditRequest, Account>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.DoB, opt => opt.MapFrom(src => src.DoB))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            CreateMap<EditRequest, CustomerDTO>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));

            CreateMap<AddScheduleRequest, Schedule>()
                .ForMember(dest => dest.Receptionist, opt => opt.MapFrom(src => src.ReceptionistId))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
                .ForMember(dest => dest.Shift, opt => opt.MapFrom(src => src.Shift))
                .ForMember(dest => dest.Receptionist, opt => opt.Ignore());

            CreateMap<EditScheduleRequest, Schedule>();
        }
    }
}
