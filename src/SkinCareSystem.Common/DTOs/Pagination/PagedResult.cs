using System.Collections.Generic;

namespace SkinCareSystem.Common.DTOs.Pagination
{
    public class PagedResult<T>
    {
        public IReadOnlyCollection<T> Items { get; set; } = new List<T>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}
