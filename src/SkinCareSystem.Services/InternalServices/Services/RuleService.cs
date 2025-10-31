using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SkinCareSystem.Common.DTOs.Pagination;
using SkinCareSystem.Common.DTOs.Rule;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Mapping;

namespace SkinCareSystem.Services.InternalServices.Services
{
    public class RuleService : IRuleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RuleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<IServiceResult> GetRulesAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                };
            }

            try
            {
                var query = _unitOfWork.RuleRepository.GetAllQueryable()
                    .OrderByDescending(r => r.created_at);

                var totalItems = await query.CountAsync();
                if (totalItems == 0)
                {
                    return new ServiceResult
                    {
                        Status = Const.WARNING_NO_DATA_CODE,
                        Message = Const.WARNING_NO_DATA_MSG
                    };
                }

                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                var rules = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var ruleDtos = rules
                    .Select(r => r.ToDto())
                    .Where(dto => dto != null)
                    .Select(dto => dto!)
                    .ToList();

                var pagedResult = new PagedResult<RuleDto>
                {
                    Items = ruleDtos,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };

                return new ServiceResult
                {
                    Status = Const.SUCCESS_READ_CODE,
                    Message = Const.SUCCESS_READ_MSG,
                    Data = pagedResult
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_EXCEPTION,
                    Message = ex.Message
                };
            }
        }

        public async Task<IServiceResult> GetRuleByIdAsync(Guid ruleId)
        {
            var rule = await _unitOfWork.RuleRepository.GetByIdWithDetailsAsync(ruleId);
            if (rule == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Rule not found"
                };
            }

            return new ServiceResult
            {
                Status = Const.SUCCESS_READ_CODE,
                Message = Const.SUCCESS_READ_MSG,
                Data = rule.ToDto()
            };
        }

        public async Task<IServiceResult> CreateRuleAsync(RuleCreateDto dto)
        {
            if (dto == null)
            {
                return new ServiceResult
                {
                    Status = Const.ERROR_VALIDATION_CODE,
                    Message = Const.ERROR_INVALID_DATA_MSG
                };
            }

            var entity = dto.ToEntity();
            await _unitOfWork.RuleRepository.CreateAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = StatusCodes.Status201Created,
                Message = Const.SUCCESS_CREATE_MSG,
                Data = entity.ToDto()
            };
        }

        public async Task<IServiceResult> UpdateRuleAsync(Guid ruleId, RuleUpdateDto dto)
        {
            var rule = await _unitOfWork.RuleRepository.GetByIdAsync(ruleId);
            if (rule == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Rule not found"
                };
            }

            rule.ApplyUpdate(dto);
            await _unitOfWork.RuleRepository.UpdateAsync(rule);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = Const.SUCCESS_UPDATE_CODE,
                Message = Const.SUCCESS_UPDATE_MSG,
                Data = rule.ToDto()
            };
        }

        public async Task<IServiceResult> DeleteRuleAsync(Guid ruleId)
        {
            var rule = await _unitOfWork.RuleRepository.GetByIdAsync(ruleId);
            if (rule == null)
            {
                return new ServiceResult
                {
                    Status = Const.WARNING_NO_DATA_CODE,
                    Message = "Rule not found"
                };
            }

            await _unitOfWork.RuleRepository.RemoveAsync(rule);
            await _unitOfWork.SaveAsync();

            return new ServiceResult
            {
                Status = Const.SUCCESS_UPDATE_CODE,
                Message = Const.SUCCESS_DELETE_MSG
            };
        }
    }
}
