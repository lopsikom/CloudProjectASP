﻿using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using System.Security.Cryptography;
using System.Runtime.Intrinsics.Arm;

namespace CloudProject.SQLClass
{
    public class SQLCLassConnection
    {
        private string ConnectString = "Host=localhost;Database=DataBaseC#;Username=lopsik;Password=lopsik123;";
        public void SqlConnectToDB()
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
        public void RegistartionUser()
        {
            Console.WriteLine("Введите ваш логин");
            string login = Console.ReadLine();
            Console.WriteLine("Введите ваш пароль");
            string password = Console.ReadLine();
            password = HashPassword(password);
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
        public void Authorization()
        {
            Console.WriteLine("Введите ваш логин");
            string login = Console.ReadLine();
            Console.WriteLine("Введите ваш пароль");
            string password = Console.ReadLine();
            password = HashPassword(password);
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
                                Console.WriteLine("Пароль верный \n Вы вошли!");
                            }
                            else
                            {
                                Console.WriteLine("Пароль неверный!");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Пользователь не найден");
                    }
                }
            }
        }
    }
}
