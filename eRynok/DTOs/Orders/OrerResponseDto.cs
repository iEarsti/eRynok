using eRynok.Models;

namespace eRynok.DTOs.Orders
{
    /// <summary>
    /// Дані замовлення, які повертаються клієнту.
    /// </summary>
    public class OrderResponseDto
    {
        /// <summary>
        /// Унікальний ідентифікатор замовлення.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Ідентифікатор покупця, який створив замовлення.
        /// </summary>
        public int BuyerId { get; set; }

        /// <summary>
        /// Ідентифікатор замовленого товару.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Назва замовленого товару.
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Кількість одиниць товару в замовленні.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Ціна однієї одиниці товару на момент створення замовлення.
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Загальна вартість замовлення.
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Поточний статус замовлення.
        /// </summary>
        public OrderStatus Status { get; set; }

        /// <summary>
        /// Дата та час створення замовлення.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
