using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Domain.Model;
using Domain.Model.Product.Request;
using Domain.Model.Product.Response;
using Domain.Model.Product;
using Microsoft.VisualBasic;
using Npgsql;
using Serilog;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.CodeAnalysis.Operations;

namespace Infrastructure.Repository
{
    public class ProductPartnerRepository : IProductPartnerRepository
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;
        public ProductPartnerRepository(string connectionString, ILogger logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public ProductPartnerResponse CreateProductPartner(CreateProductPartnerRequest createProductPartnerRequest)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {

                    connection.Open();
                    var transaction = connection.BeginTransaction();

                    var sqlinsertproduct = @$"INSERT INTO catalog.product(
                                                base_product_id, 
                                                name, 
                                                description, 
                                                price, 
                                                created_by) 
                                            VALUES(
                                               '{createProductPartnerRequest.Base_product_id}', 
                                               '{createProductPartnerRequest.Name.Replace("'","''")}',
                                               '{createProductPartnerRequest.Description.Replace("'", "''")}', 
                                               '{createProductPartnerRequest.Price}',
                                               '{createProductPartnerRequest.Created_by}') RETURNING *;";

                    var insertedProduct = connection.Query<ProductPartner>(sqlinsertproduct).ToList();

                    if (insertedProduct.Count == 0)
                    {
                        transaction.Dispose();
                        connection.Close();
                        throw new Exception("errorWhileInsertProductOnDB");
                    }


                    sqlinsertproduct = @$"INSERT INTO catalog.product_branch(
                                            product_id,
                                            branch_id,
                                            created_by)
                                          VALUES(
                                            '{insertedProduct.First().Product_id}',
                                            '{createProductPartnerRequest.Branch_id}', 
                                            '{createProductPartnerRequest.Created_by}') RETURNING *;";

                    var insertproductbranch = connection.Query<dynamic>(sqlinsertproduct).ToList();

                    if (insertproductbranch.Count == 0)
                    {
                        transaction.Dispose();
                        connection.Close();
                        throw new Exception("errorWhileInsertProductBranchOnDB");
                    }

                    transaction.Commit();
                    connection.Close();


