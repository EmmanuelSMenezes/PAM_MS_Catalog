using Application.Service;
using Application.Service.Interfaces;
using Domain.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("category")]
    [ApiController]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _service;
        private readonly ILogger _logger;
        public CategoryController(ICategoryService service, ILogger logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint responsável por criar uma categoria.
        /// </summary>
        /// <returns>Valida os dados passados para criação da categoria e retorna os dados cadastrado.</returns>
        [Authorize]
        [HttpPost("create")]
        [ProducesResponseType(typeof(Response<CategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<CategoryResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<CategoryResponse>), StatusCodes.Status500InternalServerError)]
        public ActionResult<Response<CategoryResponse>> CreateNewCategory([FromBody] CreateCategoryRequest createCategoryRequest)
        {
            try
            {
                var response = _service.CreateCategory(createCategoryRequest);
                return StatusCode(StatusCodes.Status201Created, new Response<CategoryResponse>() { Status = 200, Message = $"Categoria registrada com sucesso.", Data = response, Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception while creating new category!");
                switch (ex.Message)
                {
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<CategoryResponse>() { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false });
                }
            }
        }

        /// <summary>
        /// Endpoint responsável por listar todas categoria.
        /// </summary>
        /// <returns>Retorna lista com todas categorias cadastradas.</returns>
        [Authorize]
        [HttpGet("get")]
        [ProducesResponseType(typeof(Response<ListCategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<ListCategoryResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<ListCategoryResponse>), StatusCodes.Status500InternalServerError)]
        public ActionResult<Response<ListCategoryResponse>> GetAllCategory(string filter, int? page, int? itensPerPage)
        {
            try
            {
                var filters = new Filter
                {
                    page = page ?? 1,
                    itensPerPage = itensPerPage ?? 5,
                    filter = filter
                };
                var response = _service.GetCategory(filters);
                return StatusCode(StatusCodes.Status200OK, new Response<ListCategoryResponse>() { Status = 200, Message = $"Categorias retornada com sucesso.", Data = response, Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception while listing categories!");
                switch (ex.Message)
                {
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<ListCategoryResponse>() { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false });
                }
            }
        }

        /// <summary>
        /// Endpoint responsável por buscar categoria pelo id.
        /// </summary>
        /// <returns>Retorna categoria cadastradas.</returns>
        [Authorize]
        [HttpGet("get/{category_id}")]
        [ProducesResponseType(typeof(Response<ListCategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<ListCategoryResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<ListCategoryResponse>), StatusCodes.Status500InternalServerError)]
        public ActionResult<Response<ListCategoryResponse>> GetCategory(Guid category_id)
        {
            try
            {
                var response = _service.GetCategory(category_id);
                return StatusCode(StatusCodes.Status200OK, new Response<ListCategoryResponse>() { Status = 200, Message = $"Categoria retornada com sucesso.", Data = response, Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception while listing categories!");
                switch (ex.Message)
                {
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<ListCategoryResponse>() { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false });
                }
            }
        }

        /// <summary>
        /// Endpoint responsável por listar todas subcategorias da categorias informadas.
        /// </summary>
        /// <returns>Retorna lista com todas subcategorias das categorias informadas.</returns>
        [Authorize]
        [HttpPost("subcategory/get")]
        [ProducesResponseType(typeof(Response<ListCategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<ListCategoryResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<ListCategoryResponse>), StatusCodes.Status500InternalServerError)]
        public ActionResult<Response<ListCategoryResponse>> GetCategoryIds([FromBody] GetSubcategoryRequest getSubcategoryRequest)
        {
            try
            {
                var response = _service.GetCategory(getSubcategoryRequest);
                return StatusCode(StatusCodes.Status200OK, new Response<ListCategoryResponse>() { Status = 200, Message = $"Categorias retornada com sucesso.", Data = response, Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception while listing categories!");
                switch (ex.Message)
                {
                    case "NoContentCategories":
                        return StatusCode(StatusCodes.Status204NoContent, new Response<ListCategoryResponse>() { Status = 204, Message = $"Não há categorias cadastrada", Data = new ListCategoryResponse(), Success = true });
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<ListCategoryResponse>() { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false });
                }
            }
        }

        /// <summary>
        /// Endpoint responsável por alterar categoria.
        /// </summary>
        /// <returns>Retorna categoria alterada.</returns>
        [Authorize]
        [HttpPut("update")]
        [ProducesResponseType(typeof(Response<CategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<CategoryResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<CategoryResponse>), StatusCodes.Status500InternalServerError)]
        public ActionResult<Response<CategoryResponse>> UpdateCategory([FromBody] UpdateCategoryRequest updateCategoryRequest)
        {
            try
            {
                var response = _service.UpdateCategory(updateCategoryRequest);
                return StatusCode(StatusCodes.Status200OK, new Response<CategoryResponse>() { Status = 200, Message = $"Categoria alterada com sucesso.", Data = response, Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception while upting category!");
                switch (ex.Message)
                {
                    case "categoryAssignedProduct":
                        var mensage = updateCategoryRequest.Category_parent_id is null ? "categoria" : "subcategoria";
                        return StatusCode(StatusCodes.Status403Forbidden, new Response<ListCategoryResponse>() { Status = 403, Message = $"Não é possivel desabilitar " + mensage + ", pois ela está vinculada à um produto", Data = new ListCategoryResponse(), Success = true });

                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<CategoryResponse>() { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false });
                }
            }
        }
    }
}

