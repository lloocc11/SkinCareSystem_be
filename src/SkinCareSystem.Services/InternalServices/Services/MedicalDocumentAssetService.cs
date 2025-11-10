using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SkinCareSystem.Common.DTOs.MedicalDocument.MedicalDocumentAssetDTOs;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Mapping;

namespace SkinCareSystem.Services.InternalServices.Services
{
    public class MedicalDocumentAssetService : IMedicalDocumentAssetService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MedicalDocumentAssetService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<IServiceResult> GetAssetByIdAsync(Guid assetId)
        {
            var asset = await _unitOfWork.MedicalDocumentAssets.GetByIdAsync(assetId);
            if (asset == null)
            {
                return new ServiceResult(Const.WARNING_NO_DATA_CODE, "Asset not found");
            }

            return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, asset.ToDto());
        }

        public async Task<IServiceResult> GetAssetsByDocumentAsync(Guid documentId)
        {
            var document = await _unitOfWork.MedicalDocuments.GetByIdAsync(documentId);
            if (document == null)
            {
                return new ServiceResult(Const.ERROR_VALIDATION_CODE, "Document not found");
            }

            var assets = await _unitOfWork.MedicalDocumentAssets.GetByDocumentIdAsync(documentId);
            var payload = assets.Select(a => a.ToDto()).ToList();
            return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, payload);
        }

        public async Task<IServiceResult> CreateAssetAsync(MedicalDocumentAssetCreateDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null)
            {
                return new ServiceResult(Const.ERROR_INVALID_DATA_CODE, "Invalid asset payload");
            }

            if (dto.DocId == Guid.Empty)
            {
                return new ServiceResult(Const.ERROR_VALIDATION_CODE, "Document Id is required");
            }

            if (string.IsNullOrWhiteSpace(dto.FileUrl))
            {
                return new ServiceResult(Const.ERROR_VALIDATION_CODE, "FileUrl is required");
            }

            var document = await _unitOfWork.MedicalDocuments.GetByIdAsync(dto.DocId);
            if (document == null)
            {
                return new ServiceResult(Const.ERROR_VALIDATION_CODE, "Document not found");
            }

            if (!string.IsNullOrWhiteSpace(dto.PublicId))
            {
                var existingByPublicId = await _unitOfWork.MedicalDocumentAssets.GetByPublicIdAsync(dto.PublicId);
                if (existingByPublicId != null)
                {
                    return new ServiceResult(Const.WARNING_DATA_EXISTED_CODE, "Asset already exists for this public id", existingByPublicId.ToDto());
                }
            }

            var entity = dto.ToEntity();
            await _unitOfWork.MedicalDocumentAssets.CreateAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ServiceResult(StatusCodes.Status201Created, Const.SUCCESS_CREATE_MSG, entity.ToDto());
        }

        public async Task<IServiceResult> UpdateAssetAsync(Guid assetId, MedicalDocumentAssetUpdateDto dto, CancellationToken cancellationToken = default)
        {
            var asset = await _unitOfWork.MedicalDocumentAssets.GetByIdAsync(assetId);
            if (asset == null)
            {
                return new ServiceResult(Const.WARNING_NO_DATA_CODE, "Asset not found");
            }

            asset.ApplyUpdate(dto);
            await _unitOfWork.MedicalDocumentAssets.UpdateAsync(asset);
            await _unitOfWork.SaveAsync();

            return new ServiceResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, asset.ToDto());
        }

        public async Task<IServiceResult> DeleteAssetAsync(Guid assetId, CancellationToken cancellationToken = default)
        {
            var asset = await _unitOfWork.MedicalDocumentAssets.GetByIdAsync(assetId);
            if (asset == null)
            {
                return new ServiceResult(Const.WARNING_NO_DATA_CODE, "Asset not found");
            }

            await _unitOfWork.MedicalDocumentAssets.RemoveAsync(asset);
            await _unitOfWork.SaveAsync();

            return new ServiceResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG);
        }
    }
}
