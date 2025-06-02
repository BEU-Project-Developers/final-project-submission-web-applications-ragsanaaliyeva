using System.Collections.Generic;
using System.Linq;

namespace FurnitureManagementSystem.ViewModels
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal GrandTotal => Items.Sum(item => item.TotalPrice);
    }
}