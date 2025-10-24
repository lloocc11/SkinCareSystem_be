using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.Consent;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Mapping;

namespace SkinCareSystem.Services.InternalServices.Services
{
    public class ConsentRecordService : IConsentRecordService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ConsentRecordService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IServiceResult> GetConsentRecordByIdAsync(Guid consentId)
        {
            try
            {
                var record = await _unitOfWork.ConsentRecords.GetByIdAsync(consentId);
                if (record == null)
                {
                    return new ServiceResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
                }

                return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, record.ToDto()!);
            }
            catch (Exception ex)
            {
                return new ServiceResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<IServiceResult> GetConsentRecordsByUserAsync(Guid userId)
        {
            try
            {
                var records = await _unitOfWork.ConsentRecords.GetByUserIdAsync(userId);
                var recordDtos = records.Select(r => r.ToDto()).ToList();
                return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, recordDtos);
            }
            catch (Exception ex)
            {
                return new ServiceResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<IServiceResult> CreateConsentRecordAsync(ConsentRecordCreateDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return new ServiceResult(Const.FAIL_CREATE_CODE, "Invalid consent record data");
                }

                var record = dto.ToEntity();
                await _unitOfWork.ConsentRecords.CreateAsync(record);
                await _unitOfWork.SaveAsync();

                return new ServiceResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, record.ToDto()!);
            }
            catch (Exception ex)
            {
                return new ServiceResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<IServiceResult> UpdateConsentRecordAsync(Guid consentId, ConsentRecordUpdateDto dto)
        {
            try
            {
                var record = await _unitOfWork.ConsentRecords.GetByIdAsync(consentId);
                if (record == null)
                {
                    return new ServiceResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
                }

                record.ApplyUpdate(dto);
                await _unitOfWork.ConsentRecords.UpdateAsync(record);
                await _unitOfWork.SaveAsync();

                return new ServiceResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, record.ToDto()!);
            }
            catch (Exception ex)
            {
                return new ServiceResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<IServiceResult> DeleteConsentRecordAsync(Guid consentId)
        {
            try
            {
                var record = await _unitOfWork.ConsentRecords.GetByIdAsync(consentId);
                if (record == null)
                {
                    return new ServiceResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
                }

                await _unitOfWork.ConsentRecords.RemoveAsync(record);
                await _unitOfWork.SaveAsync();

                return new ServiceResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG);
            }
            catch (Exception ex)
            {
                return new ServiceResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }
    }
}
