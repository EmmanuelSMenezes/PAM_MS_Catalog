using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Model;
using Domain.Model.Product.Response;

namespace Application.Service.Interfaces
{
    public interface IProductPartnerService
    {
        ProductPartnerResponse CreateProductPartner(CreateProductPartnerRequest createProductPartnerRequest);
        Task<ProductPartnerResponse> CreateImagePartnerProduct(CreateImageProductPartnerRequest createImageProductPartnerRequest, string token);
        ListProductPartnerResponse GetProductPartner(Guid branch_id, Filter filter);
        ProductPartnerResponse GetProductPartner(Guid product_id);
        ListProductResponse GetBaseProductPartner(Guid branch_id, Filter filter);
        ProductPartnerResponse GetProductPartnerByProduct_id(Guid Product_id, Guid Partner);
        ProductPartnerResponse UpdateProductPartner(UpdateProductPartnerRequest updateProductPartnerRequest, string token);
        Guid DeleteProductPartner(Guid product_id);
    }
}
