using eRynok.DTOs.Orders;

namespace eRynok.DTOs.OrderGroups
{
    /// <summary>
    /// DTO для відображення групового замовлення.
    /// </summary>
    public class OrderGroupResponseDto
    {
        /// <summary>
        /// Ідентифікатор групового замовлення.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Ідентифікатор покупця.
        /// </summary>
        public int BuyerId { get; set; }

        /// <summary>
        /// Дата та час створення групового замовлення.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Загальна вартість усіх замовлень у групі.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Список окремих замовлень, що входять до групи.
        /// </summary>
        public List<OrderResponseDto> Orders { get; set; }
            = new List<OrderResponseDto>();
    }
}