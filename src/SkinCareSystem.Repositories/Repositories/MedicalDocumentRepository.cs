using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkinCareSystem.Repositories.Base;
using SkinCareSystem.Repositories.DBContext;
using SkinCareSystem.Repositories.IRepositories;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Repositories.Repositories
{
    public class MedicalDocumentRepository : GenericRepository<MedicalDocument>, IMedicalDocumentRepository
    {
        public MedicalDocumentRepository(SkinCareSystemDbContext context) : base(context)
        {
        }

        public async Task<MedicalDocument?> GetByIdWithDetailsAsync(Guid docId)
        {
            return await _context.Set<MedicalDocument>()
                .AsNoTracking()
                .Include(md => md.DocumentChunks)
                .Include(md => md.MedicalDocumentAssets)
                .FirstOrDefaultAsync(md => md.doc_id == docId);
        }
    }

    public class DocumentChunkRepository : GenericRepository<DocumentChunk>, IDocumentChunkRepository
    {
        public DocumentChunkRepository(SkinCareSystemDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<DocumentChunk>> GetByDocumentIdAsync(Guid docId)
        {
            return await _context.Set<DocumentChunk>()
                .AsNoTracking()
                .Include(dc => dc.doc)
                .Where(dc => dc.doc_id == docId)
                .OrderBy(dc => dc.created_at)
                .ToListAsync();
        }

        public async Task<DocumentChunk?> GetByIdWithDetailsAsync(Guid chunkId)
        {
            return await _context.Set<DocumentChunk>()
                .AsNoTracking()
                .Include(dc => dc.doc)
                .FirstOrDefaultAsync(dc => dc.chunk_id == chunkId);
        }
    }

    public class MedicalDocumentAssetRepository : GenericRepository<MedicalDocumentAsset>, IMedicalDocumentAssetRepository
    {
        public MedicalDocumentAssetRepository(SkinCareSystemDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<MedicalDocumentAsset>> GetByDocumentIdAsync(Guid docId)
        {
            return await _context.Set<MedicalDocumentAsset>()
                .AsNoTracking()
                .Include(asset => asset.doc)
                .Where(asset => asset.doc_id == docId)
                .OrderByDescending(asset => asset.created_at)
                .ToListAsync();
        }

        public async Task<MedicalDocumentAsset?> GetByPublicIdAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                return null;
            }

            return await _context.Set<MedicalDocumentAsset>()
                .AsNoTracking()
                .Include(asset => asset.doc)
                .FirstOrDefaultAsync(asset => asset.public_id == publicId);
        }
    }
}
