using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using Tmds.DBus.Protocol;
using static System.Net.Mime.MediaTypeNames;


namespace Ambulance
{
    public class DatabaseService
    {
        /*[ModuleInitializer]
        public static void Initialize()
        {
            
        }*/
        private readonly string _connectionString;
        private readonly bool _enableLogging = true;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
        }


        // Метод для логирования запроса
        private void LogQuery(NpgsqlCommand cmd, string methodName)
        {
            if (!_enableLogging) return;
            var sb = new StringBuilder();
            sb.AppendLine(new string('=', 100));
            sb.AppendLine($"[{DateTime.Now:HH:mm:ss.fff}] Метод: {methodName}");
            sb.AppendLine("SQL ЗАПРОС:");
            sb.AppendLine(cmd.CommandText);
            sb.AppendLine();
            sb.AppendLine("ПАРАМЕТРЫ:");

            foreach (NpgsqlParameter p in cmd.Parameters)
            {
                string valueStr = p.Value == null || p.Value == DBNull.Value
                    ? "NULL"
                    : $"\"{p.Value}\"";

                sb.AppendLine($"  • @{p.ParameterName} = {valueStr} [{p.NpgsqlDbType}]");
            }

            sb.AppendLine(new string('=', 100));

            // Вывод в консоль
            Console.WriteLine(sb.ToString());

            // Сохранение в файл (опционально)


            File.AppendAllText("C:\\Users\\OSM\\OneDrive\\Desktop\\sql_debug.log", sb.ToString() + Environment.NewLine);
        }


