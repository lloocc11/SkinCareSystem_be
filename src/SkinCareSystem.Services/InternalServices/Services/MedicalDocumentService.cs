using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.MedicalDocument;
using SkinCareSystem.Common.DTOs.Pagination;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Mapping;

namespace SkinCareSystem.Services.InternalServices.Services
{
    public class MedicalDocumentService : IMedicalDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MedicalDocumentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IServiceResult> GetMedicalDocumentsAsync(int pageNumber, int pageSize)
        {
            try
            {
                var allDocuments = await _unitOfWork.MedicalDocuments.GetAllAsync();
                var totalRecords = allDocuments.Count();

                var pagedDocuments = allDocuments
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(d => d.ToDto())
                    .ToList();

                var pagedResult = new PagedResult<MedicalDocumentDto>
                {
                    Items = pagedDocuments!,
                    TotalItems = totalRecords,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
                };

                return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, pagedResult);
            }
            catch (Exception ex)
            {
                return new ServiceResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<IServiceResult> GetMedicalDocumentByIdAsync(Guid docId)
        {
            try
            {
                var document = await _unitOfWork.MedicalDocuments.GetByIdWithDetailsAsync(docId);
                if (document == null)
                {
                    return new ServiceResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
                }

                return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, document.ToDto()!);
            }
            catch (Exception ex)
            {
                return new ServiceResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<IServiceResult> CreateMedicalDocumentAsync(MedicalDocumentCreateDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return new ServiceResult(Const.FAIL_CREATE_CODE, "Invalid document data");
                }

                var document = dto.ToEntity();
                await _unitOfWork.MedicalDocuments.CreateAsync(document);
                await _unitOfWork.SaveAsync();

                var created = await _unitOfWork.MedicalDocuments.GetByIdWithDetailsAsync(document.doc_id);
                return new ServiceResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, created?.ToDto());
            }
            catch (Exception ex)
            {
                return new ServiceResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<IServiceResult> UpdateMedicalDocumentAsync(Guid docId, MedicalDocumentUpdateDto dto)
        {
            try
            {
                var document = await _unitOfWork.MedicalDocuments.GetByIdAsync(docId);
                if (document == null)
                {
                    return new ServiceResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
                }

                document.ApplyUpdate(dto);
                await _unitOfWork.MedicalDocuments.UpdateAsync(document);
                await _unitOfWork.SaveAsync();

                var updated = await _unitOfWork.MedicalDocuments.GetByIdWithDetailsAsync(docId);
                return new ServiceResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, updated?.ToDto());
            }
            catch (Exception ex)
            {
                return new ServiceResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<IServiceResult> DeleteMedicalDocumentAsync(Guid docId)
        {
            try
            {
                var document = await _unitOfWork.MedicalDocuments.GetByIdAsync(docId);
                if (document == null)
                {
                    return new ServiceResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
                }

                await _unitOfWork.MedicalDocuments.RemoveAsync(document);
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
