using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.Service.Interfaces;
using Domain.Model;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Serilog;

namespace Application.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly ILogger _logger;
        private readonly string _privateSecretKey;
        private readonly string _tokenValidationMinutes;

        public CategoryService(
          ICategoryRepository repository,
          ILogger logger,
          string privateSecretKey,
          string tokenValidationMinutes
          )
        {
            _repository = repository;
            _logger = logger;
            _privateSecretKey = privateSecretKey;
            _tokenValidationMinutes = tokenValidationMinutes;
        }

        public CategoryResponse CreateCategory(CreateCategoryRequest createCategoryRequest)
        {
            try
            {
                var createCategory = _repository.CreateCategory(createCategoryRequest);

                if(createCategory == null ) throw new Exception("");
                return createCategory;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public CategoryResponse UpdateCategory(UpdateCategoryRequest updateCategoryRequest)
        {
            try
            {
                if (!updateCategoryRequest.Active)
                {
                    var verify = _repository.VerifyCategoryinProduct(updateCategoryRequest.Category_id);
                    if (verify) throw new Exception("categoryAssignedProduct");

                }

                var update = _repository.UpdateCategory(updateCategoryRequest);

                if (update == null) throw new Exception("");
                return update;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ListCategoryResponse GetCategory(Filter filter)
        {
            try
            {
                var getAllCategory = _repository.GetCategory(filter);

                switch (getAllCategory)
                {
                   
                    case var _ when getAllCategory == null:
                        throw new Exception("");
                    default: return getAllCategory;
                        
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ListCategoryResponse GetCategory(GetSubcategoryRequest getSubcategoryRequest)
        {

            try
            {
                var getAllCategory = _repository.GetCategory(getSubcategoryRequest);

                switch (getAllCategory)
                {

                    case var _ when getAllCategory == null:
                        throw new Exception("");
                    default: return getAllCategory;

                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ListCategoryResponse GetCategory(Guid category_id)
        {
            try
            {
                var getCategory = _repository.GetCategory(category_id);

                switch (getCategory)
                {

                    case var _ when getCategory == null:
                        throw new Exception("");
                    default: return getCategory;

                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
