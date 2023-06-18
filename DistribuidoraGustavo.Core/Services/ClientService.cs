﻿using DistribuidoraGustavo.Data.EfModels;
using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Parsers;
using DistribuidoraGustavo.Interfaces.Services;
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
            var clientsDb = await _context.Clients.ToListAsync();
            return clientsDb.Select(CastEfToModel.ToModel).ToList();
        }
    }
}
