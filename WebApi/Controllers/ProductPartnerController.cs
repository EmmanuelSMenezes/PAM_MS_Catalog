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
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("partner/product")]
    [ApiController]
    public class ProductPartnerController : Controller
    {
        private readonly IProductPartnerService _service;
        private readonly ILogger _logger;
        public ProductPartnerController(IProductPartnerService service, ILogger logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint responsável por vincular um produto com o parceiro.
        /// </summary>
        /// <returns>Valida os dados passados para vincular o produto e retorna os dados cadastrado.</returns>
        [Authorize]
        [HttpPost("create")]
        [ProducesResponseType(typeof(Response<ProductPartnerResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<ProductPartnerResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<ProductPartnerResponse>), StatusCodes.Status500InternalServerError)]
        public ActionResult<Response<ProductPartnerResponse>> CreateNewProduct([FromBody] CreateProductPartnerRequest createProductPartnerRequest)
        {
            try
            {
                var response = _service.CreateProductPartner(createProductPartnerRequest);
                return StatusCode(StatusCodes.Status201Created, new Response<ProductPartnerResponse>() { Status = 200, Message = $"Produto vinculado ao parceiro com sucesso.", Data = response, Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception while creating new product in partner!");
                switch (ex.Message)
                {
                    case "errorWhileInsertProductOnDB":
                        return StatusCode(StatusCodes.Status400BadRequest, new Response<ProductPartnerResponse>() { Status = 400, Message = $"Não foi possível vincular produto ao parceiro. Erro no processo de inserção do produto na base de dados.", Success = false, Error = ex.Message });
                    case "errorWhileInsertProductBranchOnDB": 
                        return StatusCode(StatusCodes.Status400BadRequest, new Response<ProductPartnerResponse>() { Status = 400, Message = $"Não foi possível vincular produto ao parceiro. Erro no processo de vincular filial ao produto na base de dados.", Success = false, Error = ex.Message });

                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<ProductPartnerResponse>() { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false, Error = ex.Message });
                }
            }
        }

        /// <summary>
        /// Endpoint responsável por cadastrar a imagem do produto.
        /// </summary>
        /// <returns>Valida os dados passados para criação do produto e retorna os dados cadastrado.</returns>
        [Authorize]
        [HttpPost("image/create")]
        [ProducesResponseType(typeof(Response<ProductPartnerResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(Response<ProductPartnerResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<ProductPartnerResponse>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<ProductPartnerResponse>>> CreateNewImageProduct([FromForm] CreateImageProductPartnerRequest createImageProductRequest)
        {
            try
            {

                var token = Request.Headers["Authorization"];
                var response = await _service.CreateImagePartnerProduct(createImageProductRequest, token);
                return StatusCode(StatusCodes.Status201Created, new Response<ProductPartnerResponse>() { Status = 201, Message = $"Imagens do produto registrado com sucesso.", Data = response, Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception while creating new product!");
                switch (ex.Message)
                {

                    case "errorWhileInsertImageProductOnDB":
                        return StatusCode(StatusCodes.Status400BadRequest, new Response<ProductPartnerResponse>() { Status = 400, Message = $"Não foi possível salvar imagens do produto. Erro no processo de inserção das imagens na base de dados.", Success = false, Error = ex.Message });
                    case "errorWhileUpdatedImageProductOnDB":
                        return StatusCode(StatusCodes.Status400BadRequest, new Response<ProductPartnerResponse>() { Status = 400, Message = $"Não foi possível salvar imagens do produto. Erro no processo de alteração das imagens na base de dados.", Success = false, Error = ex.Message });
                    case "errorWhileDeleteImageProductOnDB":
                        return StatusCode(StatusCodes.Status400BadRequest, new Response<ProductPartnerResponse>() { Status = 400, Message = $"Não foi possível salvar imagens do produto. Erro no processo de exclição das imagens na base de dados.", Success = false, Error = ex.Message });
                    case "failedWhileUploadedImage":
                        return StatusCode(StatusCodes.Status400BadRequest, new Response<ProductPartnerResponse>() { Status = 400, Message = $"Não foi possível salvar imagens do produto. Erro no processo de carregar imagens no storage.", Success = false, Error = ex.Message });
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<ProductPartnerResponse>() { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false, Error = ex.Message });
                }
            }
        }

        /// <summary>
        /// Endpoint responsável por buscar produtos que ainda não foram vinculados ao parceiro.
        /// </summary>
        /// <returns>Retorna produtos cadastrados.</returns>
        [Authorize]
        [HttpGet("get/baseproduct/{branch_id}")]
        [ProducesResponseType(typeof(Response<ListProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<ListProductResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<ListProductResponse>), StatusCodes.Status500InternalServerError)]
        public ActionResult<Response<ListProductResponse>> GetProductBaseByPartner(Guid branch_id, string filter, int? page, int? itensPerPage)
        {
            try
            {
                var filters = new Filter
                {
                    filter = filter,
                    page = page ?? 1,
                    itensPerPage = itensPerPage ?? 5
                };

                var response = _service.GetBaseProductPartner(branch_id, filters);
                return StatusCode(StatusCodes.Status200OK, new Response<ListProductResponse>() { Status = 200, Message = $"Produto retornado com sucesso.", Data = response, Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception when listing partner product!");
                switch (ex.Message)
                {
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<ListProductResponse>() { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false, Error = ex.Message });
                }
            }
        }

        /// <summary>
        /// Endpoint responsável por buscar produto pelo id do parceiro.
        /// </summary>
        /// <returns>Retorna produtos cadastrados.</returns>
        [Authorize]
        [HttpGet("{branch_id}")]
        [ProducesResponseType(typeof(Response<ListProductPartnerResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<ListProductPartnerResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<ListProductPartnerResponse>), StatusCodes.Status500InternalServerError)]
        public ActionResult<Response<ListProductPartnerResponse>> GetProductPartnerByPartner(Guid branch_id, string filter, int? page, int? itensPerPage)
        {
            try
            {
                var filters = new Filter
                {
                    page = page ?? 1,
                    itensPerPage = itensPerPage ?? 5,
                    filter = filter
                };

                var response = _service.GetProductPartner(branch_id, filters);
                return StatusCode(StatusCodes.Status200OK, new Response<ListProductPartnerResponse>() { Status = 200, Message = $"Produto retornado com sucesso.", Data = response, Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception when listing partner product!");
                switch (ex.Message)
                {
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<ListProductPartnerResponse>() { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false, Error = ex.Message });
                }
            }
        }

        /// <summary>
        /// Endpoint responsável por buscar produto pelo id.
        /// </summary>
        /// <returns>Retorna produto cadastrados.</returns>
        [Authorize]
        [HttpGet("id/{product_id}")]
        [ProducesResponseType(typeof(Response<ProductPartnerResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<ProductPartnerResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<ProductPartnerResponse>), StatusCodes.Status500InternalServerError)]
        public ActionResult<Response<ProductPartnerResponse>> GetProductPartnerByProductId(Guid product_id)
        {
            try
            {
                

                var response = _service.GetProductPartner(product_id);
                return StatusCode(StatusCodes.Status200OK, new Response<ProductPartnerResponse>() { Status = 200, Message = $"Produto retornado com sucesso.", Data = response, Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception when listing partner product!");
                switch (ex.Message)
                {
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<ProductPartnerResponse>() { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false, Error = ex.Message });
                }
            }
        }


        /// <summary>
        /// Endpoint responsável por alterar produto do parceiro.
        /// </summary>
        /// <returns>Retorna produto alterado.</returns>
        [Authorize]
        [HttpPut("update")]
        [ProducesResponseType(typeof(Response<ProductPartnerResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<ProductPartnerResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<ProductPartnerResponse>), StatusCodes.Status500InternalServerError)]
        public ActionResult<Response<ProductPartnerResponse>> UpdateProductPartner([FromBody] UpdateProductPartnerRequest updateProductPartnerRequest)
        {
            try
            {
                var token = Request.Headers["Authorization"];
                var response = _service.UpdateProductPartner(updateProductPartnerRequest, token);
                return StatusCode(StatusCodes.Status200OK, new Response<ProductPartnerResponse>() { Status = 200, Message = $"Produto alterado com sucesso.", Data = response, Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception while upting product in partner!");
                switch (ex.Message)
                {
                    case "errorWhileUpdatedtProductOnDB":
                        return StatusCode(StatusCodes.Status304NotModified, new Response<ProductPartnerResponse>() { Status = 304, Message = $"Não foi possível alterar produto. Erro no processo de alteração do produto na base de dados.", Success = false, Error = ex.Message });
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<ProductPartnerResponse>() { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false, Error = ex.Message });
                }
            }
        }

        /// <summary>
        /// Endpoint responsável por desvincular produto da filial.
        /// </summary>
        /// <returns>Retorna status 204 em caso de sucesso!</returns>
        [Authorize]
        [HttpDelete("{product_id}")]
        [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status500InternalServerError)]
        public ActionResult<Response<Guid>> DeleteProductPartner(Guid product_id)
        {
            try
            {
                var response = _service.DeleteProductPartner(product_id);
                
                return StatusCode(StatusCodes.Status200OK, new Response<Guid>() { Status = 200, Message = $"Produto desvinculado com sucesso.", Data = product_id, Success = true });
}
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception while deleting product in partner!");
                switch (ex.Message)
                {
                    case "ProductLinkedOrder":
                        return StatusCode(StatusCodes.Status400BadRequest, new Response<Guid>() { Status = 409, Message = $"Não foi possível remover o produto. Produto vinculado à uma ordem.", Success = false, Error = ex.Message });
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<Guid>() { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false, Error = ex.Message });
                }
            }
        }


    }
}

