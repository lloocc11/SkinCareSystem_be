using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SkinCareSystem.Common.DTOs.Rule;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Mapping;

namespace SkinCareSystem.Services.InternalServices.Services
{
    public class RuleConditionService : IRuleConditionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RuleConditionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<IServiceResult> GetRuleConditionsByRuleAsync(Guid ruleId)
        {
            var rule = await _unitOfWork.RuleRepository.GetByIdAsync(ruleId);
            if (rule == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "Rule not found"
                };
            }

            var conditions = await _unitOfWork.RuleConditionRepository.GetByRuleIdAsync(ruleId);
            var conditionDtos = conditions.Select(rc => rc.ToDto()).Where(dto => dto != null).ToList();

            return new ServiceResult
            {
                Status = Const.SUCCESS_READ_CODE,
                Message = Const.SUCCESS_READ_MSG,
                Data = conditionDtos
            };
        }

        public async Task<IServiceResult> GetRuleConditionByIdAsync(Guid ruleConditionId)
        {
            var condition = await _unitOfWork.RuleConditionRepository.GetByIdWithDetailsAsync(ruleConditionId);
            if (condition == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Rule condition not found"
                };
            }

            return new ServiceResult
            {
                Status = Const.SUCCESS_READ_CODE,
                Message = Const.SUCCESS_READ_MSG,
                Data = condition.ToDto()
            };
        }

        public async Task<IServiceResult> CreateRuleConditionAsync(RuleConditionCreateDto dto)
        {
            if (dto == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                };
            }

            var rule = await _unitOfWork.RuleRepository.GetByIdAsync(dto.RuleId);
            if (rule == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = "Rule not found"
                };
            }

            var entity = dto.ToEntity();
            await _unitOfWork.RuleConditionRepository.CreateAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = StatusCodes.Status201Created,
                Message = Const.SUCCESS_CREATE_MSG,
                Data = entity.ToDto()
            };
        }

        public async Task<IServiceResult> UpdateRuleConditionAsync(Guid ruleConditionId, RuleConditionUpdateDto dto)
        {
            var condition = await _unitOfWork.RuleConditionRepository.GetByIdAsync(ruleConditionId);
            if (condition == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Rule condition not found"
                };
            }

            condition.ApplyUpdate(dto);
            await _unitOfWork.RuleConditionRepository.UpdateAsync(condition);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = Const.SUCCESS_UPDATE_CODE,
                Message = Const.SUCCESS_UPDATE_MSG,
                Data = condition.ToDto()
            };
        }

        public async Task<IServiceResult> DeleteRuleConditionAsync(Guid ruleConditionId)
        {
            var condition = await _unitOfWork.RuleConditionRepository.GetByIdAsync(ruleConditionId);
            if (condition == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Rule condition not found"
                };
            }

            await _unitOfWork.RuleConditionRepository.RemoveAsync(condition);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = Const.SUCCESS_UPDATE_CODE,
                Message = Const.SUCCESS_DELETE_MSG
            };
        }
    }
}
