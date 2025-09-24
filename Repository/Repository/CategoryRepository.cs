using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Domain.Model;
using Microsoft.VisualBasic;
using Npgsql;
using Serilog;

namespace Infrastructure.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;
        public CategoryRepository(string connectionString, ILogger logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public CategoryResponse CreateCategory(CreateCategoryRequest createCategoryRequest)
        {
            try
            {
                var category_parent_id = createCategoryRequest.Category_parent_id != null ? $"'{createCategoryRequest.Category_parent_id.ToString()}'" : "NULL";

                var sql = @$"INSERT INTO catalog.category(description, category_parent_id, created_by, active)
                          VALUES('{createCategoryRequest.Description}', {category_parent_id}, '{createCategoryRequest.Created_by}', true) RETURNING *";

                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var response = connection.Query<Category>(sql).FirstOrDefault();

                    if (response == null) throw new Exception("");

                    return new CategoryResponse() { category = response };
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public CategoryResponse UpdateCategory(UpdateCategoryRequest updateCategoryRequest)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                var transaction = connection.BeginTransaction();

                try
                {
                    var category_parent_id = updateCategoryRequest.Category_parent_id != null ? $"'{updateCategoryRequest.Category_parent_id.ToString()}'" : "NULL";

                    var sql = @$"UPDATE catalog.category
                          SET description = '{updateCategoryRequest.Description}', category_parent_id = {category_parent_id}, 
                              updated_by = '{updateCategoryRequest.Updated_by}', updated_at = CURRENT_TIMESTAMP, active= {updateCategoryRequest.Active} 
                          WHERE category_id = '{updateCategoryRequest.Category_id}'
                          RETURNING *";


                    var response = connection.Query<Category>(sql).FirstOrDefault();

                    if (response == null) {

                        transaction.Dispose();
                        connection.Close();
                        throw new Exception("");
                    } 

                    

                    if (updateCategoryRequest.Category_parent_id == null && !updateCategoryRequest.Active)
                    {
                        var sqlparent = @$"UPDATE catalog.category
                          SET active= {updateCategoryRequest.Active},
                              updated_by = '{updateCategoryRequest.Updated_by}', 
                              updated_at = now()
                          WHERE category_parent_id = '{updateCategoryRequest.Category_id}'
                          RETURNING *";

                        var responseparent = connection.Query<Category>(sqlparent).ToList();
                        if (responseparent.Count < 0) {

                            transaction.Dispose();
                            connection.Close();
                            throw new Exception("");
                        }
                    }

                    transaction.Commit();
                    connection.Close();
                    return new CategoryResponse() { category = response };

                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
        }

        public ListCategoryResponse GetCategory(Filter filter)
        {
            try
            {
                var sql = @$"SELECT c2.description AS category_parent_name, c.* FROM catalog.category c 
                            LEFT JOIN catalog.category c2
                            ON c2.category_id = c.category_parent_id
                            WHERE (upper(c.description) like upper('%{filter.filter}%') or cast(c.identifier as text) like '%{filter.filter}%')";



                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var response = connection.Query<Category>(sql).ToList();

                    int totalRows = response.Count();
                    float totalPages = (float)totalRows / (float)filter.itensPerPage;

                    totalPages = (float)Math.Ceiling(totalPages);

                    response = response.Skip((int)((filter.page - 1) * filter.itensPerPage)).Take((int)filter.itensPerPage).ToList();


                    if (response == null) throw new Exception("");

                    return new ListCategoryResponse() { categories = response, Paginations = new Pagination() { totalPages = (int)totalPages, totalRows = totalRows } };
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
                var response = new List<Category>();
                foreach (var item in getSubcategoryRequest.Category_ids)
                {
                    var sql = @$"SELECT c2.description AS category_parent_name, c.* FROM catalog.category c 
                            LEFT JOIN catalog.category c2
                            ON c2.category_id = c.category_parent_id WHERE c.category_parent_id = '{item}'";



                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        response.AddRange(connection.Query<Category>(sql).ToList());

                    }

                }

                if (response == null) throw new Exception("");

                return new ListCategoryResponse() { categories = response, Paginations = new Pagination() { totalPages = response.Count, totalRows = response.Count } };

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
                var sql = @$"SELECT c2.description AS category_parent_name, c.* FROM catalog.category c 
                            LEFT JOIN catalog.category c2
                            ON c2.category_id = c.category_parent_id WHERE c.category_id = '{category_id}'";

                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var response = connection.Query<Category>(sql).ToList();
                    if (response == null) throw new Exception("");
                    return new ListCategoryResponse() { categories = response, Paginations = new Pagination() { totalPages = response.Count, totalRows = response.Count } };
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public bool VerifyCategoryinProduct(Guid category_id)
        {
            try
            {
                var sql = @$"SELECT * FROM catalog.category_base_product WHERE category_id = '{category_id}'";

                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var response = connection.Query<Category>(sql).ToList();
                    if (response.Any()) return true;
                    return false;
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
