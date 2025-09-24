using Domain.Model;
using Domain.Model.Product.Image.Request;
using Domain.Model.Product.Request;
using Domain.Model.Product.Response;
using System;
using System.Threading.Tasks;

namespace Application.Service.Interfaces
{
    public interface IProductService
    {
        ProductResponse CreateProduct(CreateProductRequest createProductRequest);
        Task<ProductResponse> CreateImageProduct(CreateImageProductRequest createImageProductRequest, string token);
        ListProductResponse GetProduct(Filter filter);
        ProductResponse GetProduct(Guid product_id);
        ProductResponse UpdateProduct(UpdateProductRequest updateProductRequest);
    }
}
