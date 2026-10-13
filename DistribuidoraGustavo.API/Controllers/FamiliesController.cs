using DistribuidoraGustavo.Interfaces.Shared;
using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Results;
using DistribuidoraGustavo.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace DistribuidoraGustavo.API.Controllers
{
    public class FamiliesController : BaseController
    {
        private readonly IFamilyService _familyService;

        public FamiliesController(IFamilyService familyService)
        {
            _familyService = familyService;
        }

        [HttpGet]
        public async Task<ListResult<FamilyModel>> GetAll([FromQuery] string? filter = "")
        {
            try
            {
                var families = await _familyService.GetAll(filter ?? "");
                return ListResult<FamilyModel>.Ok(families.ToList());
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return ListResult<FamilyModel>.Error(ex);
            }
        }

        [HttpGet("{familyId:int}")]
        public async Task<DTOResult<FamilyModel>> Get([FromRoute] int familyId)
        {
            try
            {
                var family = await _familyService.GetById(familyId);

                if (family == null)
                    return DTOResult<FamilyModel>.Error("La familia no existe");

                return DTOResult<FamilyModel>.Ok(family);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return DTOResult<FamilyModel>.Error(ex);
            }
        }

        [HttpPost]
        public async Task<DTOResult<FamilyModel>> Upsert([FromBody] FamilyModel model)
        {
            try
            {
                return await _familyService.Upsert(model);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return DTOResult<FamilyModel>.Error(ex);
            }
        }

        [HttpDelete("{familyId:int}")]
        public async Task<BoolResult> Delete([FromRoute] int familyId)
        {
            try
            {
                return await _familyService.Delete(familyId);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return BoolResult.Error(ex);
            }
        }
    }
}
