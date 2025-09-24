using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Application.Service.Interfaces;
using Domain.Model;
using Domain.Model.Product;
using Domain.Model.Product.Image.Request;
using Domain.Model.Product.Image.Response;
using Domain.Model.Product.Response;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Serilog;

namespace Application.Service
{
    public class ProductPartnerService : IProductPartnerService
    {
        private readonly IProductPartnerRepository _repository;
        private readonly ILogger _logger;
        private readonly string _privateSecretKey;
        private readonly string _tokenValidationMinutes;
        private readonly HttpEndPoints _httpEndPoints;

        public ProductPartnerService(
          IProductPartnerRepository repository,
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

        public ProductPartnerResponse CreateProductPartner(CreateProductPartnerRequest createProductPartnerRequest)
        {
            try
            {
                var create = _repository.CreateProductPartner(createProductPartnerRequest);

                if (create == null) throw new Exception("");
                return create;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ProductPartnerResponse> CreateImagePartnerProduct(CreateImageProductPartnerRequest createImageProductPartnerRequest, string token)
        {
            try
            {
                List<string> urls = new List<string>();
                List<string> updateimage = new List<string>();
                var getimages = _repository.GetImagesProduct(createImageProductPartnerRequest.Product_id);

                foreach (var item in createImageProductPartnerRequest.Images)
                {
                    if (item != null)
                    {
                        var requestImage = new UploadImageRequest()
                        {
                            File = item
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

                                var compare = getimages.Where(x => x.Contains(item.FileName)).ToList();

                                if (!compare.Any())
                                {
                                    var req = await httpClient.PostAsync($"{_httpEndPoints.MSStorageBaseUrl}storage/upload", form);
                                    var responseString = await req.Content.ReadAsStringAsync();
                                    var response = JsonConvert.DeserializeObject<HttpResponse<UploadImageResponse>>(responseString);

                                    if (!response.data.isUploaded)
                                    {
                                        throw new Exception("failedWhileUploadedImage");
                                    }

                                    createImageProductPartnerRequest.Imagedefaultname = createImageProductPartnerRequest.Imagedefaultname.Equals(requestImage.File.FileName) ? response.data.Url : createImageProductPartnerRequest.Imagedefaultname;
                                    urls.Add(response.data.Url);
                                }
                                else
                                {

                                getimages.RemoveAll(x => x.Contains(item.FileName));
                                updateimage.Add(compare.First());
                                }

                            }
                        }
                    }
                }

                var decodedToken = GetDecodeToken(token.Split(' ')[1], _privateSecretKey);

                if (urls.Any())
                    _repository.CreateImagePartnerProduct(urls, createImageProductPartnerRequest.Product_id, createImageProductPartnerRequest.Imagedefaultname, 
                                                          decodedToken.UserId);

                if (updateimage.Any())
                    _repository.UpdateImagePartnerProduct(updateimage, createImageProductPartnerRequest.Product_id, createImageProductPartnerRequest.Imagedefaultname, 
                                                          decodedToken.UserId);

                if (getimages.Any())
                    _repository.DeleteImagePartnerProduct(getimages, createImageProductPartnerRequest.Product_id);

                var getproductpartner = _repository.GetProductPartnerByProduct_id(createImageProductPartnerRequest.Product_id, createImageProductPartnerRequest.Partner_id);

                return getproductpartner;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public DecodedToken GetDecodeToken(string token, string secret)
        {
            DecodedToken decodedToken = new DecodedToken();
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtSecurityToken = jwtSecurityTokenHandler.ReadToken(token) as JwtSecurityToken;
            if (IsValidToken(token, secret))
            {
                foreach (Claim claim in jwtSecurityToken.Claims)
                {
                    if (claim.Type == "email")
                    {
                        decodedToken.email = claim.Value;
                    }
                    else if (claim.Type == "name")
                    {
                        decodedToken.name = claim.Value;
                    }
                    else if (claim.Type == "userId")
                    {
                        decodedToken.UserId = new Guid(claim.Value);
                    }
                    else if (claim.Type == "roleId")
                    {
                        decodedToken.RoleId = new Guid(claim.Value);
                    }
                }

                return decodedToken;
            }

            throw new Exception("invalidToken");
        }
        public bool IsValidToken(string token, string secret)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("emptyToken");
            }
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters();
            tokenValidationParameters.ValidateIssuer = false;
            tokenValidationParameters.ValidateAudience = false;
            tokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(Base64UrlEncoder.Encode(secret)));

            try
            {
                SecurityToken validatedToken;
                ClaimsPrincipal claimsPrincipal = jwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out validatedToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public ListProductPartnerResponse GetProductPartner(Guid branch_id, Filter filter)
        {
            try
            {
                var getbypartner = _repository.GetProductPartner(branch_id, filter);

                switch (getbypartner)
                {

                    case var _ when getbypartner == null:
                        throw new Exception("");
                    default: return getbypartner;

                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ListProductResponse GetBaseProductPartner(Guid branch_id, Filter filter)
        {
            try
            {
                var getbypartner = _repository.GetBaseProductPartner(branch_id, filter);

                switch (getbypartner)
                {

                    case var _ when getbypartner == null:
                        throw new Exception("");
                    default: return getbypartner;

                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ProductPartnerResponse GetProductPartnerByProduct_id(Guid product_id, Guid partner_id)
        {
            try
            {
                var getbyproduct = _repository.GetProductPartnerByProduct_id(product_id, partner_id);

                switch (getbyproduct)
                {

                    case var _ when getbyproduct == null:
                        throw new Exception("");
                    default: return getbyproduct;

                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ProductPartnerResponse UpdateProductPartner(UpdateProductPartnerRequest updateProductPartnerRequest, string token)
        {
            try
            {
                updateProductPartnerRequest.Updated_by = GetDecodeToken(token.Split(' ')[1], _privateSecretKey).UserId;
                var update = _repository.UpdateProductPartner(updateProductPartnerRequest);

                if (update == null) throw new Exception("");
                return update;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public Guid DeleteProductPartner(Guid product_id)
        {
            try
            {
                var order = _repository.GetOrdersByProduct_id(product_id);

                if (order) throw new Exception("ProductLinkedOrder");
            
                var product = _repository.DeleteProductPartner(product_id);

                if(product == 1) return product_id;
                 throw new Exception("");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ProductPartnerResponse GetProductPartner(Guid product_id)
        {
            try
            {
               
                var product = _repository.GetProductPartner(product_id);

                return product;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
