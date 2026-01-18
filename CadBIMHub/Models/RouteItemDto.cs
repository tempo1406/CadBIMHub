using System;
using System.Collections.Generic;

namespace CadBIMHub.Models
{
    public class RouteItemDto
    {
        public int Id { get; set; }
        public List<object> CustomFields { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string BatchNo { get; set; }
        public string Symbol { get; set; }
        public double Quantity { get; set; }
        public int ItemGroupId { get; set; }
        public string ItemGroupName { get; set; }
        public int ItemId { get; set; }
        public string ItemDescription { get; set; }
        public int SizeId { get; set; }
        public string SizeName { get; set; }
        public int Status { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime ModifyTime { get; set; }
        public string CreateBy { get; set; }
        public int IsActive { get; set; }
    }

    public class RouteDataResponse
    {
        public List<RouteItemDto> Items { get; set; }
        public int TotalCount { get; set; }
    }
}
