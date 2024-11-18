﻿using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace GHCW_BE.Services
{
    public class CustomerService
    {
        private readonly GHCWContext _context;
        private readonly IMapper _mapper;

        public CustomerService(GHCWContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<CustomerDTO>> GetCustomerList()
        {
            var customers = await _context.Customers.Include(c => c.Account).ToListAsync();
            var customerDTOs = _mapper.Map<List<Customer>, List<CustomerDTO>>(customers);
            return customerDTOs;
        }

        public async Task<CustomerDTO?> GetCustomerProfileById(int uID)
        {
            var customer = await _context.Customers.Include(c => c.Account).FirstOrDefaultAsync(u => u.Id == uID);
            if (customer != null)
            {
                var userDTO = _mapper.Map<Customer, CustomerDTO>(customer);
                return userDTO;
            }
            return null;
        }

        public async Task<CustomerDTO?> CheckCustomerExsit(string email)
        {
            var checkCustomer = await _context.Customers.FirstOrDefaultAsync(x => x.Email.Equals(email));
            if (checkCustomer == null)
            {
                return null;
            }
            var customerDTO = _mapper.Map<Customer, CustomerDTO>(checkCustomer);
            return customerDTO;
        }

        public async Task<(bool isSuccess, string message)> AddNewCustomer(AddCustomerRequest a)
        {
            try
            {
                var customer = _mapper.Map<AddCustomerRequest, Customer>(a);
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                return (true, "Thêm khách hàng mới thành công.");
            }
            catch (Exception)
            {
                return (false, "Có lỗi trong quá trình thêm khách hàng mới, vui lòng thử lại.");
            }
        }

        public async Task<(bool isSuccess, string message)> EditCustomer(CustomerDTO c)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(u => u.Id == c.Id);
            if (customer == null)
            {
                return (false, "Không tìm thấy khách hàng.");
            }
            try
            {
                _mapper.Map(c, customer);
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
                return (true, "Cập nhật thông tin thành công.");
            }
            catch (Exception)
            {
                return (false, "Cập nhật thông tin thất bại, vui lòng kiểm tra lại.");
            }
        }
    }
}
