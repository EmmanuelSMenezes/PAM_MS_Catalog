using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Model;
using Domain.Model.Product.Response;

namespace Infrastructure.Repository
{
  public interface IProductPartnerRepository
    {
        ProductPartnerResponse CreateProductPartner(CreateProductPartnerRequest createProductPartnerRequest);
        ListProductPartnerResponse GetProductPartner(Guid branch_id, Filter filter);
        ListProductResponse GetBaseProductPartner(Guid branch_id, Filter filter);
        ProductPartnerResponse GetProductPartnerByProduct_id(Guid product_id, Guid partner_id);
        ProductPartnerResponse UpdateProductPartner(UpdateProductPartnerRequest updateProductPartnerRequest);
        ProductPartnerResponse GetProductPartner(Guid product_id);
        List<string> GetImagesProduct(Guid product_id);
        void CreateImagePartnerProduct(List<string> urls, Guid product_id, string imagedefaultname, Guid created_by);
        void UpdateImagePartnerProduct(List<string> urls, Guid product_id, string imagedefaultname, Guid created_by);
        void DeleteImagePartnerProduct(List<string> urls, Guid product_id);
        int DeleteProductPartner(Guid product_id);
        bool GetOrdersByProduct_id(Guid product_id);
    }
}
