using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using System.Security.Cryptography;
using System.Runtime.Intrinsics.Arm;
using System.Text.RegularExpressions;
using CloudProjectASP.HttpDataClasses;
using System.Diagnostics.Metrics;

namespace CloudProject.SQLClass
{
    public class SQLCLassConnection
    {
        private string ConnectString = "Host=localhost;Database=DataBaseC#;Username=lopsik;Password=lopsik123;";
        //private void SqlConnectToDB()
        //{
        //    using (var conn = new NpgsqlConnection(ConnectString))
        //    {
        //        conn.Open();
        //        Console.WriteLine("Соединение установленно");
        //        Console.WriteLine($"{conn.Database}");
        //        using (var command = new NpgsqlCommand("SELECT * FROM \"Users\"", conn))
        //        {
        //            using (var reader = command.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    Console.WriteLine(reader.GetString(2));
        //                }
        //            }
        //        }
        //    }
        //}
        public async Task RegistartionUser(HttpRequest request, HttpResponse responce)
        {
            var user = await request.ReadFromJsonAsync<UserClass>();
            var login = user.Name;
            var password = HashPassword(user.Password);
            int userID;
            using (var conn = new NpgsqlConnection(ConnectString))
            {
                conn.Open();
                using(var command = new NpgsqlCommand("SELECT COUNT(*) FROM \"Users\" WHERE username = @username", conn))
                {
                    command.Parameters.AddWithValue("username", login);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    if (count > 0)
                    {
                        responce.StatusCode = StatusCodes.Status409Conflict;
                        await responce.WriteAsync("Такой пользователь уже есть");
                        Console.WriteLine($"Попытка создать существующего пользователя: {login}");
                        return;
                    }
                }
                using(var command = new NpgsqlCommand("INSERT INTO \"Users\" (UserName, Password, Root) VALUES (@UserName, @Password, @Root)", conn))
                {
                    command.Parameters.AddWithValue("UserName", login);
                    command.Parameters.AddWithValue("Password", password);
                    command.Parameters.AddWithValue("Root", false);
                    command.ExecuteNonQuery();
                }
            }
            string hash = AddHashCodeInDatabase(login, true);
            responce.StatusCode = StatusCodes.Status200OK;
            await responce.WriteAsJsonAsync(new {message = "Пользователь создан", Hash = hash});
            Console.WriteLine($"Пользователь создан под именем: {login}");
        }
        public async Task Authorization(HttpRequest request, HttpResponse responce)
        {
            var user = await request.ReadFromJsonAsync<UserClass>();
            var login = user.Name;
            var password = HashPassword(user.Password);
            using(var conn = new NpgsqlConnection(ConnectString))
            {
                conn.Open();
                using (var command2 = new NpgsqlCommand("SELECT password,root FROM \"Users\" WHERE username = @username", conn))
                {
                    command2.Parameters.AddWithValue("@username", login);

                    using (var result = command2.ExecuteReader())
                    {
                        if (result.Read()) // Проверка наличия строки
                        {
                            string hashuser = result["password"].ToString();
                            string root = result["root"].ToString();
                            // Сравнение паролей
                            if (hashuser.Equals(password)) // Предположим, что password уже является хешем
                            {
                                string hash = AddHashCodeInDatabase(login, false);
                                responce.StatusCode = StatusCodes.Status200OK; // Стандартный код успешного ответа
                                await responce.WriteAsJsonAsync(new { message = "Успешный вход", Hash = hash,Root = root });
                                Console.WriteLine($"Пользователь успешно зашёл, его логин: {login}");
                            }
                            else
                            {
                                responce.StatusCode = StatusCodes.Status401Unauthorized; // Стандартный код для неправильного пароля
                                await responce.WriteAsync("Неверный пароль");
                                Console.WriteLine($"Пользователь неудачная попытка входа пользователя, его логин: {login}");
                            }
                        }
                        else
                        {
                            // Если пользователь не найден
                            responce.StatusCode = StatusCodes.Status404NotFound; // Стандартный код для не найденного ресурса
                            await responce.WriteAsync("Пользователь не найден");
                            Console.WriteLine($"Пользователь не найден, его логин: {login}");
                        }
                    }
                }
            }
        }
        public async Task GetUsersList(HttpRequest request, HttpResponse responce)
        {
            request.Headers.TryGetValue("HashCode", out var hash);
            if(CheckHashInDataBase(hash) == null)
            {
                responce.StatusCode = StatusCodes.Status404NotFound;
                await responce.WriteAsync("Ошибка при проверки хеш-кода");
                Console.WriteLine("Ошибка при проверки хеш-кода");
            }
            else
            {
                using (var conn = new NpgsqlConnection(ConnectString))
                {
                    var userDataList = new List<UserClass>();
                    conn.Open();
                    using(var command = new NpgsqlCommand("SELECT username, root FROM \"Users\"", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    var userData = new UserClass
                                    {
                                        Name = reader["username"].ToString(),
                                        Root = reader["root"].ToString()
                                    };
                                    userDataList.Add(userData);
                                }
                                responce.StatusCode = StatusCodes.Status200OK;
                                await responce.WriteAsJsonAsync(new {message = "Получение данных о пользователях", UsersList = userDataList });
                                Console.WriteLine("Получение данных о пользователях");
                            }
                        }
                    }
                }
            }
        }
        private string AddHashCodeInDatabase(string login, bool isRegistration)
        {
            string query = isRegistration
                ? "INSERT INTO \"UserHash\" (Username, HashCode) VALUES (@Username, @HashCode)"
                : "UPDATE \"UserHash\" SET HashCode = @HashCode WHERE Username = @Username";

            using (var conn = new NpgsqlConnection(ConnectString))
            {
                conn.Open();
                using (var command = new NpgsqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@Username", login);
                    string hash = RandomHash();
                    command.Parameters.AddWithValue("@HashCode", hash);
                    command.ExecuteNonQuery();
                    return hash;
                }
            }
        }
        public virtual string CheckHashInDataBase(string hash)
        {
            using (var conn = new NpgsqlConnection(ConnectString))
            {
                conn.Open();
                using (var command = new NpgsqlCommand("SELECT username FROM \"UserHash\" WHERE hashcode = @hashcode", conn))
                {
                    command.Parameters.AddWithValue("@hashcode", hash);
                    using (var result = command.ExecuteReader())
                    {
                        if (result.Read())
                        {
                            return result.GetString(0); 
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }
        private string HashPassword(string pass)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(pass);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
        private string RandomHash()
        {
            byte[] randomData = GenerateRandomByteData(32);
            return HashComplete(randomData);

        }
        private byte[] GenerateRandomByteData(int lenght)
        {
            byte[] data = new byte[lenght];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(data);
            }

            return data;
        }
        private string HashComplete(byte[] ByteData)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(ByteData);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
