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
                .FirstOrDefaultAsync(md => md.DocId == docId);
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
                .Include(dc => dc.Doc)
                .Where(dc => dc.DocId == docId)
                .OrderBy(dc => dc.CreatedAt)
                .ToListAsync();
        }

        public async Task<DocumentChunk?> GetByIdWithDetailsAsync(Guid chunkId)
        {
            return await _context.Set<DocumentChunk>()
                .AsNoTracking()
                .Include(dc => dc.Doc)
                .FirstOrDefaultAsync(dc => dc.ChunkId == chunkId);
        }
    }
}
