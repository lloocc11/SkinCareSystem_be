using System;
using System.Collections.Generic;
using SkinCareSystem.Repositories.Models;

namespace SkinCareSystem.Services.InternalServices.Models;

public sealed record RoutineRecommendation(
    Guid RoutineId,
    string Title,
    string ShortDescription,
    string? TargetSkinType,
    string? TargetConditions,
    double SimilarityScore,
    string InitialReason,
    string CandidateContext,
    Routine Routine,
    IReadOnlyList<RoutineStep> Steps);
