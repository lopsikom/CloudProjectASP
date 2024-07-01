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

namespace CloudProject.SQLClass
{
    public class SQLCLassConnection
    {
        private string ConnectString = "Host=localhost;Database=DataBaseC#;Username=lopsik;Password=lopsik123;";
        private void SqlConnectToDB()
        {
            using(var conn = new NpgsqlConnection(ConnectString)) 
            {
                conn.Open();
                Console.WriteLine("Соединение установленно");
                Console.WriteLine($"{conn.Database}");
                using (var command = new NpgsqlCommand("SELECT * FROM \"Users\"", conn))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine(reader.GetString(2));
                        }
                    }
                }
            }
        }
        public async Task RegistartionUser(HttpRequest request, HttpResponse responce)
        {
            var user = await request.ReadFromJsonAsync<UserClass>();
            var login = user.Name;
            var password = HashPassword(user.Password);
            using(var conn = new NpgsqlConnection(ConnectString))
            {
                conn.Open();
                using(var command = new NpgsqlCommand("INSERT INTO \"Users\" (UserName, Password, Root) VALUES (@UserName, @Password, @Root)", conn))
                {
                    command.Parameters.AddWithValue("UserName", login);
                    command.Parameters.AddWithValue("Password", password);
                    command.Parameters.AddWithValue("Root", false);
                    command.ExecuteNonQuery();
                }
            }
            responce.StatusCode = 210;
            await responce.WriteAsync("Пользователь создан");
            Console.WriteLine($"Пользователь создан под именем: {login}");
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
        public async Task Authorization(HttpRequest request, HttpResponse responce)
        {
            var user = await request.ReadFromJsonAsync<UserClass>();
            var login = user.Name;
            var password = HashPassword(user.Password);
            using(var conn = new NpgsqlConnection(ConnectString))
            {
                conn.Open();
                using(var command = new NpgsqlCommand("SELECT COUNT(*) FROM \"Users\" WHERE username = @username", conn))
                {
                    command.Parameters.AddWithValue("@username", login);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    if (count > 0)
                    {
                        using (var command2 = new NpgsqlCommand("SELECT COUNT(*) FROM \"Users\" WHERE username = @username AND password = @password", conn))
                        {
                            command2.Parameters.AddWithValue("@username", login);
                            command2.Parameters.AddWithValue("@password", password);
                            int count2 = Convert.ToInt32(command2.ExecuteScalar());
                            if (count2 > 0)
                            {
                                responce.StatusCode = 210;
                                await responce.WriteAsync("Успешный вход");
                                Console.WriteLine($"Пользователь успешно зашёл, его логин: {login}");
                            }
                            else
                            {
                                responce.StatusCode = 212;
                                await responce.WriteAsync("Неверный пароль");
                                Console.WriteLine($"Пользователь неудачная попытка входа пользователя, его логин: {login}");
                            }
                        }
                    }
                    else
                    {
                        responce.StatusCode = 215;
                        await responce.WriteAsync("Пользователь не найден");
                        Console.WriteLine("Пользователь не найден, не удачная попытка войти");
                    }
                }
            }
        }
    }
}
