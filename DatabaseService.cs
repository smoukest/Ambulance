using Npgsql;
using NpgsqlTypes;
using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using static System.Net.Mime.MediaTypeNames;


namespace Ambulance
{
    public class DatabaseService
    {
        [ModuleInitializer]
        public static void Initialize()
        {
        }
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
        }

        public string[,] ConnectAndQuery()
        {
            string[,] names = new string[21, 2];
            int i = 0;
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                //Выполнение запроса
                using (var stationsDB = new NpgsqlCommand("select ST_X(coordinates), ST_Y(coordinates), id_line, id_station, name FROM stations ORDER BY id_line, id_station", connection))
                using (var linesDB = new NpgsqlCommand("select id_line, hex_color, name FROM lines ORDER BY id_line", connection))
                {
                    using (var readerLines = linesDB.ExecuteReader())
                    {
                        while (readerLines.Read())
                        {/*
                            StartLineComboBox.Items.Add(readerLines.GetString(2));
                            EndLineComboBox.Items.Add(readerLines.GetString(2));
                            int idLine = readerLines.GetInt32(0);
                            string color = readerLines.GetString(1);
                            Chart.Series.Add($"{idLine}");
                            Chart.Series[$"{idLine}"].Color = ColorTranslator.FromHtml($"#{color}");
                            Chart.Series[$"{idLine}"].ChartType = SeriesChartType.Line;
                            names[i, 0] = readerLines.GetString(2);
                            names[i++, 1] = readerLines.GetInt32(0).ToString();*/
                        }
                    }
                    using (var readerStations = stationsDB.ExecuteReader())
                    {
                        int prevIdLine = -1;
                        double firstX = 0;
                        double firstY = 0;
                        while (readerStations.Read())
                        {
                            // Обработка данных
                            double x = readerStations.GetDouble(0);
                            double y = readerStations.GetDouble(1);
                            int idLine = readerStations.GetInt32(2);
                            int idStation = readerStations.GetInt32(3);
                            string name = readerStations.GetString(4);
                            string node = $"{idLine}_Node";
                            /*if (0 > Chart.Series.IndexOf(node))
                            {
                                if (prevIdLine == 5 || prevIdLine == 11 || prevIdLine == 95)
                                {
                                    Chart.Series[$"{prevIdLine}"].Points.Add(new DataPoint(firstX, firstY));
                                }
                                firstX = x;
                                firstY = y;
                                prevIdLine = idLine;
                                Chart.Series.Add(node);
                                Chart.Series[node].ChartType = SeriesChartType.Point;
                                Chart.Series[node].Color = System.Drawing.Color.Black;
                            }
                            Chart.Series[node].Points.Add(new DataPoint(x, y));                                     //Создание точки станции
                            Chart.Series[$"{idLine}"].Points.Add(new DataPoint(x, y)                                //Создание линии от станции к станции на ветке
                            {
                                Label = name,
                                LabelForeColor = System.Drawing.Color.Black
                            });
                            Chart.Series[node].SmartLabelStyle.Enabled = true;
                            Chart.Series[node].SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.Yes;*/

                            //MessageBox.Show($@"""{color}""");
                        }
                    }
                }
                using (var stationsDB = new NpgsqlCommand("select distinct(name) FROM stations ORDER BY name", connection))
                using (var readerStations = stationsDB.ExecuteReader())
                {
                    while (readerStations.Read())
                    {
                        
                    }
                }
                    connection.Close();
            }
            return names;
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


                using (var create = new NpgsqlCommand($"INSERT INTO patients (name, surname, patronymic, phone_number, address, email, anamnesis, complaints)" +
                    $" VALUES ('{name}', '{surname}', '{patronymic}', '{phoneNumber}', '{address}', '{email}', '{anamnesis}', '{complaints}')" , connection))
                    if (create.ExecuteNonQuery() < 0)
                        throw new Exception("Неверно введены данные.");
                    else
                    {
                        /*using (var length = new NpgsqlCommand($"SELECT ST_DistanceSphere(" +
                            $"(SELECT coordinates FROM stations WHERE id_line = {idLineNew} AND id_station = {idStationNew})::geometry," +
                            $"(SELECT coordinates FROM stations WHERE id_line = {idLineOld} AND id_station = {idStationOld})::geometry" +
                            ") AS distance_in_meters;", connection))
                        using (var keyDB = new NpgsqlCommand($"select id_client, delete_key FROM clients WHERE passport_series={}" +
                            $" AND passport_number={passportNumber} ORDER BY id_client DESC", connection))
                        {
                            /*using (var readerLength = length.ExecuteReader())
                            {
                                readerLength.Read();
                                info[2] = Convert.ToInt32(readerLength.GetDouble(0) / 11.38888 * 1.2 + 7);
                            }
                            using (var readerLines = keyDB.ExecuteReader())
                            {

                                readerLines.Read();
                                info[0] = readerLines.GetInt32(0);
                                info[1] = readerLines.GetInt32(1);
                                return info;
                            }
                        }*/
                    }
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
                                 string _phoneNumber, string _address,
                                 string _email, string _appealPurpose, string _priority)
        {
            string request = @"
                SELECT 
                    p.patient_id, p.name, p.surname, p.patronymic, p.phone_number, p.address, p.email,
                    p.anamnesis, p.complaints, 
                    c.appeal_purpose, c.priority, c.call_id, c.time
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
                    (c.priority = @priority OR @priority IS NULL);";
            //string[] requestPartsNames = { "name", "surname", "patronymic", "phoneNumber", "address", "email" };
            //string[] requestParts = { $"{_name}", $"{_surname}", $"{_patronymic}", $"{_phoneNumber}", $"{_address}", $"{_email}" };
            string[,] strings = new string[50,11];
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
                    requestDB.Parameters.Add("name", NpgsqlDbType.Varchar).Value = _name;
                    requestDB.Parameters.Add("surname", NpgsqlDbType.Varchar).Value = _surname;
                    requestDB.Parameters.Add("patronymic", NpgsqlDbType.Varchar).Value = _patronymic;
                    requestDB.Parameters.Add("phone_number", NpgsqlDbType.Varchar).Value = _phoneNumber;
                    requestDB.Parameters.Add("address", NpgsqlDbType.Varchar).Value = _address;
                    requestDB.Parameters.Add("email", NpgsqlDbType.Varchar).Value = _email;
                    requestDB.Parameters.Add("appeal_purpose", NpgsqlDbType.Varchar).Value = _appealPurpose;
                    requestDB.Parameters.Add("priority", NpgsqlDbType.Varchar).Value = _priority;
                    using (var patients = requestDB.ExecuteReader())
                    {
                        int countRows = 0;
                        while (patients.Read())
                        {
                            /*for (int  i = 0; i <  patients.FieldCount; i++) 
                                strings[countRows, i] = patients.GetString(i);*/
                            string patinentId = patients.GetInt32(0).ToString();
                            string name = patients.GetString(1);
                            string surname = patients.GetString(2);
                            string patronymic = patients.GetString(3);
                            string phoneNumber = patients.GetString(4);
                            string address = patients.GetString(5);
                            string email = patients.GetString(6);
                            string anamnesis = patients.GetString(7);
                            string complaints = patients.GetString(8);
                            string appeaPurpose = patients.GetString(9);
                            string priority = patients.GetString(10);
                            string callId = patients.GetInt32(11).ToString();
                            DateTime timestamp = patients.GetDateTime(12);
                            string time = timestamp.ToString();

                        }
                    }
                }
            }
            return strings;
        }
    }
}
