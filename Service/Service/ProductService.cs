using Application.Service.Interfaces;
using Domain.Model;
using Domain.Model.Product.Request;
using Domain.Model.Product.Response;
using Infrastructure.Repository.Interfaces;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using Domain.Model.Product.Image.Request;
using Domain.Model.Product.Image.Response;

namespace Application.Service
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly ILogger _logger;
        private readonly string _privateSecretKey;
        private readonly string _tokenValidationMinutes;
        private readonly HttpEndPoints _httpEndPoints;

        public ProductService(
          IProductRepository repository,
          ILogger logger,
          string privateSecretKey,
          HttpEndPoints httpEndPoints,
          string tokenValidationMinutes
          )
        {
            _repository = repository;
            _logger = logger;
            _privateSecretKey = privateSecretKey;
            _httpEndPoints = httpEndPoints;
            _tokenValidationMinutes = tokenValidationMinutes;
        }

        public ProductResponse CreateProduct(CreateProductRequest createProductRequest)
        {
            try
            {

                var createProduct = _repository.CreateProduct(createProductRequest);


                return createProduct;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<ProductResponse> CreateImageProduct(CreateImageProductRequest createImageProduct, string token)
        {
            try
            {
                var url = string.Empty;
                if (createImageProduct.Image != null)
                {
                    var requestImage = new UploadImageRequest()
                    {
                        File = createImageProduct.Image
                    };
                    HttpContent intContent = new StringContent(requestImage.bucketId.ToString());
                    var fileStream = requestImage.File.OpenReadStream();


                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Split(' ')[1]);
                        using (var form = new MultipartFormDataContent())
                        {
                            form.Add(intContent, "bucketId");
                            form.Add(new StreamContent(fileStream), "File", requestImage.File.FileName);

                            var req = await httpClient.PostAsync($"{_httpEndPoints.MSStorageBaseUrl}storage/upload", form);
                            var responseString = await req.Content.ReadAsStringAsync();
                            var response = JsonConvert.DeserializeObject<HttpResponse<UploadImageResponse>>(responseString);

                            if (!response.data.isUploaded)
                            {
                                throw new Exception("failedWhileUpdateImage");
                            }
                            url = response.data.Url;
                        }
                    }
                }
                var createProduct = _repository.CreateImageProduct(url, createImageProduct.product_id);


                return createProduct;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public ListProductResponse GetProduct(Filter filter)
        {
            try
            {
                var getProduct = _repository.GetProduct(filter);
                if (getProduct == null) throw new Exception("");
                return getProduct;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public ProductResponse GetProduct(Guid product_id)
        {
            try
            {
                var getProduct = _repository.GetProduct(product_id);
                if (getProduct == null) throw new Exception("");
                return getProduct;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public ProductResponse UpdateProduct(UpdateProductRequest updateProductRequest)
        {
            try
            {


                if (updateProductRequest.Category.Any())
                {
                    var getCategoriesold = _repository.GetCategoriesOld(updateProductRequest.Product_id);

                    var update = new List<ProductCategoryRequest>();
                    var delete = new List<Category>();
                    var insert = new List<Category>();

                    foreach (var item in updateProductRequest.Category)
                    {
                        var compare = getCategoriesold.Where(x => x.Category_id.Equals(item.Category_id)).ToList();

                        if (compare.Any())
                        {
                            update.Add(new ProductCategoryRequest()
                            {
                                Category_id = item.Category_id,
                                Category_parent_id = item.Category_parent_id
                            });
                            getCategoriesold.RemoveAll(x => x.Category_id.Equals(item.Category_id));
                        }
                        else
                        {
                            insert.Add(new Category()
                            {
                                Category_id = item.Category_id,
                                Category_parent_id = item.Category_parent_id
                            });
                        }

                    }
                    if (insert.Any()) _repository.InsertCategory(insert.ToList(), updateProductRequest.Product_id, updateProductRequest.Updated_by);

                    if (getCategoriesold.Any()) _repository.DeleteCategory(getCategoriesold.ToList(), updateProductRequest.Product_id);

                updateProductRequest.Category = update;
                }

                var updateProduct = _repository.UpdateProduct(updateProductRequest);

                var categories = _repository.GetCategoriesOld(updateProductRequest.Product_id);
                updateProduct.Product.Categories = categories;
                return updateProduct;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
