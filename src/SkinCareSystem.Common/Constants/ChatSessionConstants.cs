using System;
using System.Collections.Generic;

namespace SkinCareSystem.Common.Constants;

public static class ChatSessionChannels
{
    public const string Ai = "ai";
    public const string AiAdmin = "ai_admin";
    public const string Specialist = "specialist";

    private static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        Ai,
        AiAdmin,
        Specialist
    };

    public static bool TryNormalize(string? input, out string normalized)
    {
        normalized = (input ?? string.Empty).Trim().ToLowerInvariant();
        return Allowed.Contains(normalized);
    }

    public static IReadOnlyCollection<string> Values => Allowed;
}

public static class ChatSessionStates
{
    public const string Open = "open";
    public const string WaitingSpecialist = "waiting_specialist";
    public const string Assigned = "assigned";
    public const string Closed = "closed";

    private static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        Open,
        WaitingSpecialist,
        Assigned,
        Closed
    };

    public static bool TryNormalize(string? input, out string normalized)
    {
        normalized = (input ?? string.Empty).Trim().ToLowerInvariant();
        return Allowed.Contains(normalized);
    }

    public static IReadOnlyCollection<string> Values => Allowed;
}
