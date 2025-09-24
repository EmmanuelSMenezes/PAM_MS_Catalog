using System;
using System.Collections.Generic;
using Domain.Model;

namespace Infrastructure.Repository
{
  public interface ICategoryRepository
  {
        CategoryResponse CreateCategory(CreateCategoryRequest createCategoryRequest);
        ListCategoryResponse GetCategory(Filter filter);
        ListCategoryResponse GetCategory(Guid category_id);
        CategoryResponse UpdateCategory(UpdateCategoryRequest updateCategoryRequest);
        ListCategoryResponse GetCategory(GetSubcategoryRequest getSubcategoryRequest);
        bool VerifyCategoryinProduct(Guid category_id);
    }
}
