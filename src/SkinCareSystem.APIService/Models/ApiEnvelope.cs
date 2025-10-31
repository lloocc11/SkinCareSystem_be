using System;

namespace SkinCareSystem.APIService.Models;

public sealed record ApiResponse<T>(
    int Status,
    bool Success,
    string Message,
    T? Data,
    DateTime Timestamp)
{
    public static ApiResponse<T> Create(int status, bool success, string message, T? data) =>
        new(status, success, message, data, DateTime.UtcNow);
}

public static class Api
{
    public static ApiResponse<T> Ok<T>(T data, string message = "OK", int status = 200) =>
        ApiResponse<T>.Create(status, true, message, data);

    public static ApiResponse<T> Created<T>(T data, string message = "Created") =>
        ApiResponse<T>.Create(201, true, message, data);

    public static ApiResponse<object> Fail(string message, int status) =>
        ApiResponse<object>.Create(status, false, message, null);
}
