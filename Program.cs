using System;
using System.Data;
using System.Data.Common;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace UsingDbProviderFactory
{
    class Program
    {
        static void Main(string[] args)
        {

            // Вывод списка имен установленных поставщиков и фабрик
            // из файла конфигурационного файла machine.config
            Console.WriteLine("===== Имена установленных поставщиков и фабрик =====");
            GetProviderFactoryClasses();
            Console.WriteLine();

            //Задание имени поставщика, который будет использоваться для доступа к базе данных
            string providerName = "System.Data.SqlClient";

            // Задать имя каталога, где расположены файлы 
            // базы данных, параметру DataDirectory для использования его в конфигкрационном файле App.Config
            string pathDB = AppDomain.CurrentDomain.BaseDirectory + @"APP_DATA\";
            AppDomain.CurrentDomain.SetData("DataDirectory", pathDB);
            string dpath = ConfigurationManager.AppSettings["DataDirectory"];


            //Получение из конфигурационного файла строки подключения к базе данных
            Console.WriteLine("============ Строка соединения===========");
            string connectionString = GetConnectionStringByProvider(providerName);
            Console.WriteLine(connectionString);
            Console.WriteLine();

            //Получение соединения
            DbConnection conn = CreateDbConnection(providerName, connectionString);
            if (conn != null)
            {
                Console.WriteLine("Соединение установлено!");

            }
            //Извлечение данных с помощью объекта типа DbCommand
            Console.WriteLine("==== Выполнение выборки данных с помощью объекта DbCommand =====");
            DbCommandSelect(conn);
            Console.WriteLine();

            //Изменение данных с помощью объекта типа DbCommand
            conn = CreateDbConnection(providerName, connectionString);
            Console.WriteLine("==== Выполнение изменений данных с помощью объекта DbCommand =====");
            ExecuteDbCommand(conn);
            Console.WriteLine();
            Console.ReadKey();


            //Получение данных с помощью объекта типа DbDataAdapter
            Console.WriteLine("==== Выполнение выборки данных с помощью объекта DbDataAdapter =====");
            CreateDataAdapter(providerName, connectionString);
            Console.WriteLine();

            //Изменение данных с помощью объекта типа DbDataAdapter
            Console.WriteLine("==== Выполнение выборки данных с помощью объекта DbDataAdapter =====");
            CRUDDataAdapter(providerName, connectionString);
            Console.WriteLine();

            Console.ReadKey();
        }
        
        // Получение имен установленных поставщиков и фабрик.
        static DataTable GetProviderFactoryClasses()
        {
            // Получение имен установленных поставщиков и фабрик
            // и сохранение их в объекте типа DataTable.
            DataTable table = DbProviderFactories.GetFactoryClasses();
            // Отобразить значения каждой строки и столбца.
            int i = 1;
            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn column in table.Columns)
                {
                    Console.Write(i.ToString()+". ");
                    Console.WriteLine(row[column]);
                    Console.WriteLine();
                    i += 1;
                }
            }
            return table;
        }
        
        // Получение строки соединения по имени поставщика. 
        // Предполагается, что в конфигурационном файле существует одно соединение для каждого поставщика.
        static string GetConnectionStringByProvider(string providerName)
        {
            // Значение по умолчанию - будет возвращено при ошибке.
            string returnValue = null;

            // Получить коллекцию строк соединения
            var settings = ConfigurationManager.ConnectionStrings;

            // Перебор элементов коллекции для нахождения
            // строки соединения, соответствующей заданному поставщику
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

        // Создание фабрики DbProviderFactory и объекта DbConnection 
        // путем передачи имени поставщика в формате «System.Data.ProviderName» и строки соединения. 
        // В случае успеха возвращается объект DbConnection; в случае любой ошибки - null.
        static DbConnection CreateDbConnection(string providerName, string connectionString)
        {
            // Значение по умолчанию - будет возвращено при ошибке.
            DbConnection connection = null;

            // Создание объектов DbProviderFactory и DbConnection.
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
                    // Установить занчение connection  null, если оно было установлено.
                    if (connection != null)
                    {
                        connection = null;
                    }
                    Console.WriteLine(ex.Message);
                }
            }
            // Вернуть объект Connection
            return connection;
        }

        // Пример извлечения данных
        // В качестве аргумента указывается объект DbConnection.
        // Объект DbCommand создается для выбора данных из таблицы Categories путем задания CommandText инструкции SQL SELECT. 
        // Предполагается, что в источнике данных существует таблица Categories.
        // Открывается соединение, и данные получаются при помощи объекта DbDataReader.
        static void DbCommandSelect(DbConnection connection)
        {
            string queryString =
                "SELECT CategoryID, CategoryName FROM Categories";

            // Проверка объекта DbConnection.
            if (connection != null)
            {
                using (connection)
                {
                    try
                    {
                        // Создать команду
                        DbCommand command = connection.CreateCommand();
                        command.CommandText = queryString;
                        command.CommandType = CommandType.Text;

                        // Открыть соединение
                        connection.Open();

                        // Запросить и получить данные
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
                Console.WriteLine("Ошибка: DbConnection is null.");

            }

        }
        // Пример выполнения команды
        // В качестве аргумента указывается объект DbConnection.
        // Если объект DbConnection является допустимым, то открывается соединение, 
        // создается и выполняется команда DbCommand.CommandText задается инструкции SQL INSERT, 
        // которая выполняет вставку в таблицу Categories в базе данных Northwind.
        // Предполагается, что база данных Northwind существует в источнике данных, 
        // а также что используемый в инструкции INSERT синтаксис SQL является допустимым для указанного поставщика. 
        // Ошибки в источнике данных обрабатываются блоком кода DbException, а все остальные исключения - в блоке Exception.
        static void ExecuteDbCommand(DbConnection connection)
        {
            // Проверка на существование объекта DbConnection.
            if (connection != null)
            {
                using (connection)
                {
                    try
                    {
                        // Открыть соединение.
                        connection.Open();

                        // Создать и выполнить DbCommand.
                        DbCommand command = connection.CreateCommand();
                        command.CommandText =
                            "INSERT INTO Categories (CategoryName) VALUES ('Low Carb')";
                        int rows = command.ExecuteNonQuery();

                        // Отобразить число вставленных строк.
                        Console.WriteLine("Inserted {0} rows.", rows);
                    }
                    // Обработка ошибок свзанных с данными.
                    catch (DbException exDb)
                    {
                        Console.WriteLine("DbException.GetType: {0}", exDb.GetType());
                        Console.WriteLine("DbException.Source: {0}", exDb.Source);
                        Console.WriteLine("DbException.ErrorCode: {0}", exDb.ErrorCode);
                        Console.WriteLine("DbException.Message: {0}", exDb.Message);
                    }
                    // Обработка других ошибок.
                    catch (Exception ex)
                    {
                        Console.WriteLine("Сообщение об исключении: {0}", ex.Message);
                    }
                }
            }
            else
            {
                Console.WriteLine("Ошибка: DbConnection is null.");
            }
        }
        // Получение данных с помощью объекта DbDataAdapter
        // Демонстрируется создание строго типизированного объекта DbDataAdapter на основе имени поставщика и строки соединения.
        // В коде используется метод CreateConnection объекта DbProviderFactory для создания DbConnection.
        // После этого в коде с помощью метода CreateCommand путем указания его свойств CommandText и Connection создается команда DbCommand для выборки данных.
        // Наконец, в коде с помощью метода CreateDataAdapter создается объект DbDataAdapter и устанавливается его свойство SelectCommand. 
        // Метод Fill объекта DbDataAdapter загружает эти данные в DataTable.
        static void CreateDataAdapter(string providerName, string connectionString)
        {
            try
            {
                // Создать DbProviderFactory и DbConnection.
                DbProviderFactory factory =
                    DbProviderFactories.GetFactory(providerName);

                DbConnection connection = factory.CreateConnection();
                connection.ConnectionString = connectionString;

                using (connection)
                {
                    // Определение строки запроса.
                    string queryString =
                        "SELECT CategoryName FROM Categories";

                    // Создание DbCommand.
                    DbCommand command = factory.CreateCommand();
                    command.CommandText = queryString;
                    command.Connection = connection;

                    // Создание DbDataAdapter.
                    DbDataAdapter adapter = factory.CreateDataAdapter();
                    adapter.SelectCommand = command;

                    // Заполнение DataTable.
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    //  Отображение каждой строки и столбца.
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
        // Демонстрируется модификация данных в DataTable с использованием DbDataAdapter, 
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
                // Создать объеты DbProviderFactory и DbConnection.
                DbProviderFactory factory =
                    DbProviderFactories.GetFactory(providerName);

                DbConnection connection = factory.CreateConnection();
                connection.ConnectionString = connectionString;

                using (connection)
                {
                    // Задать строку запроса.
                    string queryString =
                        "SELECT CustomerID, CompanyName FROM Customers";

                    // Создать команду на выборку.
                    DbCommand command = factory.CreateCommand();
                    command.CommandText = queryString;
                    command.Connection = connection;

                    // Создать объект DbDataAdapter.
                    DbDataAdapter adapter = factory.CreateDataAdapter();
                    adapter.SelectCommand = command;

                    // Создать объект DbCommandBuilder.
                    DbCommandBuilder builder = factory.CreateCommandBuilder();
                    builder.DataAdapter = adapter;

                    // Создать команды insert, update и delete 
                    // с помощью методов объекта DbCommandBuilder.
                    adapter.InsertCommand = builder.GetInsertCommand();
                    adapter.UpdateCommand = builder.GetUpdateCommand();
                    adapter.DeleteCommand = builder.GetDeleteCommand();

                    // Отобразить созданное объектом DbCommandBuilder
                    // содержимое для каждой команды insert, update и delete.
                    Console.WriteLine("Сгенерированные команды:");
                    Console.WriteLine("InsertCommand: {0}",
                        adapter.InsertCommand.CommandText);
                    Console.WriteLine("UpdateCommand: {0}",
                        adapter.UpdateCommand.CommandText);
                    Console.WriteLine("DeleteCommand: {0}",
                        adapter.DeleteCommand.CommandText);

                    // Запонить данными объект DataTable.
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    // Вставить новую строку.
                    DataRow newRow = table.NewRow();
                    newRow["CustomerID"] = "XYZZZ";
                    newRow["CompanyName"] = "XYZ Company";
                    table.Rows.Add(newRow);

                    adapter.Update(table);

                    // Отобразить строки после вставки.
                    Console.WriteLine();
                    Console.WriteLine("----Список всех строк-----");
                    foreach (DataRow row in table.Rows)
                    {
                        Console.WriteLine("{0} {1}", row[0], row[1]);
                    }
                    Console.WriteLine("----После вставки-----");

                    // Редактировать существующую строку.
                    DataRow[] editRow = table.Select("CustomerID = 'XYZZZ'");
                    editRow[0]["CompanyName"] = "XYZ Corporation";

                    adapter.Update(table);

                    // Отобразить строки после обновления.
                    Console.WriteLine();
                    foreach (DataRow row in table.Rows)
                    {
                        Console.WriteLine("{0} {1}", row[0], row[1]);
                    }
                    Console.WriteLine("----После обновления-----");

                    // Удалить строку.
                    DataRow[] deleteRow = table.Select("CustomerID = 'XYZZZ'");
                    foreach (DataRow row in deleteRow)
                    {
                        row.Delete();
                    }

                    adapter.Update(table);

                    // Отобразить строки после удаления.
                    Console.WriteLine();
                    foreach (DataRow row in table.Rows)
                    {
                        Console.WriteLine("{0} {1}", row[0], row[1]);
                    }
                    Console.WriteLine("----После удаления-----");
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
