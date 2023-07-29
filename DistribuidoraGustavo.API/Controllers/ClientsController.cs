using DistribuidoraGustavo.Core.Shared;
using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Services;
using FMCW.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace DistribuidoraGustavo.API.Controllers
{
    public class ClientsController : BaseController
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpGet]
        public async Task<ListResult<ClientModel>> GetAll()
        {
            try
            {
                var clients = await _clientService.GetAll();
                return ListResult<ClientModel>.Ok(clients.ToList());
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return ListResult<ClientModel>.Error(ex);
            }
        }

        [HttpGet("{clientId:int}")]
        public async Task<DTOResult<ClientModel>> GetById([FromRoute] int clientId)
        {
            try
            {
                var client = await _clientService.GetById(clientId);
                return DTOResult<ClientModel>.Ok(client);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return DTOResult<ClientModel>.Error(ex);
            }
        }

        [HttpPost]
        public async Task<DTOResult<ClientModel>> UpsertClient([FromBody] ClientModel clientModel)
        {
            if ((clientModel is null) || string.IsNullOrEmpty(clientModel.Name) || clientModel.DefaultPriceList is null)
                return DTOResult<ClientModel>.Error("No se pudieron cargar los datos del cliente");
            if ((clientModel.ClientId != 0) && string.IsNullOrEmpty(clientModel.InvoicePrefix))
                return DTOResult<ClientModel>.Error("No se pudieron cargar los datos del cliente");

            try
            {
                if (clientModel.ClientId == 0)
                    return await _clientService.InsertClient(clientModel);
                else
                    return await _clientService.UpdateClient(clientModel);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return DTOResult<ClientModel>.Error(ex);
            }
        }
    }
}
