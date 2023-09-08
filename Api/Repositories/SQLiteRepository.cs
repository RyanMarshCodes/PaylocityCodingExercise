using Api.Dtos.Employee;
using Api.Models;
using Microsoft.Data.Sqlite;

namespace Api.Repositories;

/*
    Normally I would have implemented EFCore Code-First or something (for the ease of using Linq) but didn't want to take up too much time setting that up
    Normally I also would have used AutoMapper/Fluent API for validations
*/
public class SQLiteRepository : ISQLiteRepository
{
    private readonly string connectionString;

    public SQLiteRepository(IConfiguration configuration)
    {
        this.connectionString = configuration.GetConnectionString("SQLite");
    }

    public async Task<Employee?> GetEmployeeAsync(int id)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText =
                @"
                    SELECT Id, FirstName, LastName, Salary, DateOfBirth
                    FROM employees
                    WHERE Id = @Id;
                ";
            selectCommand.Parameters.AddWithValue("@Id", id);

            using (var reader = await selectCommand.ExecuteReaderAsync())
            {
                if (reader.Read())
                {
                    Employee employee = new Employee
                    {
                        Id = reader.GetInt32(0),
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        Salary = reader.GetDecimal(3),
                        DateOfBirth = reader.GetDateTime(4),
                    };

                    return employee;
                }
                else return null;
            }
        }
    }

    public async Task<IEnumerable<Employee>> GetEmployeesAsync()
    {
        var employees = new List<Employee>();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText =
                @"
                    SELECT Id, FirstName, LastName, Salary, DateOfBirth
                    FROM employees;
                ";

            using (var reader = await selectCommand.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    Employee employee = new Employee
                    {
                        Id = reader.GetInt32(0),
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        Salary = reader.GetDecimal(3),
                        DateOfBirth = reader.GetDateTime(4),
                    };

                    employees.Add(employee);
                }
            }
        }
        
        return employees;
    }

    public async Task<Dependent?> GetDependentAsync(int id)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText =
                @"
                    SELECT Id, EmployeeId, Relationship, FirstName, LastName, DateOfBirth
                    FROM dependents
                    WHERE Id = @Id;
                ";
            selectCommand.Parameters.AddWithValue("@Id", id);

            using (var reader = await selectCommand.ExecuteReaderAsync())
            {
                if (reader.Read())
                {
                    Dependent dependent = new Dependent
                    {
                        Id = reader.GetInt32(0),
                        EmployeeId = reader.GetInt32(1),
                        Relationship = (Relationship)reader.GetInt32(2),
                        FirstName = reader.GetString(3),
                        LastName = reader.GetString(4),
                        DateOfBirth = reader.GetDateTime(5),
                    };

                    return dependent;
                }
                else return null;
            }
        }
    }

    public async Task<IEnumerable<Dependent>> GetDependentsAsync()
    {
        var dependents = new List<Dependent>();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText =
                @"
                    SELECT Id, EmployeeId, Relationship, FirstName, LastName, DateOfBirth
                    FROM dependents;
                ";

            using (var reader = await selectCommand.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    Dependent dependent = new Dependent
                    {
                        Id = reader.GetInt32(0),
                        EmployeeId = reader.GetInt32(1),
                        Relationship = (Relationship)reader.GetInt32(2),
                        FirstName = reader.GetString(3),
                        LastName = reader.GetString(4),
                        DateOfBirth = reader.GetDateTime(5),
                    };

                    dependents.Add(dependent);
                }
            }
        }

        return dependents;
    }
    
    public async Task<IEnumerable<Dependent>> GetDependentsByEmployeeIdAsync(int employeeId)
    {
        var dependents = new List<Dependent>();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText =
                @"
                    SELECT Id, EmployeeId, Relationship, FirstName, LastName, DateOfBirth
                    FROM dependents
                    WHERE EmployeeId = @Id;
                ";
            selectCommand.Parameters.AddWithValue("@Id", employeeId);

            using (var reader = await selectCommand.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    Dependent dependent = new Dependent
                    {
                        Id = reader.GetInt32(0),
                        EmployeeId = reader.GetInt32(1),
                        Relationship = (Relationship)reader.GetInt32(2),
                        FirstName = reader.GetString(3),
                        LastName = reader.GetString(4),
                        DateOfBirth = reader.GetDateTime(5),
                    };

                    dependents.Add(dependent);
                }
            }
        }

        return dependents;
    }

    public async Task<bool> CreateTablesAsync()
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var createCommand = connection.CreateCommand();
            createCommand.CommandText =
                @"
                CREATE TABLE employees (
                    Id int,
                    FirstName varchar(255),
                    LastName varchar(255),
                    Salary varchar(255),
                    DateOfBirth datetime2
                );

                CREATE TABLE dependents (
                    Id int,
                    EmployeeId int,
                    Relationship int,
                    FirstName varchar(255),
                    LastName varchar(255),
                    DateOfBirth datetime2
                );
            ";

            await createCommand.ExecuteScalarAsync();
            return true;
        }
    }

    // Quick and dirty Add Employee with no duplicate checking (for the sake of time)
    public async Task<bool> AddEmployeeAsync(GetEmployeeDto employee)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            await connection.OpenAsync();
            using (var transaction = connection.BeginTransaction(deferred: true))
            {
                var tasks = new List<Task>();
                await connection.OpenAsync();
                var insertCommand = connection.CreateCommand();
                insertCommand.CommandText =
                @"
                INSERT INTO employees (Id, FirstName, LastName, Salary, DateOfBirth)
                VALUES (@Id, @FirstName, @LastName, @Salary, @DateOfBirth);
                ";

                insertCommand.Parameters.AddWithValue("@Id", employee.Id);
                insertCommand.Parameters.AddWithValue("@FirstName", employee.FirstName);
                insertCommand.Parameters.AddWithValue("@LastName", employee.LastName);
                insertCommand.Parameters.AddWithValue("@Salary", employee.Salary);
                insertCommand.Parameters.AddWithValue("@DateOfBirth", employee.DateOfBirth);

                tasks.Add(insertCommand.ExecuteScalarAsync());

                if (employee.Dependents.Count > 0)
                {
                    // Loop and insert each dependent
                    foreach (var dependent in employee.Dependents)
                    {
                        var insertDependentCommand = connection.CreateCommand();
                        insertDependentCommand.CommandText =
                            @"
                        INSERT INTO dependents (Id, EmployeeId, Relationship, FirstName, LastName, DateOfBirth)
                        VALUES (@Id, @EmployeeId, @Relationship, @FirstName, @LastName, @DateOfBirth);
                    ";

                        insertDependentCommand.Parameters.AddWithValue("@Id", dependent.Id);
                        insertDependentCommand.Parameters.AddWithValue("@EmployeeId", employee.Id);
                        insertDependentCommand.Parameters.AddWithValue(
                            "@Relationship",
                            dependent.Relationship
                        );
                        insertDependentCommand.Parameters.AddWithValue(
                            "@FirstName",
                            dependent.FirstName
                        );
                        insertDependentCommand.Parameters.AddWithValue(
                            "@LastName",
                            dependent.LastName
                        );
                        insertDependentCommand.Parameters.AddWithValue(
                            "@DateOfBirth",
                            dependent.DateOfBirth
                        );

                        tasks.Add(insertDependentCommand.ExecuteScalarAsync());
                    }
                }

                var allTasks = Task.WhenAll(tasks);
                await allTasks;

                await transaction.CommitAsync();

                return allTasks.Status == TaskStatus.RanToCompletion;
            }
        }
    }

}
