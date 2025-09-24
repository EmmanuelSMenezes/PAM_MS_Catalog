using Domain.Model;
using Domain.Model.Product.Request;
using Domain.Model.Product.Response;
using System;
using System.Collections.Generic;

namespace Infrastructure.Repository.Interfaces
{
  public interface IProductRepository
    {
        ProductResponse CreateProduct(CreateProductRequest createProductRequest);
        ProductResponse CreateImageProduct(string url, Guid product_id);
        ListProductResponse GetProduct(Filter filter);
        ProductResponse GetProduct(Guid product_id);
        ProductResponse UpdateProduct(UpdateProductRequest updateProductRequest);
        List<Category> GetCategoriesOld(Guid product_id);
        bool InsertCategory(List<Category> productCategoryRequests, Guid product_id, Guid created_by);
        bool DeleteCategory(List<Category> productCategoryRequests, Guid product_id);
    }
}
