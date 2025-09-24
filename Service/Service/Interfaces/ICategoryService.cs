using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Model;

namespace Application.Service.Interfaces
{
    public interface ICategoryService
    {
        CategoryResponse CreateCategory(CreateCategoryRequest createCategoryRequest);
        ListCategoryResponse GetCategory(Filter filter);
        ListCategoryResponse GetCategory(Guid category_id);
        CategoryResponse UpdateCategory(UpdateCategoryRequest updateCategoryRequest);
        ListCategoryResponse GetCategory(GetSubcategoryRequest getSubcategoryRequest);
    }
}
