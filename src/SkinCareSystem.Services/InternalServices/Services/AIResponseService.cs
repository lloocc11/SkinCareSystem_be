using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SkinCareSystem.Common.DTOs.AI;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Mapping;

namespace SkinCareSystem.Services.InternalServices.Services
{
    public class AIResponseService : IAIResponseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AIResponseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<IServiceResult> CreateResponseAsync(AIResponseCreateDto dto)
        {
            var query = await _unitOfWork.UserQueryRepository.GetByIdAsync(dto.QueryId);
            if (query == null)
            {
                return new ServiceResult(Const.ERROR_VALIDATION_CODE, "User query not found");
            }

            var entity = dto.ToEntity();
            await _unitOfWork.AIResponseRepository.CreateAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ServiceResult(StatusCodes.Status201Created, Const.SUCCESS_CREATE_MSG, entity.ToDto());
        }

        public async Task<IServiceResult> GetResponseAsync(Guid responseId)
        {
            var response = await _unitOfWork.AIResponseRepository.GetByIdAsync(responseId);
            if (response == null)
            {
                return new ServiceResult(Const.WARNING_NO_DATA_CODE, "AI response not found");
            }

            return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, response.ToDto());
        }

        public async Task<IServiceResult> GetResponsesByQueryAsync(Guid queryId)
        {
            var responses = await _unitOfWork.AIResponseRepository.GetByQueryAsync(queryId);
            if (responses == null || responses.Count == 0)
            {
                return new ServiceResult(Const.WARNING_NO_DATA_CODE, "No responses found for query");
            }

            var dtos = responses.Select(r => r.ToDto()).ToList();
            return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, dtos);
        }
    }
}
