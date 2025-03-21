﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStock.Models
{
    public class ProductSold
    {

        [Key]
        public int Id { get; set; }

        [ForeignKey("SaleTransaction")]
        public int SalesTransactionId { get; set; }

        public SalesTransaction SalesTransaction { get; set; }
    
        public required int Quantity { get; set; }

        public required decimal SoldPrice { get; set; }

        [ForeignKey("ProductId")]
        public required int ProductId { get; set; }

        public required string ProductTitle { get; set; }

        public required decimal Discount { get; set; }
    }
}
