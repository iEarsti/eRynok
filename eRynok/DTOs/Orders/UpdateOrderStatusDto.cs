using eRynok.Models;
using System.ComponentModel.DataAnnotations;

namespace eRynok.DTOs.Orders
{
    public class UpdateOrderStatusDto
    {
        [EnumDataType(typeof(OrderStatus))]
        public OrderStatus Status { get; set; }
    }
}
