using DistribuidoraGustavo.Data.EfModels;
using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Parsers;
using DistribuidoraGustavo.Interfaces.Services;
using FMCW.Common.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistribuidoraGustavo.Core.Services
{
    public class ClientService : IClientService
    {
        private readonly DistribuidoraGustavoContext _context;

        public ClientService(DistribuidoraGustavoContext context)
        {
            _context = context;
        }

        public async Task<IList<ClientModel>> GetAll()
        {
            var clientsDb = await _context.Clients.Include(c => c.DefaultPriceList).ToListAsync();
            return clientsDb.Select(CastEfToModel.ToModel).ToList();
        }

        public async Task<ClientModel> GetById(int clientId)
        {
            var client = await _context.Clients.Include(c => c.DefaultPriceList)
                .FirstOrDefaultAsync(c => c.ClientId == clientId);

            return client.ToModel();
        }

        public async Task<DTOResult<ClientModel>> UpdateClient(ClientModel clientModel)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == clientModel.ClientId);
            if (client is null)
                return DTOResult<ClientModel>.Error("El cliente no existe");

            client.Name = clientModel.Name;
            client.DefaultPriceListId = clientModel.DefaultPriceList.PriceListId;

            await _context.SaveChangesAsync();
            return client.ToModel();
        }

        public async Task<DTOResult<ClientModel>> InsertClient(ClientModel clientModel)
        {
            var prefixList = await _context.Clients.Select(c => c.InvoicePrefix).ToListAsync(); 
	       
            clientModel.InvoicePrefix = clientModel.InvoicePrefix.TrimEnd('-');
            if (prefixList.Contains(clientModel.InvoicePrefix))
                return DTOResult<ClientModel>.Error("El prefijo ya existe");

            var client = new Client
            {
                Name = clientModel.Name,
                DefaultPriceListId = clientModel.DefaultPriceList.PriceListId,
                InvoicePrefix = clientModel.InvoicePrefix
            };

            _context.Add(client);
            await _context.SaveChangesAsync();
            return client.ToModel();
        }
    }
}
