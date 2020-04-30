
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
    
public partial class Order
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public Order()
    {

        this.OrderItems = new HashSet<OrderItem>();

    }


    public int Id { get; set; }

    public Nullable<double> Total { get; set; }

    public System.DateTime OrderDate { get; set; }

    public string Warehouse { get; set; }

    public string Store { get; set; }

    public System.Guid OrderNumber { get; set; }



    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<OrderItem> OrderItems { get; set; }

    public virtual User Customer { get; set; }

    public virtual Delivery Delivery { get; set; }

}

}
