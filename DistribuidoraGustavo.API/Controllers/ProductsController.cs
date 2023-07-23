using DistribuidoraGustavo.Core.Shared;
using DistribuidoraGustavo.Data.EfModels;
using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Services;
using FMCW.Common.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DistribuidoraGustavo.API.Controllers
{
    public class ProductsController : BaseController
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ListResult<ProductModel>> GetAll(
            [FromQuery] string? filter = "",
            [FromQuery] int? priceListId = 0)
        {
            try
            {
                var products = await _productService.GetAll(filter, priceListId);
                return ListResult<ProductModel>.Ok(products.ToList());
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return ListResult<ProductModel>.Error(ex);
            }
        }

        [HttpGet("prices")]
        public async Task<ListResult<PricedProductModel>> GetWithPrices(
            [FromQuery] string? filter = "",
            [FromQuery] int? priceListId = 0)
        {
            try
            {
                var products = await _productService.GetWithPrices(filter, priceListId);
                return ListResult<PricedProductModel>.Ok(products.ToList());
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return ListResult<PricedProductModel>.Error(ex);
            }
        }


        [HttpPost]
        public async Task<DTOResult<ProductModel>> Upsert([FromBody] ProductModel model)
        {
            try
            {
                var productResult = await _productService.Upsert(model);
                return productResult;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return DTOResult<ProductModel>.Error(ex);
            }
        }

        [HttpGet("{ProductId:int}")]
        public async Task<DTOResult<ProductModel>> Get([FromRoute] int ProductId)
        {
            try
            {
                var product = await _productService.GetById(ProductId);
                return DTOResult<ProductModel>.Ok(product);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return DTOResult<ProductModel>.Error(ex);
            }
        }
    }
}
