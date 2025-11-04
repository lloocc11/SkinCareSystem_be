using System.Threading;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.Chat;
using SkinCareSystem.Services.Base;

namespace SkinCareSystem.Services.InternalServices.IServices
{
    /// <summary>
    /// Orchestrates realtime AI conversations inside an existing chat session.
    /// </summary>
    public interface IAiChatService
    {
        /// <summary>
        /// Processes a user message inside a chat session, invokes RAG + LLM, persists all artifacts and returns the chat turn.
        /// </summary>
        Task<ServiceResult> ChatInSessionAsync(ChatMessageCreateDto dto, CancellationToken cancellationToken = default);
    }
}
