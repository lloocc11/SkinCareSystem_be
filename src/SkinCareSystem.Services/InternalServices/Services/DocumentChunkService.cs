using SkinCareSystem.Common.DTOs.MedicalDocument;
using SkinCareSystem.Common.Enum.ServiceResultEnums;
using SkinCareSystem.Repositories.UnitOfWork;
using SkinCareSystem.Services.Base;
using SkinCareSystem.Services.InternalServices.IServices;
using SkinCareSystem.Services.Mapping;

namespace SkinCareSystem.Services.InternalServices.Services
{
    public class DocumentChunkService : IDocumentChunkService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DocumentChunkService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IServiceResult> GetDocumentChunkByIdAsync(Guid chunkId)
        {
            try
            {
                var chunk = await _unitOfWork.DocumentChunks.GetByIdAsync(chunkId);
                if (chunk == null)
                {
                    return new ServiceResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
                }

                return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, chunk.ToDto()!);
            }
            catch (Exception ex)
            {
                return new ServiceResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<IServiceResult> GetDocumentChunksByDocumentAsync(Guid docId)
        {
            try
            {
                var chunks = await _unitOfWork.DocumentChunks.GetByDocumentIdAsync(docId);
                var chunkDtos = chunks.Select(c => c.ToDto()).ToList();
                return new ServiceResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, chunkDtos);
            }
            catch (Exception ex)
            {
                return new ServiceResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<IServiceResult> CreateDocumentChunkAsync(DocumentChunkCreateDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return new ServiceResult(Const.FAIL_CREATE_CODE, "Invalid chunk data");
                }

                var chunk = dto.ToEntity();
                await _unitOfWork.DocumentChunks.CreateAsync(chunk);
                await _unitOfWork.SaveAsync();

                return new ServiceResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, chunk.ToDto()!);
            }
            catch (Exception ex)
            {
                return new ServiceResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<IServiceResult> UpdateDocumentChunkAsync(Guid chunkId, DocumentChunkUpdateDto dto)
        {
            try
            {
                var chunk = await _unitOfWork.DocumentChunks.GetByIdAsync(chunkId);
                if (chunk == null)
                {
                    return new ServiceResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
                }

                chunk.ApplyUpdate(dto);
                await _unitOfWork.DocumentChunks.UpdateAsync(chunk);
                await _unitOfWork.SaveAsync();

                return new ServiceResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, chunk.ToDto()!);
            }
            catch (Exception ex)
            {
                return new ServiceResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<IServiceResult> DeleteDocumentChunkAsync(Guid chunkId)
        {
            try
            {
                var chunk = await _unitOfWork.DocumentChunks.GetByIdAsync(chunkId);
                if (chunk == null)
                {
                    return new ServiceResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
                }

                await _unitOfWork.DocumentChunks.RemoveAsync(chunk);
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
