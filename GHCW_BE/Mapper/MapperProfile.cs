﻿using AutoMapper;
using GHCW_BE.DTOs;
using GHCW_BE.Models;

namespace GHCW_BE.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<News, NewsDTO>();
            CreateMap<Discount, DiscountDTO>();
            CreateMap<Service, ServiceDTO>();
            CreateMap<Service, ServiceDTO2>();
            CreateMap<Product, ProductDTO>();
            CreateMap<Product, ProductDTO2>();
            CreateMap<Category, CategoryDTO>();


            CreateMap<NewsDTO, News>();
            CreateMap<DiscountDTO, Discount>();
            CreateMap<ServiceDTO, Service>();
            CreateMap<ServiceDTO2, Service>();
            CreateMap<ProductDTO, Product>();
            CreateMap<ProductDTO2, Product>();
            CreateMap<CategoryDTO, Category>();


        }
    }
}