
# BookStore in .NET
COMP5348 Assignment to improve the robustnestness and realism of a current BookStore application 

## Getting Started


### Opening up the file
1. Download the .ZIP File and unzip the file
2. Execute `Bookstore.sln` file

### Database Setup 
1. Create the database by going on `Tools` (on the toolbar) ---> `SQL Server` ---> `New Query`
2. Should a pop up appear, go to Local ---> MSSQLLocalDB then connect
4. Copy and paste the SQL commands in the attached `CreateDatabases.sql`. Exclude the `drop database	` commands if the databases do not exist 
5. Execute on 
6. Run each of the `<Database>EntityModel.edmx.sql` files to build the schema for these databases (BookStore, Bank, DeliveryCo)

Once you have done all the above preliminary setups, run each of the processes below by right clicking on it, Debug ---> Start New Instance 
- Bank ---> Bank.Application ---> Bank.Process 
- BookStore ---> BookStore.Application ---> BookStore.Process
- DeliveryCo ---> DeliveryCo.Application ---> DeliveryCo.Process
- EmailService ---> EmailService.Application ---> EmailService.Process


## Authors

* [Mustafa Fulwala](https://github.com/mospy26/)
* [Kevin Su](https://github.com/SuKaiwen)
* [Brendon Lam](https://github.com/blam135)
