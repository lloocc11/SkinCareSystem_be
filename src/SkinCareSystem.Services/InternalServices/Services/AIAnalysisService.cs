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
    public class AIAnalysisService : IAIAnalysisService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AIAnalysisService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<IServiceResult> CreateAnalysisAsync(AIAnalysisCreateDto dto)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                return new ServiceResult(Const.ERROR_VALIDATION_CODE, "User not found");
            }

            var message = await _unitOfWork.ChatMessageRepository.GetByIdAsync(dto.ChatMessageId);
            if (message == null)
            {
                return new ServiceResult(Const.ERROR_VALIDATION_CODE, "Chat message not found");
            }

            var existing = await _unitOfWork.AIAnalysisRepository.GetByMessageIdAsync(dto.ChatMessageId);
            if (existing != null)
            {
                return new ServiceResult(Const.WARNING_DATA_EXISTED_CODE, "Analysis already exists for this message");
            }

            var entity = dto.ToEntity();
            await _unitOfWork.AIAnalysisRepository.CreateAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ServiceResult(StatusCodes.Status201Created, Const.SUCCESS_CREATE_MSG, entity.ToDto());
        }

        public async Task<IServiceResult> GetAnalysisByMessageAsync(Guid messageId)
        {
            var analysis = await _unitOfWork.AIAnalysisRepository.GetByMessageIdAsync(messageId);
            if (analysis == null)
            {
                return new ServiceResult(Const.WARNING_NO_DATA_CODE, "Analysis not found");
            }

            return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, analysis.ToDto());
        }

        public async Task<IServiceResult> GetAnalysesBySessionAsync(Guid sessionId)
        {
            var analyses = await _unitOfWork.AIAnalysisRepository.GetBySessionAsync(sessionId);
            if (analyses == null || analyses.Count == 0)
            {
                return new ServiceResult(Const.WARNING_NO_DATA_CODE, "No analyses found for this session");
            }

            var items = analyses.Select(a => a.ToDto()).ToList();
            return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, items);
        }
    }
}
