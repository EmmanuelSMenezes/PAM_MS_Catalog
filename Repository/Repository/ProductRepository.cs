using Dapper;
using Domain.Model;
using Domain.Model.Product;
using Domain.Model.Product.Request;
using Domain.Model.Product.Response;
using Infrastructure.Repository.Interfaces;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Npgsql;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Xml.Linq;

namespace Infrastructure.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;
        public ProductRepository(string connectionString, ILogger logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public ProductResponse CreateProduct(CreateProductRequest createProductRequest)
        {

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {

                    connection.Open();
                    var transaction = connection.BeginTransaction();

                    var sqlinsertproduct = @$"INSERT INTO catalog.base_product
	                                      (name, description, note, minimum_price, type, created_by, admin_id) 
                                          VALUES ( '{createProductRequest.Name.Replace("'","''")}', '{createProductRequest.Description.Replace("'", "''")}', '{createProductRequest.Note}',
                                                   {createProductRequest.Minimum_price.ToString("", CultureInfo.InvariantCulture)},
                                          '{createProductRequest.Type}' , '{createProductRequest.Created_by}',
                                          '{createProductRequest.Admin_id}') RETURNING *;";

                    var insertedProduct = connection.Query<Product>(sqlinsertproduct).ToList();


                    if (insertedProduct.Any())
                    {
                        insertedProduct.FirstOrDefault().Categories = new List<Category>();

                        foreach (var item in createProductRequest.Category)
                        {
                            var category_parent_id = item.Category_parent_id != null ? $"'{item.Category_parent_id.ToString()}'" : "NULL";

                            var sqlinsertcategory = @$"INSERT INTO catalog.category_base_product
	                                           ( category_id, category_parent_id, product_id, created_by) 
                                               VALUES ( '{item.Category_id}', {category_parent_id}, '{insertedProduct.First().Product_id}', '{createProductRequest.Created_by}') RETURNING *;";


                            var category = new Category();
                            category = connection.Query<Category>(sqlinsertcategory).FirstOrDefault();


                            insertedProduct.FirstOrDefault().Categories.Add(category);
                        }
                    }

                    if (insertedProduct.Count == 0 || insertedProduct.FirstOrDefault().Categories.Count != createProductRequest.Category.Count)
                    {
                        transaction.Dispose();
                        connection.Close();
                        throw new Exception("errorWhileInsertProductOnDB");
                    }

                    transaction.Commit();
                    connection.Close();
                    var response = new ProductResponse()
                    {
                        Product = insertedProduct.FirstOrDefault()

                    };

                    return response;

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public ProductResponse CreateImageProduct(string url, Guid product_id)
        {

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {


                    var sql = @$"UPDATE catalog.base_product SET
	                                          url = '{url}'
                                              WHERE product_id = '{product_id}'
                                              RETURNING *;";

                    var insertedImageProduct = connection.Query<Product>(sql).ToList();

                    var response = new ProductResponse()
                    {
                        Product = insertedImageProduct.FirstOrDefault(),

                    };

                    return response;

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public List<Category> GetCategoriesOld(Guid product_id)
        {
            try
            {
                var sql = @$"select c.* from catalog.base_product bp 
join catalog.category_base_product cbp 
on cbp.product_id = bp.product_id
join catalog.category c 
on c.category_id = cbp.category_id
                             WHERE bp.product_id = '{product_id}'";

                using (var connection = new NpgsqlConnection(_connectionString))
                {

                    var listcategory = connection.Query<Category>(sql).ToList();

                    return listcategory;
                }
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
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                            var sql = @$"SELECT 
                              p.*,
                              ( 
                                SELECT json_agg(category)
                                FROM (
                                  SELECT c2.*
                                  FROM catalog.category_base_product c 
                                  JOIN catalog.category c2 ON c.category_id = c2.category_id
                                  WHERE c.product_id = p.product_id 
                                ) category
                              ) AS categories
                            FROM catalog.base_product p
                            WHERE p.active = true and (upper(p.name) like upper('%{filter.filter}%') or cast(p.identifier as TEXT) like '%{filter.filter}%')";

                     var getProducts = connection.Query(sql).Select(x => new Product()
                     {
                         Product_id = x.product_id,
                         Identifier = x.identifier,
                         Name = x.name,
                         Description = x.description,
                         Note = x.note,
                         Minimum_price = x.minimum_price,
                         Active = x.active,
                         Type = x.type,
                         Categories = !string.IsNullOrEmpty(x.categories) ? JsonConvert.DeserializeObject<List<Category>>(x.categories) : new List<Category>(),
                         Admin_id = x.admin_id,
                         Url = x.url,
                         Created_by = x.created_by,
                         Created_at = x.created_at,
                         Updated_by = x.updated_by,
                         Updated_at = x.updated_at
                     }).ToList();


                    int totalRows = getProducts.Count();
                    float totalPages = (float)totalRows / (float)filter.itensPerPage;

                    totalPages = (float)Math.Ceiling(totalPages);

                    getProducts = getProducts.Skip((int)((filter.page - 1) * filter.itensPerPage)).Take((int)filter.itensPerPage).ToList();


                    return new ListProductResponse() { Products = getProducts, Pagination = new Pagination() { totalPages = (int)totalPages, totalRows = totalRows } };

                }
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
                using (var connection = new NpgsqlConnection(_connectionString))
                {

                    var sql = @$"SELECT 
                              p.*,
                              ( 
                                SELECT json_agg(category)
                                FROM (
                                  SELECT c2.*
                                  FROM catalog.category_base_product c 
                                  JOIN catalog.category c2 ON c.category_id = c2.category_id
                                  WHERE c.product_id = p.product_id 
                                ) category
                              ) AS categories
                            FROM catalog.base_product p
                            WHERE p.active = true and product_id= '{product_id}';";

                    var getProducts = connection.Query(sql).Select(x => new Product()
                    {
                        Product_id = x.product_id,
                        Identifier = x.identifier,
                        Name = x.name,
                        Description = x.description,
                        Note = x.note,
                        Minimum_price = x.minimum_price,
                        Active = x.active,
                        Type = x.type,
                        Categories = !string.IsNullOrEmpty(x.categories) ? JsonConvert.DeserializeObject<List<Category>>(x.categories) : new List<Category>(),
                        Admin_id = x.admin_id,
                        Url = x.url,
                        Created_by = x.created_by,
                        Created_at = x.created_at,
                        Updated_by = x.updated_by,
                        Updated_at = x.updated_at
                    }).ToList();

                    return new ProductResponse()
                    {
                        Product = getProducts.FirstOrDefault()
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool InsertCategory(List<Category> productCategoryRequests, Guid product_id, Guid created_by)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var transaction = connection.BeginTransaction();

                    var productcategory = new List<ProductCategoryRequest>();

                    foreach (var item in productCategoryRequests)
                    {
                        var category_parent_id = item.Category_parent_id != null ? $"'{item.Category_parent_id.ToString()}'" : "NULL";

                        var sqlinsertcategory = @$"INSERT INTO catalog.category_base_product
	                                           ( category_id, category_parent_id, product_id, created_by) 
                                               VALUES ( '{item.Category_id}', {category_parent_id}, '{product_id}', '{created_by}') RETURNING *;";

                        var category = connection.Query<ProductCategoryRequest>(sqlinsertcategory).FirstOrDefault();
                        productcategory.Add(category);
                    }

                    if (productcategory.Count != productCategoryRequests.Count)
                    {
                        transaction.Dispose();
                        connection.Close();
                        throw new Exception("errorWhileInsertProducCategorytOnDB");
                    }

                    transaction.Commit();
                    connection.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public bool DeleteCategory(List<Category> productCategoryRequests, Guid product_id)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var transaction = connection.BeginTransaction();

                    var productcategory = new List<ProductCategoryRequest>();
                    var count = 0;

                    foreach (var item in productCategoryRequests)
                    {

                        var sqldeletecategory = @$"DELETE FROM catalog.category_base_product 
                                                    WHERE product_id = '{product_id}' AND category_id = '{item.Category_id}';";

                        var category = connection.Execute(sqldeletecategory);
                        count = count + category;
                    }

                    if (count != productCategoryRequests.Count)
                    {
                        transaction.Dispose();
                        connection.Close();
                        throw new Exception("errorWhileDeleteProducCategorytOnDB");
                    }

                    transaction.Commit();
                    connection.Close();
                    return true;
                }
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
                using (var connection = new NpgsqlConnection(_connectionString))
                {

                    connection.Open();
                    var transaction = connection.BeginTransaction();

                    var sqlupdateproduct = @$"UPDATE catalog.base_product SET
	                                      name = '{updateProductRequest.Name.Replace("'", "''")}',
                                          description = '{updateProductRequest.Description.Replace("'", "''")}',
                                          note = '{updateProductRequest.Note.Replace("'", "''")}',
                                          minimum_price = {updateProductRequest.Minimum_price.ToString("", CultureInfo.InvariantCulture)},
                                          active = {updateProductRequest.Active},
                                          updated_by = '{updateProductRequest.Updated_by}',
                                          updated_at = CURRENT_TIMESTAMP
                                          WHERE product_id = '{updateProductRequest.Product_id}'
                                          RETURNING *;";

                    var updatedProduct = connection.Query<Product>(sqlupdateproduct).ToList();

                    var updatedcategory = new List<Category>();

                    if (updatedProduct.Any())
                    {
                        foreach (var item in updateProductRequest.Category)
                        {
                            var category_parent_id = item.Category_parent_id != null ? $"'{item.Category_parent_id.ToString()}'" : "NULL";

                            var sqlinsertcategory = @$"UPDATE catalog.category_base_product SET
	                                           category_id = '{item.Category_id}',
                                               category_parent_id = {category_parent_id},
                                               updated_by = '{updateProductRequest.Updated_by}',
                                               updated_at = CURRENT_TIMESTAMP
                                               WHERE product_id = '{updateProductRequest.Product_id}' AND category_id = '{item.Category_id}'
                                               RETURNING *;";


                            var category = new Category();
                            category = connection.Query<Category>(sqlinsertcategory).FirstOrDefault();

                            updatedcategory.Add(category);
                            updatedProduct.First().Categories = updatedcategory;
                        }

                    }



                    if (updatedProduct.Count == 0 || updatedcategory.Count != updatedcategory.Count())
                    {
                        transaction.Dispose();
                        connection.Close();
                        throw new Exception("errorWhileUpdatedtProductOnDB");
                    }

                    transaction.Commit();
                    connection.Close();
                    var response = new ProductResponse()
                    {
                        Product = updatedProduct.FirstOrDefault(),
                    };
                    return response;

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
