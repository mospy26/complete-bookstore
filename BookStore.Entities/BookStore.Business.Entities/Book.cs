
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


namespace BookStore.Business.Entities
{

using System;
    using System.Collections.Generic;
    
public partial class Book
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public Book()
    {

        this.Stocks = new HashSet<Stock>();

    }


    public int Id { get; set; }

    public string Title { get; set; }

    public string Author { get; set; }

    public string Genre { get; set; }

    public double Price { get; set; }



    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<Stock> Stocks { get; set; }

}

}
