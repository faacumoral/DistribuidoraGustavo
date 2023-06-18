﻿using DistribuidoraGustavo.Core.Shared;
using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Services;
using FMCW.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace DistribuidoraGustavo.API.Controllers
{
    public class PriceListsController : BaseController
    {
        private readonly IPriceListService _priceListService;

        public PriceListsController(IPriceListService priceListService)
        {
            _priceListService = priceListService;
        }

        [HttpGet]
        public async Task<ListResult<PriceListModel>> GetAll()
        {
            try
            {
                var priceLists = await _priceListService.GetAll();
                return ListResult<PriceListModel>.Ok(priceLists.ToList());
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return ListResult<PriceListModel>.Error(ex);
            }
        }
    }
}