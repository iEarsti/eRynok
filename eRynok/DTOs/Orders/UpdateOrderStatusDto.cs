using eRynok.Models;
using System.ComponentModel.DataAnnotations;

namespace eRynok.DTOs.Orders
{
    /// <summary>
    /// Дані для зміни статусу існуючого замовлення.
    /// </summary>
    public class UpdateOrderStatusDto
    {
        /// <summary>
        /// Новий статус замовлення: Pending, Confirmed, Processing, Shipped, Delivered або Cancelled.
        /// </summary>
        [EnumDataType(typeof(OrderStatus))]
        public OrderStatus Status { get; set; }
    }
}
