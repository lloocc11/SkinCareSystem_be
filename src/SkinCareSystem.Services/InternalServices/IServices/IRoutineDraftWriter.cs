using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkinCareSystem.Common.DTOs.AI;

namespace SkinCareSystem.Services.InternalServices.IServices;

public interface IRoutineDraftWriter
{
    Task<Guid?> SaveDraftAsync(RoutineDraftDto draft, Guid creatorUserId, string? targetSkinType, IList<string> targetConditions);
}
