//manages the entity class, and provides data access to DB 
//managing -> fetching, storing and updating Product class instances(~rows) in the Db
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity; //allows the capibilities of query, insert, update, delete

namespace WingtipToys.Models
{
	public class ProductContext : DbContext //inheriting from Db content
	{
		public ProductContext() : base("WingtipToys") //base-> constructor of the parent class
        {
        }
        public DbSet <Category> Categories { get; set; }    
        public DbSet<Product> Products { get; set; }
    }
}