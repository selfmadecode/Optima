using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class Rate : BaseEntity
    {
        [Column(TypeName = "decimal(18,4)")] //This will store 18 digits in the database with 4 of those after the decimal.
        public decimal Amount { get; set; }
    }
}