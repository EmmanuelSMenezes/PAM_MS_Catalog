using Application.Service;
using Application.Service.Interfaces;
using Domain.Model;
using Domain.Model.Product.Image.Request;
using Domain.Model.Product.Request;
using Domain.Model.Product.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("product")]
    [ApiController]
    public class ProductBaseController : Controller
    {
        private readonly IProductService _service;
        private readonly ILogger _logger;
        public ProductBaseController(IProductService service, ILogger logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint responsável por criar um produto.
        /// </summary>
        /// <returns>Valida os dados passados para criação do produto e retorna os dados cadastrado.</returns>
        [Authorize]
        [HttpPost("create")]
        [ProducesResponseType(typeof(Response<ProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<ProductResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<ProductResponse>), StatusCodes.Status500InternalServerError)]
        public ActionResult<Response<ProductResponse>> CreateNewProduct([FromBody] CreateProductRequest createProductRequest)
        {
            try
            {

                var response = _service.CreateProduct(createProductRequest);
                return StatusCode(StatusCodes.Status201Created, new Response<ProductResponse>() { Status = 200, Message = $"Produto registrado com sucesso.", Data = response, Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception while creating new product!");
                switch (ex.Message)
                {
                    case "errorWhileInsertProductOnDB":
                        return StatusCode(StatusCodes.Status304NotModified, new Response<ProductResponse>() { Status = 304, Message = $"Não foi possível registrar produto. Erro no processo de inserção do produto na base de dados.", Success = false, Error = ex.Message });
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<ProductResponse>() { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false, Error = ex.Message });
                }
            }
        }

        /// <summary>
        /// Endpoint responsável por cadastrar a imagem do produto.
        /// </summary>
        /// <returns>Valida os dados passados para criação do produto e retorna os dados cadastrado.</returns>
        [Authorize]
        [HttpPost("image/create")]
        [ProducesResponseType(typeof(Response<ProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<ProductResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<ProductResponse>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<ProductResponse>>> CreateNewImageProduct([FromForm] CreateImageProductRequest createImageProductRequest)
        {
            try
            {
                var token = Request.Headers["Authorization"];
                var response = await _service.CreateImageProduct(createImageProductRequest, token);
                return StatusCode(StatusCodes.Status201Created, new Response<ProductResponse>() { Status = 200, Message = $"Image do produto registrado com sucesso.", Data = response, Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception while creating new product!");
                switch (ex.Message)
                {

                    case "errorWhileInsertProductOnDB":
                        return StatusCode(StatusCodes.Status304NotModified, new Response<ProductResponse>() { Status = 304, Message = $"Não foi possível registrar produto. Erro no processo de inserção do produto na base de dados.", Success = false, Error = ex.Message });
                    case "failedWhileUpdateImage":
                        return StatusCode(StatusCodes.Status304NotModified, new Response<ProductResponse>() { Status = 304, Message = $"Não foi possível registrar a imagem do produto. Erro no processo de inserção da imagem do produto na base de dados.", Success = false, Error = ex.Message });

                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<ProductResponse>() { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false, Error = ex.Message });
                }
            }
        }

        /// <summary>
        /// Endpoint responsável por listar todos produtos.
        /// </summary>
        /// <returns>Retorna lista com todos produtos cadastrados.</returns>
        [Authorize]
        [HttpGet("get")]
        [ProducesResponseType(typeof(Response<ListProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<ListProductResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<ListProductResponse>), StatusCodes.Status500InternalServerError)]
        public ActionResult<Response<ListProductResponse>> GetAllProduct(string filter, int? page, int? itensPerPage)
        {
            try
            {
                var filters = new Filter
                {
                    page = page ?? 1,
                    itensPerPage = itensPerPage ?? 5,
                    filter = filter
                };

                var response = _service.GetProduct(filters);
                return StatusCode(StatusCodes.Status200OK, new Response<ListProductResponse>() { Status = 200, Message = $"Produtos retornado com sucesso.", Data = response, Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception while listing inputs!");
                switch (ex.Message)
                {
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<ListProductResponse> () { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false, Error = ex.Message});
                }
            }
        }

        /// <summary>
        /// Endpoint responsável por buscar produto pelo id.
        /// </summary>
        /// <returns>Retorna produtos cadastrados.</returns>
        [Authorize]
        [HttpGet("get/{product_id}")]
        [ProducesResponseType(typeof(Response<ProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<ProductResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<ProductResponse>), StatusCodes.Status500InternalServerError)]
        public ActionResult<Response<ProductResponse>> GetProduct(Guid product_id)
        {
            try
            {
                var response = _service.GetProduct(product_id);
                return StatusCode(StatusCodes.Status200OK, new Response<ProductResponse>() { Status = 200, Message = $"Produto retornado com sucesso.", Data = response, Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception while listing product!");
                switch (ex.Message)
                {
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<ProductResponse>() { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false, Error = ex.Message });
                }
            }
        }

        /// <summary>
        /// Endpoint responsável por alterar product.
        /// </summary>
        /// <returns>Retorna product alterado.</returns>
        [Authorize]
        [HttpPut("update")]
        [ProducesResponseType(typeof(Response<ProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<ProductResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<ProductResponse>), StatusCodes.Status500InternalServerError)]
        public ActionResult<Response<ProductResponse>> UpdateProduct([FromBody] UpdateProductRequest updateProductRequest)
        {
            try
            {
                var response = _service.UpdateProduct(updateProductRequest);
                return StatusCode(StatusCodes.Status200OK, new Response<ProductResponse>() { Status = 200, Message = $"Produto alterado com sucesso.", Data = response, Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception while upting product!");
                switch (ex.Message)
                {
                    case "errorWhileUpdatedtProductOnDB":
                        return StatusCode(StatusCodes.Status304NotModified, new Response<ProductResponse>() { Status = 304, Message = $"Não foi possível alterar produto. Erro no processo de alteração do produto na base de dados.", Success = false, Error = ex.Message });
                    case "errorWhileDeleteProducCategorytOnDB":
                        return StatusCode(StatusCodes.Status304NotModified, new Response<ProductResponse>() { Status = 304, Message = $"Não foi possível alterar produto. Erro no processo de alteração do produto na base de dados.", Success = false, Error = ex.Message });
                    case "errorWhileInsertProducCategorytOnDB":
                        return StatusCode(StatusCodes.Status304NotModified, new Response<ProductResponse>() { Status = 304, Message = $"Não foi possível alterar produto. Erro no processo de alteração do produto na base de dados.", Success = false, Error = ex.Message });
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<ProductResponse>() { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false, Error = ex.Message });
                }
            }
        }


    }
}