                    return new ProductPartnerResponse()
                    {
                        Product = insertedProduct.FirstOrDefault()

                    };

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void CreateImagePartnerProduct(List<string> urls, Guid product_id, string imagedefaultname, Guid created_by)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {

                    connection.Open();
                    var transaction = connection.BeginTransaction();
                    List<ImageProduct> images = new List<ImageProduct>();
                    int count = 0;
                    string image_default = string.Empty;

                    foreach (var item in urls)
                    {

                        var sqlimage = @$"INSERT INTO catalog.product_image
	                                        (url, created_by) 
                                            VALUES ('{item}', '{created_by}') RETURNING *;";

                        var image = connection.Query<ImageProduct>(sqlimage).FirstOrDefault();

                       // image_default = image.Url.Contains(imagedefaultname) ? "'" + image.Product_image_id + "'" : "NULL";

                       

                        images.Add(image);

                        var sqlproductimage = @$"INSERT INTO catalog.product_image_product
	                                        (product_id, product_image_id, created_by) 
                                            VALUES ('{product_id}', '{image.Product_image_id}', '{created_by}') RETURNING *;";

                        var result = connection.Execute(sqlproductimage);
                        count = count + result;

                    }

                    image_default = "'" + images.Where(x => x.Url.Contains(imagedefaultname)).First().Product_image_id + "'";
                    var sqlproduct = @$"UPDATE catalog.product SET
	                                    image_default = {image_default},
                                        updated_by = '{created_by}',
                                        updated_at = CURRENT_TIMESTAMP
                                        WHERE product_id = '{product_id}' RETURNING *;";

                    var insertedProduct = connection.Query<ProductPartner>(sqlproduct).ToList();


                    if (insertedProduct.Count == 0 || count != urls.Count || images.Count != urls.Count)
                    {
                        transaction.Dispose();
                        connection.Close();
                        throw new Exception("errorWhileInsertImageProductOnDB");
                    }

                    transaction.Commit();
                    connection.Close();

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void UpdateImagePartnerProduct(List<string> urls, Guid product_id, string imagedefaultname, Guid created_by)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {

                    connection.Open();
                    var transaction = connection.BeginTransaction();
                    List<ImageProduct> images = new List<ImageProduct>();
                    int count = 0;
                    string image_default = string.Empty;

                    foreach (var item in urls)
                    {

                        var sqlimage = @$"UPDATE catalog.product_image SET
	                                        url = '{item}', 
                                            updated_by = '{created_by}', 
                                            updated_at = CURRENT_TIMESTAMP
                                            FROM catalog.product_image_product pip
                                            WHERE pip.product_id = '{product_id}' AND url = '{item}'
                                            RETURNING *;";

                        var image = connection.Query<ImageProduct>(sqlimage).FirstOrDefault();

                        images.Add(image);

                        var sqlproductimage = @$"UPDATE catalog.product_image_product SET
	                                        updated_by = '{created_by}', 
                                            updated_at = CURRENT_TIMESTAMP
                                            WHERE product_image_id = '{image.Product_image_id}'
                                            RETURNING *;";

                        var result = connection.Execute(sqlproductimage);
                        count += result;

                    }

                    image_default = "'" + images.Where(x => x.Url.Contains(imagedefaultname)).First().Product_image_id + "'" ;
                        //? "'" + image.Product_image_id + "'" : "NULL";
                    var sqlproduct = @$"UPDATE catalog.product SET
	                                    image_default = {image_default},
                                        updated_by = '{created_by}', 
                                        updated_at = CURRENT_TIMESTAMP
                                        WHERE product_id = '{product_id}' RETURNING *;";

                    var insertedProduct = connection.Query<ProductPartner>(sqlproduct).ToList();

                    if (count != urls.Count || images.Count != urls.Count)
                    {
                        transaction.Dispose();
                        connection.Close();
                        throw new Exception("errorWhileUpdatedImageProductOnDB");
                    }

                    transaction.Commit();
                    connection.Close();

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void DeleteImagePartnerProduct(List<string> urls, Guid product_id)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {

                    connection.Open();
                    var transaction = connection.BeginTransaction();
                   
                    int count = 0;
                   

                    foreach (var item in urls)
                    {

                        var sqlimage = @$"DELETE FROM catalog.product_image pi 
                                          USING catalog.product_image_product pip 
                                          WHERE pi.product_image_id = pip.product_image_id 
                                          AND pi.url ='{item}' AND pip.product_id = '{product_id}'";

                        
                        var result = connection.Execute(sqlimage);
                        count += result;

                    }


                    if (count != urls.Count)
                    {
                        transaction.Dispose();
                        connection.Close();
                        throw new Exception("errorWhileDeleteImageProductOnDB");
                    }

                    transaction.Commit();
                    connection.Close();

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public List<string> GetImagesProduct(Guid produc_id)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var sql = $@"SELECT pi.url FROM catalog.product_image_product pip
                                 JOIN catalog.product_image pi ON pi.product_image_id = pip.product_image_id
                                 WHERE pip.product_id = '{produc_id}'";

                    return connection.Query<string>(sql).ToList();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public ListProductPartnerResponse GetProductPartner(Guid branch_id, Filter filter)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    

                    var sql = @$"select 
                                        b.partner_id,
                                        pb.product_id,
                                        p.identifier,
                                        pp.price,
                                        pp.sale_price,
                                        p.minimum_price,
                                        pp.name,
                                        p.type,
                                        pp.description,
                                        pp.active,
                                        pp.reviewer,
                                        pp.created_by,
                                        pp.created_at,
                                        pp.updated_by,
                                        pp.updated_at,
                                        pp.image_default,
                                       ( 
                                        SELECT json_agg(image)
                                        FROM (
                                          SELECT c2.*
                                          FROM catalog.product_image_product c 
                                          JOIN catalog.product_image c2 ON c.product_image_id = c2.product_image_id
                                          WHERE c.product_id = pp.product_id 
                                        ) image
                                       ) AS images
                                        from partner.branch b 
                                        left join catalog.product_branch pb on 
                                        pb.branch_id = b.branch_id
                                        left join catalog.product pp on
                                        pp.product_id = pb.product_id
                                        left join catalog.base_product p on
                                        p.product_id = pp.base_product_id
                                        where b.branch_id in('{branch_id}') and upper(pp.name) like upper('%{filter.filter}%')";

                    var getproducts = connection.Query(sql).Select(x => new ProductPartner()
                    {
                        Product_id = x.product_id,
                        Partner_id = x.partner_id,
                        Image_default = x.image_default,
                        Identifier = x.identifier,
                        Name = x.name,
                        Description = x.description,
                        Minimum_price = x.minimum_price,
                        Price = x.price,
                        Sale_price = x.sale_price,
                        Type = x.type,
                        Active = x.active,
                        Images = !string.IsNullOrEmpty(x.images) ? JsonConvert.DeserializeObject<List<ImageProduct>>(x.images) : new List<ImageProduct>(),
                        Created_by = x.created_by,
                        Created_at = x.created_at,
                        Updated_by = x.updated_by,
                        Updated_at = x.updated_at,
                        Reviewer = x.reviewer,
                    }).ToList();

                    int totalRows = getproducts.Count();
                    float totalPages = (float)totalRows / (float)filter.itensPerPage;

                    totalPages = (float)Math.Ceiling(totalPages);

                    getproducts = getproducts.Skip((int)((filter.page - 1) * filter.itensPerPage)).Take((int)filter.itensPerPage).ToList();

                    return new ListProductPartnerResponse() { Products = getproducts, Pagination = new Pagination() { totalPages = (int)totalPages, totalRows = totalRows } };

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
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var sql = @$"WITH base_product AS (
                                SELECT product_id FROM catalog.base_product where active = true
                                EXCEPT
                                SELECT p.base_product_id FROM catalog.product_branch pb 
                                inner join catalog.product p on p.product_id = pb.product_id
                                WHERE pb.branch_id = '{branch_id}'
                                )SELECT P.* FROM base_product bp
                                INNER JOIN catalog.base_product p on
                                p.product_id = bp.product_id
                                where upper(p.name) like upper('%{filter.filter}%')";
    
                    var getproducts = connection.Query<Product>(sql).ToList();

                    int totalRows = getproducts.Count();
                    float totalPages = (float)totalRows / (float)filter.itensPerPage;

                    totalPages = (float)Math.Ceiling(totalPages);

                    getproducts = getproducts.Skip((int)((filter.page - 1) * filter.itensPerPage)).Take((int)filter.itensPerPage).ToList();



                    return new ListProductResponse() { Products = getproducts, Pagination = new Pagination() { totalPages = (int)totalPages, totalRows = totalRows } };

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
                using (var connection = new NpgsqlConnection(_connectionString))
                {

                    var sql = @$"select 
                                        b.partner_id,
                                        pb.product_id,
                                        p.identifier,
                                        pp.price,
                                        pp.sale_price,
                                        p.minimum_price,
                                        pp.name,
                                        p.type,
                                        pp.description,
                                        pp.active,
                                        pp.created_by,
                                        pp.created_at,
                                        pp.updated_by,
                                        pp.updated_at,
                                        pp.image_default,
                                       ( 
                                        SELECT json_agg(image)
                                        FROM (
                                          SELECT c2.*
                                          FROM catalog.product_image_product c 
                                          JOIN catalog.product_image c2 ON c.product_image_id = c2.product_image_id
                                          WHERE c.product_id = pp.product_id 
                                        ) image
                                       ) AS images
                                        from partner.branch b 
                                        left join catalog.product_branch pb on 
                                        pb.branch_id = b.branch_id
                                        left join catalog.product pp on
                                        pp.product_id = pb.product_id
                                        left join catalog.base_product p on
                                        p.product_id = pp.base_product_id
                                where b.partner_id = '{partner_id}' and pp.product_id = '{product_id}'";

                    var getproducts = connection.Query(sql).Select(x => new ProductPartner()
                    {
                        Product_id = x.product_id,
                        Partner_id = x.partner_id,
                        Image_default = x.image_default,
                        Identifier = x.identifier,
                        Name = x.name,
                        Description = x.description,
                        Minimum_price = x.minimum_price,
                        Price = x.price,
                        Sale_price = x.sale_price,
                        Type = x.type,
                        Active = x.active,
                        Images = !string.IsNullOrEmpty(x.images) ? JsonConvert.DeserializeObject<List<ImageProduct>>(x.images) : new List<ImageProduct>(),
                        Created_by = x.created_by,
                        Created_at = x.created_at,
                        Updated_by = x.updated_by,
                        Updated_at = x.updated_at
                    }).ToList();


                    return new ProductPartnerResponse() { Product = getproducts.FirstOrDefault() };

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public ProductPartnerResponse UpdateProductPartner(UpdateProductPartnerRequest updateProductPartnerRequest)
        {

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {

                    connection.Open();
                    var transaction = connection.BeginTransaction();

                    var sqlinsertproduct = @$"UPDATE catalog.product SET	                                        
                                            name = '{updateProductPartnerRequest.Name}', 
                                            description = '{updateProductPartnerRequest.Description}', 
                                            price = {updateProductPartnerRequest.Price.ToString().Replace(",",".")},
                                            sale_price = {updateProductPartnerRequest.Sale_price.ToString().Replace(",", ".")},
                                            updated_by = '{updateProductPartnerRequest.Updated_by}',
                                            active = {updateProductPartnerRequest.Active},
                                            updated_at = CURRENT_TIMESTAMP,
                                            reviewer = {updateProductPartnerRequest.Reviewer}
                                            WHERE product_id = '{updateProductPartnerRequest.Product_id}' 
                                            RETURNING *;";

                    var updatedProduct = connection.Query<ProductPartner>(sqlinsertproduct).ToList();

                    if (updatedProduct.Count == 0)
                    {
                        transaction.Dispose();
                        connection.Close();
                        throw new Exception("errorWhileUpdateProductOnDB");
                    }

                    transaction.Commit();
                    connection.Close();
                    var response = new ProductPartnerResponse()
                    {
                        Product = updatedProduct.FirstOrDefault()

                    };

                    return response;

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public bool GetOrdersByProduct_id(Guid product_id)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var sql = $@"SELECT * FROM ORDERS.orders_itens where product_id = '{product_id}'";

                    var order = connection.Query<dynamic>(sql).ToList();

                    if (order.Any()) return true;

                    return false;

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public int DeleteProductPartner(Guid product_id)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var sql = $@"DELETE FROM catalog.product
                                WHERE product_id = '{product_id}'";

                    var order = connection.Execute(sql);

                    return order;

                }
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
                using (var connection = new NpgsqlConnection(_connectionString))
                {

                    var sql = @$"select 
                                        b.partner_id,
                                        pb.product_id,
                                        p.identifier,
                                        pp.price,
                                        pp.sale_price,
                                        p.minimum_price,
                                        pp.name,
                                        p.type,
                                        pp.description,
                                        pp.active,
                                        pp.created_by,
                                        pp.created_at,
                                        pp.updated_by,
                                        pp.updated_at,
                                        pp.image_default,
                                        b.branch_id,
                                        b.branch_name,
                                       ( 
                                        SELECT json_agg(image)
                                        FROM (
                                          SELECT c2.*
                                          FROM catalog.product_image_product c 
                                          JOIN catalog.product_image c2 ON c.product_image_id = c2.product_image_id
                                          WHERE c.product_id = pp.product_id 
                                        ) image
                                       ) AS images,
                                        ( 
                                        SELECT json_agg(category)
                                        FROM (
                                          SELECT c.*,(SELECT
                               description AS category_parent_name
                                        FROM   catalog.category
                                        WHERE
                               category_id = c.category_parent_id)
                                          FROM catalog.category_base_product bp 
                                          JOIN catalog.category c ON c.category_id = bp.category_id
                                          WHERE bp.product_id = p.product_id 
                                        ) category
                                       ) AS categories
                                        from partner.branch b 
                                        left join catalog.product_branch pb on 
                                        pb.branch_id = b.branch_id
                                        left join catalog.product pp on
                                        pp.product_id = pb.product_id
                                        left join catalog.base_product p on
                                        p.product_id = pp.base_product_id
                                where pp.product_id = '{product_id}' and pp.active = true";

                    var getproducts = connection.Query(sql).Select(x => new ProductPartnerResponse()
                    {
                        
                        Branch_id = x.branch_id,
                        Branch_name = x.branch_name,
                        Product = new ProductPartner() { 
                        
                        Product_id = x.product_id,
                        Partner_id = x.partner_id,
                        Image_default = x.image_default,
                        Identifier = x.identifier,
                        Name = x.name,
                        Description = x.description,
                        Minimum_price = x.minimum_price,
                        Price = x.price,
                        Sale_price = x.sale_price,
                        Type = x.type,
                        Active = x.active,
                        Images = !string.IsNullOrEmpty(x.images) ? JsonConvert.DeserializeObject<List<ImageProduct>>(x.images) : new List<ImageProduct>(),
                        Categories = !string.IsNullOrEmpty(x.categories) ? JsonConvert.DeserializeObject<List<Category>>(x.categories) : new List<Category>(),
                        Created_by = x.created_by,
                        Created_at = x.created_at,
                        Updated_by = x.updated_by,
                        Updated_at = x.updated_at
                        }

                    }).FirstOrDefault();


                    return getproducts;

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
