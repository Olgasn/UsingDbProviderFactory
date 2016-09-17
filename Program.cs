using System;
using System.Data;
using System.Data.Common;
using System.Configuration;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            GetProviderFactoryClasses();
        }
        
        static DataTable GetProviderFactoryClasses()
        {
            // Получение установленные поставщики и фабрики.
            DataTable table = DbProviderFactories.GetFactoryClasses();

            // Отобразить значения каждой строки и столбца.
            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn column in table.Columns)
                {
                    Console.WriteLine(row[column]);
                }
            }
            return table;
        }
        // Retrieve a connection string by specifying the providerName.
        // Assumes one connection string per provider in the config file.
        static string GetConnectionStringByProvider(string providerName)
        {
            // Return null on failure.
            string returnValue = null;

            // Get the collection of connection strings.
            var settings = ConfigurationManager.ConnectionStrings;

            // Walk through the collection and return the first 
            // connection string matching the providerName.
            if (settings != null)
            {
                foreach (ConnectionStringSettings cs in settings)
                {
                    if (cs.ProviderName == providerName)
                        returnValue = cs.ConnectionString;
                    break;
                }
            }
            return returnValue;
        }

        // Given a provider name and connection string, 
        // create the DbProviderFactory and DbConnection.
        // Returns a DbConnection on success; null on failure.
        static DbConnection CreateDbConnection(
            string providerName, string connectionString)
        {
            // Assume failure.
            DbConnection connection = null;

            // Create the DbProviderFactory and DbConnection.
            if (connectionString != null)
            {
                try
                {
                    DbProviderFactory factory =
                        DbProviderFactories.GetFactory(providerName);

                    connection = factory.CreateConnection();
                    connection.ConnectionString = connectionString;
                }
                catch (Exception ex)
                {
                    // Set the connection to null if it was created.
                    if (connection != null)
                    {
                        connection = null;
                    }
                    Console.WriteLine(ex.Message);
                }
            }
            // Return the connection.
            return connection;
        }

        //Пример извлечения данных
        //В этом примере в качестве аргумента указывается объект DbConnection.
        //Объект DbCommand создается для выбора данных из таблицы Categories путем задания CommandText инструкции SQL SELECT. 
        //Предполагается, что в источнике данных существует таблица Categories.
        //Открывается соединение, и данные получаются при помощи объекта DbDataReader.
        static void DbCommandSelect(DbConnection connection)
        {
            string queryString =
                "SELECT CategoryID, CategoryName FROM Categories";

            // Check for valid DbConnection.
            if (connection != null)
            {
                using (connection)
                {
                    try
                    {
                        // Create the command.
                        DbCommand command = connection.CreateCommand();
                        command.CommandText = queryString;
                        command.CommandType = CommandType.Text;

                        // Open the connection.
                        connection.Open();

                        // Retrieve the data.
                        DbDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            Console.WriteLine("{0}. {1}", reader[0], reader[1]);
                        }
                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception.Message: {0}", ex.Message);
                    }
                }
            }
            else
            {
                Console.WriteLine("Failed: DbConnection is null.");
            }
        }
        // Пример выполнения команды
        //В этом примере в качестве аргумента указывается объект DbConnection.
        //Если объект DbConnection является допустимым, то открывается соединение, 
        //создается и выполняется команда DbCommand.CommandText задается инструкции SQL INSERT, 
        //которая выполняет вставку в таблицу Categories в базе данных Northwind.
        //Предполагается, что база данных Northwind существует в источнике данных, 
        //а также что используемый в инструкции INSERT синтаксис SQL является допустимым для указанного поставщика. 
        //Ошибки в источнике данных обрабатываются блоком кода DbException, а все остальные исключения - в блоке Exception.
        static void ExecuteDbCommand(DbConnection connection)
        {
            // Check for valid DbConnection object.
            if (connection != null)
            {
                using (connection)
                {
                    try
                    {
                        // Open the connection.
                        connection.Open();

                        // Create and execute the DbCommand.
                        DbCommand command = connection.CreateCommand();
                        command.CommandText =
                            "INSERT INTO Categories (CategoryName) VALUES ('Low Carb')";
                        int rows = command.ExecuteNonQuery();

                        // Display number of rows inserted.
                        Console.WriteLine("Inserted {0} rows.", rows);
                    }
                    // Handle data errors.
                    catch (DbException exDb)
                    {
                        Console.WriteLine("DbException.GetType: {0}", exDb.GetType());
                        Console.WriteLine("DbException.Source: {0}", exDb.Source);
                        Console.WriteLine("DbException.ErrorCode: {0}", exDb.ErrorCode);
                        Console.WriteLine("DbException.Message: {0}", exDb.Message);
                    }
                    // Handle all other exceptions.
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception.Message: {0}", ex.Message);
                    }
                }
            }
            else
            {
                Console.WriteLine("Failed: DbConnection is null.");
            }
        }
        //Получение данных с помощью объекта DbDataAdapter
        //В этом примере демонстрируется создание строго типизированного объекта DbDataAdapter на основе имени поставщика и строки соединения.
        //В коде используется метод CreateConnection объекта DbProviderFactory для создания DbConnection.
        //После этого в коде с помощью метода CreateCommand путем указания его свойств CommandText и Connection создается команда DbCommand для выборки данных.
        //Наконец, в коде с помощью метода CreateDataAdapter создается объект DbDataAdapter и устанавливается его свойство SelectCommand. 
        //Метод Fill объекта DbDataAdapter загружает эти данные в DataTable.
        static void CreateDataAdapter(string providerName, string connectionString)
        {
            try
            {
                // Create the DbProviderFactory and DbConnection.
                DbProviderFactory factory =
                    DbProviderFactories.GetFactory(providerName);

                DbConnection connection = factory.CreateConnection();
                connection.ConnectionString = connectionString;

                using (connection)
                {
                    // Define the query.
                    string queryString =
                        "SELECT CategoryName FROM Categories";

                    // Create the DbCommand.
                    DbCommand command = factory.CreateCommand();
                    command.CommandText = queryString;
                    command.Connection = connection;

                    // Create the DbDataAdapter.
                    DbDataAdapter adapter = factory.CreateDataAdapter();
                    adapter.SelectCommand = command;

                    // Fill the DataTable.
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    //  Display each row and column value.
                    foreach (DataRow row in table.Rows)
                    {
                        foreach (DataColumn column in table.Columns)
                        {
                            Console.WriteLine(row[column]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        // Изменение данных с помощью DbDataAdapter
        // В этом примере демонстрируется модификация данных в DataTable с использованием DbDataAdapter, 
        // в котором применяется объект DbCommandBuilder для формирования команд, необходимых для обновления данных в источнике данных.
        // Значение SelectCommand свойства DbDataAdapter устанавливается для получения значений CustomerID и CompanyName из таблицы Customers.
        // Метод GetInsertCommand используется для задания свойства InsertCommand, 
        // метод GetUpdateCommand служит для задания свойства UpdateCommand, 
        // а метод GetDeleteCommand используется для задания свойства DeleteCommand. 
        // В коде осуществляется добавление новой строки в таблицу Customers и обновление источника данных.
        // После этого в коде находится добавленная строка путем поиска по значению CustomerID, представляющему собой первичный ключ, определенный для таблицы Customers. 
        // В коде изменяется значение CompanyName и обновляется источник данных.
        // Наконец, строка удаляется.
        static void CRUDDataAdapter(string providerName, string connectionString)
        {
            try
            {
                // Create the DbProviderFactory and DbConnection.
                DbProviderFactory factory =
                    DbProviderFactories.GetFactory(providerName);

                DbConnection connection = factory.CreateConnection();
                connection.ConnectionString = connectionString;

                using (connection)
                {
                    // Define the query.
                    string queryString =
                        "SELECT CustomerID, CompanyName FROM Customers";

                    // Create the select command.
                    DbCommand command = factory.CreateCommand();
                    command.CommandText = queryString;
                    command.Connection = connection;

                    // Create the DbDataAdapter.
                    DbDataAdapter adapter = factory.CreateDataAdapter();
                    adapter.SelectCommand = command;

                    // Create the DbCommandBuilder.
                    DbCommandBuilder builder = factory.CreateCommandBuilder();
                    builder.DataAdapter = adapter;

                    // Get the insert, update and delete commands.
                    adapter.InsertCommand = builder.GetInsertCommand();
                    adapter.UpdateCommand = builder.GetUpdateCommand();
                    adapter.DeleteCommand = builder.GetDeleteCommand();

                    // Display the CommandText for each command.
                    Console.WriteLine("InsertCommand: {0}",
                        adapter.InsertCommand.CommandText);
                    Console.WriteLine("UpdateCommand: {0}",
                        adapter.UpdateCommand.CommandText);
                    Console.WriteLine("DeleteCommand: {0}",
                        adapter.DeleteCommand.CommandText);

                    // Fill the DataTable.
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    // Insert a new row.
                    DataRow newRow = table.NewRow();
                    newRow["CustomerID"] = "XYZZZ";
                    newRow["CompanyName"] = "XYZ Company";
                    table.Rows.Add(newRow);

                    adapter.Update(table);

                    // Display rows after insert.
                    Console.WriteLine();
                    Console.WriteLine("----List All Rows-----");
                    foreach (DataRow row in table.Rows)
                    {
                        Console.WriteLine("{0} {1}", row[0], row[1]);
                    }
                    Console.WriteLine("----After Insert-----");

                    // Edit an existing row.
                    DataRow[] editRow = table.Select("CustomerID = 'XYZZZ'");
                    editRow[0]["CompanyName"] = "XYZ Corporation";

                    adapter.Update(table);

                    // Display rows after update.
                    Console.WriteLine();
                    foreach (DataRow row in table.Rows)
                    {
                        Console.WriteLine("{0} {1}", row[0], row[1]);
                    }
                    Console.WriteLine("----After Update-----");

                    // Delete a row.
                    DataRow[] deleteRow = table.Select("CustomerID = 'XYZZZ'");
                    foreach (DataRow row in deleteRow)
                    {
                        row.Delete();
                    }

                    adapter.Update(table);

                    // Display rows after delete.
                    Console.WriteLine();
                    foreach (DataRow row in table.Rows)
                    {
                        Console.WriteLine("{0} {1}", row[0], row[1]);
                    }
                    Console.WriteLine("----After Delete-----");
                    Console.WriteLine("Customer XYZZZ was deleted.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }



    }
}
