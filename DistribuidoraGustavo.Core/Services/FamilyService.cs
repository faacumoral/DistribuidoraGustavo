using DistribuidoraGustavo.Data.EfModels;
using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Parsers;
using DistribuidoraGustavo.Interfaces.Results;
using DistribuidoraGustavo.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace DistribuidoraGustavo.Core.Services
{
    public class FamilyService : IFamilyService
    {
        private readonly DistribuidoraGustavoContext _context;

        public FamilyService(DistribuidoraGustavoContext context)
        {
            _context = context;
        }

        public async Task<IList<FamilyModel>> GetAll(string filter = "")
        {
            var familiesDb = _context.Families.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                filter = filter.Trim().ToLower();
                familiesDb = familiesDb.Where(f =>
                    f.Name.ToLower().Contains(filter) || f.CodeSuffix.ToLower().Contains(filter));
            }

            // only active products count
            return await familiesDb
                .OrderBy(f => f.Name)
                .Select(f => new FamilyModel
                {
                    FamilyId = f.FamilyId,
                    Name = f.Name,
                    CodeSuffix = f.CodeSuffix,
                    ProductsCount = f.ProductsFamilies.Count(pf => pf.Product.Active == true)
                })
                .ToListAsync();
        }

        public async Task<FamilyModel> GetById(int familyId)
        {
            var family = await _context.Families.FirstOrDefaultAsync(f => f.FamilyId == familyId);
            if (family == null) return null;

            var model = family.ToModel();
            model.ProductsCount = await _context.ProductsFamilies
                .CountAsync(pf => pf.FamilyId == familyId && pf.Product.Active == true);
            return model;
        }

        public async Task<DTOResult<FamilyModel>> Upsert(FamilyModel model)
        {
            var name = model.Name?.Trim();
            var suffix = model.CodeSuffix?.Trim().ToUpper();

            if (string.IsNullOrEmpty(name))
                return DTOResult<FamilyModel>.Error("El nombre es obligatorio");

            if (string.IsNullOrEmpty(suffix))
                return DTOResult<FamilyModel>.Error("El subfijo es obligatorio");

            if (await _context.Families.AnyAsync(f => f.FamilyId != model.FamilyId && f.Name == name))
                return DTOResult<FamilyModel>.Error($"Ya existe una familia llamada '{name}'");

            if (await _context.Families.AnyAsync(f => f.FamilyId != model.FamilyId && f.CodeSuffix == suffix))
                return DTOResult<FamilyModel>.Error($"El subfijo '{suffix}' ya lo usa otra familia");

            Family? family;

            if (model.FamilyId != 0)
            {
                family = await _context.Families.FirstOrDefaultAsync(f => f.FamilyId == model.FamilyId);
                if (family == null)
                    return DTOResult<FamilyModel>.Error("La familia no existe");
            }
            else
            {
                family = new Family();
                _context.Families.Add(family);
            }

            family.Name = name;
            family.CodeSuffix = suffix;

            await _context.SaveChangesAsync();

            return DTOResult<FamilyModel>.Ok(family.ToModel());
        }

        public async Task<BoolResult> Delete(int familyId)
        {
            var family = await _context.Families.FirstOrDefaultAsync(f => f.FamilyId == familyId);

            if (family == null) return BoolResult.Error("La familia no existe");

            // products removed logically keep their link, so any link blocks the delete
            if (await _context.ProductsFamilies.AnyAsync(pf => pf.FamilyId == familyId))
                return BoolResult.Error("No se puede eliminar una familia que tiene productos");

            _context.Families.Remove(family);
            await _context.SaveChangesAsync();

            return BoolResult.Ok();
        }
    }
}
