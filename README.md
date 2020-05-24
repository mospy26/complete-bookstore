
# BookStore in .NET Framework (v4.5)
COMP5348 Assignment to improve the robustnestness and realism of a current BookStore application integrated with multiple other applications

## Getting Started


### Opening up the file
1. Download the .ZIP File and unzip its contents
2. Navigate to `Bookstore.sln` and execute it by double clicking on it (Note that you should have [Visual Studio](https://visualstudio.microsoft.com/downloads/) installed and you must be running it on a Windows OS

### Database Setup 
1. Create the database by going on `Tools` (on the toolbar of Visual Studio) ---> `SQL Server` ---> `New Query`
2. Should a pop up appear, go to Local ---> MSSQLLocalDB then connect
4. Copy and paste the SQL commands in the attached `CreateDatabases.sql`. Exclude the `drop database ...` commands if the databases do not exist 
5. Execute the SQL Command by clicking the play button on the top right below the tabs and wait until Visual Studio prompts that the SQL statements execution succeeded 
6. After the databases were created successfully, run each of the `<Database>EntityModel.edmx.sql` files to build the schema for these databases (BookStore, Bank, DeliveryCo) similarly by clicking the play button. This time you may be prompted to select the SQL server in which case you must choose MSSQLLocalDB 

Once you have done all the above preliminary setups, run each of the processes below by right clicking on it, Debug ---> Start New Instance 
```
Bank ---> Bank.Application ---> Bank.Process
BookStore ---> BookStore.Application ---> BookStore.Process
DeliveryCo ---> DeliveryCo.Application ---> DeliveryCo.Process
EmailService ---> EmailService.Application ---> EmailService.Process
BookStore ---> BookStore.WebClient ---> BookStore.WebClient
```

For the `.Process` instances, 4 terminals should pop up, each saying that the Service hsa started. 
  
Then finally starting an instance of the `BookStore.WebClient` opens up the BookStore page in a browser window (at localhost:1274)

### Using the Application

You can log in to the BookStore Application using the following credentials:
```
Username: customer
Password: COMP5348
```

## Authors

* [Mustafa Fulwala](https://github.com/mospy26/)
* [Kevin Su](https://github.com/SuKaiwen)
* [Brendon Lam](https://github.com/blam135)
