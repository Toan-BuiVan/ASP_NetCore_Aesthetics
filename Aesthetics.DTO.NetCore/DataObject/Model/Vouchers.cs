﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.DataObject.Model
{
    public class Vouchers
    {
        [Key]
        public int VoucherID { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
		public string? VoucherImage { get; set; }
		public double? DiscountValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public decimal? MinimumOrderValue { get; set; }
		public decimal? MaxValue { get; set; }
		public string? RankMember { get; set; }
		public int? RatingPoints { get; set; }
		public int? AccumulatedPoints { get; set; }
		public int? IsActive { get; set; }
		public Invoice Invoice { get; set; }
		public InvoiceDetail InvoiceDetails { get; set; }
		public ICollection<Wallets> Wallets { get; set; }
    }
}
