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


        public int CreatePatient(string name, string surname, string patronymic,
                                 string phoneNumber, string address,
                                 string email, string anamnesis,
                                 string birthDate, string gender)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                // Используем параметризованный запрос чтобы избежать SQL-инъекций
                using (var cmd = new NpgsqlCommand(
                    "INSERT INTO patients (name, surname, patronymic, phone_number, address, email, anamnesis, birth_date, gender) " +
                    "VALUES (@name, @surname, @patronymic, @phone_number, @address, @email, @anamnesis, @birth_date, @gender) RETURNING patient_id", connection))
                {
                    cmd.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, name ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("surname", NpgsqlDbType.Varchar, surname ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("patronymic", NpgsqlDbType.Varchar, patronymic ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("phone_number", NpgsqlDbType.Varchar, phoneNumber ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("address", NpgsqlDbType.Varchar, address ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("email", NpgsqlDbType.Varchar, string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email);
                    cmd.Parameters.AddWithValue("anamnesis", NpgsqlDbType.Varchar, anamnesis ?? (object)DBNull.Value);

                    // birth_date ожидается как date или timestamp в БД. Если передана пустая строка, сохраняем NULL.
                    if (!string.IsNullOrWhiteSpace(birthDate) && DateTime.TryParse(birthDate, out var bd))
                        cmd.Parameters.AddWithValue("birth_date", NpgsqlDbType.Timestamp, bd);
                    else
                        cmd.Parameters.AddWithValue("birth_date", NpgsqlDbType.Timestamp, DBNull.Value);

                    cmd.Parameters.AddWithValue("gender", NpgsqlDbType.Varchar, string.IsNullOrWhiteSpace(gender) ? (object)DBNull.Value : gender);

                    LogQuery(cmd, nameof(CreatePatient));

                    var createdPatientId = cmd.ExecuteScalar();
                    if (createdPatientId == null || createdPatientId == DBNull.Value)
                        throw new Exception("Неверно введены данные при создании пациента.");

                    return Convert.ToInt32(createdPatientId);
                }
            }
        }

        public int CreateCall(int patientId, string appealPurpose, string complaints,
                              string? priority = "Неотложный", string? status = "В работе")
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var cmd = new NpgsqlCommand(
                    "INSERT INTO calls (patient_id, appeal_purpose, priority, status, complaints) " +
                    "VALUES (@patient_id, @appeal_purpose, @priority, @status, @complaints) RETURNING call_id", connection))
                {
                    cmd.Parameters.AddWithValue("patient_id", NpgsqlDbType.Integer, patientId);
                    cmd.Parameters.AddWithValue("appeal_purpose", NpgsqlDbType.Varchar,
                        string.IsNullOrWhiteSpace(appealPurpose) ? (object)DBNull.Value : appealPurpose);
                    cmd.Parameters.AddWithValue("priority", NpgsqlDbType.Varchar,
                        string.IsNullOrWhiteSpace(priority) ? (object)DBNull.Value : priority);
                    cmd.Parameters.AddWithValue("status", NpgsqlDbType.Varchar,
                        string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status);
                    cmd.Parameters.AddWithValue("complaints", NpgsqlDbType.Text,
                        string.IsNullOrWhiteSpace(complaints) ? (object)DBNull.Value : complaints);

                    LogQuery(cmd, nameof(CreateCall));

                    var createdCallId = cmd.ExecuteScalar();
                    if (createdCallId == null || createdCallId == DBNull.Value)
                        throw new Exception("Не удалось создать вызов.");

                    return Convert.ToInt32(createdCallId);
                }
            }
        }


        public string[,] GetAllPatient(string _name, string _surname, string _patronymic,
                                 string _phoneNumber, string _address, string _email,
                                 List<string> _appealPurposes, List<string> _priorities, string _id,
                                 List<string> _status, string _gender, DateTime? _dateStart, DateTime? _dateEnd)
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
                    (p.address ILIKE @address OR @address IS NULL) AND
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
                WHERE 
                    (p.name = @name OR @name IS NULL) AND
                    (p.surname = @surname OR @surname IS NULL) AND
                    (p.patronymic = @patronymic OR @patronymic IS NULL) AND
                    (p.phone_number LIKE @phone_number OR @phone_number IS NULL) AND
                    (p.address LIKE @address OR @address IS NULL) AND
                    (p.email = @email OR @email IS NULL) AND
                    (@appeal_purposes IS NULL OR c.appeal_purpose = ANY(@appeal_purposes)) AND
                    (@priorities IS NULL OR c.priority = ANY(@priorities)) AND
                    (c.call_id = @call_id OR @call_id IS NULL) AND
                    (@status IS NULL OR c.status = ANY(@status)) AND
                    (p.gender = @gender OR @gender IS NULL) AND
                    (@date_start IS NULL OR c.time >= @date_start) AND
                    (@date_end IS NULL OR c.time <= @date_end)
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
                    requestDB.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, string.IsNullOrWhiteSpace(_name) ? (object)DBNull.Value : _name);
                    requestDB.Parameters.AddWithValue("surname", NpgsqlDbType.Varchar, string.IsNullOrWhiteSpace(_surname) ? (object)DBNull.Value : _surname);
                    requestDB.Parameters.AddWithValue("patronymic", NpgsqlDbType.Varchar, string.IsNullOrWhiteSpace(_patronymic) ? (object)DBNull.Value : _patronymic);
                    requestDB.Parameters.AddWithValue("phone_number", NpgsqlDbType.Varchar, string.IsNullOrWhiteSpace(_phoneNumber) ? (object)DBNull.Value : _phoneNumber);
                    if (string.IsNullOrWhiteSpace(_address))
                        requestDB.Parameters.AddWithValue("address", NpgsqlDbType.Varchar, DBNull.Value);
                    else
                        requestDB.Parameters.AddWithValue("address", NpgsqlDbType.Varchar, $"%{_address}%");
                    if (string.IsNullOrWhiteSpace(_email))
                        requestDB.Parameters.AddWithValue("email", NpgsqlDbType.Varchar, DBNull.Value);
                    else
                        requestDB.Parameters.AddWithValue("email", NpgsqlDbType.Varchar, $"{_address}%");
                    if (_appealPurposes == null || _appealPurposes.Count == 0)
                        requestDB.Parameters.AddWithValue("appeal_purposes", NpgsqlDbType.Array | NpgsqlDbType.Varchar, DBNull.Value);
                    else
                        requestDB.Parameters.AddWithValue("appeal_purposes", NpgsqlDbType.Array | NpgsqlDbType.Varchar, _appealPurposes.ToArray());

                    // priorities: pass array or NULL
                    if (_priorities == null || _priorities.Count == 0)
                        requestDB.Parameters.AddWithValue("priorities", NpgsqlDbType.Array | NpgsqlDbType.Varchar, DBNull.Value);
                    else
                        requestDB.Parameters.AddWithValue("priorities", NpgsqlDbType.Array | NpgsqlDbType.Varchar, _priorities.ToArray());

                    // gender filter
                    requestDB.Parameters.AddWithValue("gender", NpgsqlDbType.Varchar, string.IsNullOrWhiteSpace(_gender) ? (object)DBNull.Value : _gender);

                    // call id filter (expects numeric id or null)
                    if (string.IsNullOrWhiteSpace(_id) || !int.TryParse(_id, out var callId))
                        requestDB.Parameters.AddWithValue("call_id", NpgsqlDbType.Integer, DBNull.Value);
                    else
                        requestDB.Parameters.AddWithValue("call_id", NpgsqlDbType.Integer, callId);

                    // status list: pass array or NULL
                    if (_status == null || _status.Count == 0)
                        requestDB.Parameters.AddWithValue("status", NpgsqlDbType.Array | NpgsqlDbType.Varchar, DBNull.Value);
                    else
                        requestDB.Parameters.AddWithValue("status", NpgsqlDbType.Array | NpgsqlDbType.Varchar, _status.ToArray());

                    // date range
                    if (_dateStart.HasValue)
                        requestDB.Parameters.AddWithValue("date_start", NpgsqlDbType.Timestamp, _dateStart.Value);
                    else
                        requestDB.Parameters.AddWithValue("date_start", NpgsqlDbType.Timestamp, DBNull.Value);

                    if (_dateEnd.HasValue)
                        requestDB.Parameters.AddWithValue("date_end", NpgsqlDbType.Timestamp, _dateEnd.Value);
                    else
                        requestDB.Parameters.AddWithValue("date_end", NpgsqlDbType.Timestamp, DBNull.Value);

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