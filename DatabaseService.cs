using Npgsql;
using System;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;


namespace Ambulance
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
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

        public string[] GetAllPatient(string name, string surname, string patronymic,
                                 string phoneNumber, string address,
                                 string email, string appealPurpose, string priority)
        {
            string request = $"SELECT  p.patient_id, p.name, p.surname, p.patronymic, p.phone_number, p.address, p.email, " +
                $"p.anamnesis, p.complaints, c.appeal_purpose, c.priority FROM patients AS p ";
            string[] requestParts = { "name", "surname", "patronymic", "phoneNumber", "address", "email" };
            string[] strings = new string[50];
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                int counterWHERE = 0;
                foreach (string part in requestParts)
                {
                    if (!string.IsNullOrEmpty(part))
                    {
                        if (counterWHERE == 0)
                        {
                            counterWHERE++;
                            request += "WHERE p." + part + $" '{part}' ";
                        }
                        else
                            request += "AND p." + part + $" '{part}' ";
                    }
                }

                request += "INNER JOIN calls AS c ON p.patient_id = c.patient_id ";
                if (!string.IsNullOrEmpty(appealPurpose) || !string.IsNullOrEmpty(priority))
                {
                    if (!string.IsNullOrEmpty(appealPurpose))
                    {
                        request += "AND c.appeal_purpose " + $"'{appealPurpose}' ";
                    }
                    if (!string.IsNullOrEmpty(priority))
                    {
                        request += "AND c.priority " + $"'{priority}' ";
                    }
                }
                request += ';';

                using (var coordsDB = new NpgsqlCommand(request, connection))
                using (var readerCoords = coordsDB.ExecuteReader())
                {

                    readerCoords.Read();
                    name = readerCoords.GetDouble(0).ToString() + " " + readerCoords.GetDouble(1).ToString();
                }
            }
            return strings;
        }
    }
}