        public void CreatePatient(string name, string surname, string patronymic,
                                 string phoneNumber, string address,
                                 string email, string anamnesis,
                                 string complaints, string status)
        {
            int[] info = new int[2];
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();


                using (var create = new NpgsqlCommand($"INSERT INTO patients (name, surname, patronymic, phone_number, address, email, anamnesis)" +
                    $" VALUES ('{name}', '{surname}', '{patronymic}', '{phoneNumber}', '{address}', '{email}', '{anamnesis}')", connection))
                    if (create.ExecuteNonQuery() < 0)
                        throw new Exception("Неверно введены данные.");
            }
        }

        public void DeleteClient(int id, int key)
        {
            int[] info = new int[2];
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var create = new NpgsqlCommand($"DELETE FROM clients WHERE id_client={id} AND delete_key={key}", connection))
                    if (create.ExecuteNonQuery() < 0)
                        throw new Exception("Неверно введены данные.");
            }
        }

        public string[,] GetAllPatient(string _name, string _surname, string _patronymic,
                                 string _phoneNumber, string _address, string _email,
                                 string _appealPurpose, string _priority, string _id,
                                 List<string> _status, DateTime? _dateStart, DateTime? _dateEnd)
        {
            /*string request = @"
                SELECT DISTINCT
                    p.patient_id, p.name, p.surname, p.patronymic, p.phone_number, p.address, p.email,
                    p.anamnesis, p.complaints,
                    c.appeal_purpose, c.priority, c.call_id, c.time, c.status
                FROM patients AS p
                INNER JOIN calls AS c ON p.patient_id = c.patient_id
                WHERE 
                    (p.name = @name OR @name IS NULL) AND
                    (p.surname = @surname OR @surname IS NULL) AND
                    (p.patronymic = @patronymic OR @patronymic IS NULL) AND
                    (p.phone_number = @phone_number OR @phone_number IS NULL) AND
                    (p.address = @address OR @address IS NULL) AND
                    (p.email = @email OR @email IS NULL) AND
                    (c.appeal_purpose = @appeal_purpose OR @appeal_purpose IS NULL) AND
                    (c.priority = @priority OR @priority IS NULL)
                    ORDER BY p.patient_id DESC;";*/
            string request = @"
                SELECT DISTINCT
                    p.patient_id, p.name, p.surname, p.patronymic, p.phone_number, p.address, p.email, p.anamnesis, 
                    c.complaints, c.appeal_purpose, c.priority, c.call_id, c.time, c.status, p.birth_date, p.gender
                FROM patients AS p
                INNER JOIN calls AS c ON p.patient_id = c.patient_id
                ORDER BY p.patient_id DESC;";
            //string[] requestPartsNames = { "name", "surname", "patronymic", "phoneNumber", "address", "email" };
            //string[] requestParts = { $"{_name}", $"{_surname}", $"{_patronymic}", $"{_phoneNumber}", $"{_address}", $"{_email}" };
            string[,] patients = new string[50, 16];
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                /*int counterWHERE = 0;
                for (int i = 0; i < requestParts.Length; i++)
                {
                    if (!string.IsNullOrEmpty(requestParts[i]))
                    {
                        if (counterWHERE == 0)
                        {
                            counterWHERE++;
                            request += "WHERE p." + requestPartsNames[i] + $" = '{requestParts[i]}' ";
                        }
                        else
                            request += "WHERE p." + requestPartsNames[i] + $" = '{requestParts[i]}' ";
                    }
                }

                request += "INNER JOIN calls AS c ON p.patient_id = c.patient_id ";
                if (!string.IsNullOrEmpty(_appealPurpose))
                {
                    request += "AND c.appeal_purpose = " + $"'{_appealPurpose}' ";
                }
                if (!string.IsNullOrEmpty(_priority))
                {
                    request += "AND c.priority = " + $"'{_priority}' ";
                }
                request += ';';*/

                using (var requestDB = new NpgsqlCommand(request, connection))
                {
                    requestDB.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, _name ?? (object)DBNull.Value);
                    requestDB.Parameters.AddWithValue("surname", NpgsqlDbType.Varchar, _surname ?? (object)DBNull.Value);
                    requestDB.Parameters.AddWithValue("patronymic", NpgsqlDbType.Varchar, _patronymic ?? (object)DBNull.Value);
                    requestDB.Parameters.AddWithValue("phone_number", NpgsqlDbType.Varchar, "" ?? (object)DBNull.Value);
                    requestDB.Parameters.AddWithValue("address", NpgsqlDbType.Varchar, _address ?? (object)DBNull.Value);
                    requestDB.Parameters.AddWithValue("email", NpgsqlDbType.Varchar, _email ?? (object)DBNull.Value);
                    requestDB.Parameters.AddWithValue("appeal_purpose", NpgsqlDbType.Varchar, _appealPurpose ?? (object)DBNull.Value);
                    requestDB.Parameters.AddWithValue("priority", NpgsqlDbType.Varchar, _priority ?? (object)DBNull.Value);

                    LogQuery(requestDB, nameof(GetAllPatient));
                    using (var patientsDB = requestDB.ExecuteReader())
                    {
                        int countRows = 0;
                        while (patientsDB.Read())
                        {
                            /*for (int  i = 0; i <  patientsDB.FieldCount; i++) 
                                patients[countRows, i] = patientsDB.GetString(i);*/
                            patients[countRows, 0] = patientsDB.GetInt32(0).ToString();                     //patientId
                            patients[countRows, 1] = patientsDB.GetString(1);                               //name
                            patients[countRows, 2] = patientsDB.GetString(2);                               //surname
                            patients[countRows, 3] = patientsDB.GetString(3);                               //patronymic
                            patients[countRows, 4] = patientsDB.GetString(4);                               //phoneNumber
                            patients[countRows, 5] = patientsDB.GetString(5);                               //address
                            patients[countRows, 6] = patientsDB.GetString(6);                               //email
                            patients[countRows, 7] = patientsDB.GetString(7);                               //anamnesis
                            patients[countRows, 8] = patientsDB.GetString(8);                               //complaints
                            patients[countRows, 9] = patientsDB.GetString(9);                               //appealPurpose
                            patients[countRows, 10] = patientsDB.GetString(10);                             //priority
                            patients[countRows, 11] = patientsDB.GetInt32(11).ToString();                   //callId
                            patients[countRows, 12] = patientsDB.GetDateTime(12).ToString();                //time
                            patients[countRows, 13] = patientsDB.GetString(13);                             //status
                            patients[countRows, 14] = patientsDB.GetDateTime(14).ToString("dd.MM.yyyy");    //birth_date
                            patients[countRows, 15] = patientsDB.GetString(15);                             //gender 
                            countRows++;
                        }
                    }
                }
            }
            return patients;
        }
    }
}