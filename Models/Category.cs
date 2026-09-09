//this is for Category of each product.eg, bull dog is a product, and categor is Dog.
//each instance = a row in DB, a property of an instance = a col
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace WingtipToys.Models
{
	public class Category
	{
		[ScaffoldColumn(false)]
        public int CategoryID { get; set; }

        [Required, StringLength(100), Display(Name = "Category Name")]
        public string CategoryName { get; set; }

        [Display(Name = "Product Description")]
        public string Description { get; set; }
        public virtual ICollection<Product> Products { get; set; }

    }
}