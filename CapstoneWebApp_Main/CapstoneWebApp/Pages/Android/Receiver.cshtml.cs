using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Metrics;
using System.Reflection.PortableExecutable;
using System.Security.Principal;
using System.Web.Http;
using static CapstoneWebApp.Pages.Clients.AdminModel;

namespace CapstoneWebApp.Pages.Android
{
    public class ReceiverModel : PageModel
    {
        // To consider for later: possible separating the student and event classes into their own location since they seem to get used a decent amount
        public class Student
        {
            public int user_id { get; private set; } = 0;
            public String first_name { get; private set; } = String.Empty;
            public String last_name { get; private set; } = String.Empty;
            public String eNumber { get; private set; } = String.Empty;
            public String password { get; private set; } = String.Empty;

            public Student(int uid, String fn, String ln, String en, String ps)
            {
                user_id = uid;
                first_name = fn;
                last_name = ln;
                eNumber = en;
                password = ps;
            }

        }

        public int current_value { get; set; } = 0;
        public List<Student> students = new List<Student>();
        public string result { get; private set; } = "";

        // tries logging in, this just posts the json the actual logging in is handled on the mobile side
        public void tryLogin(SqlConnection connection)
        {
            String sql = "SELECT * FROM [User]";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // bad names but user_id, firstname, last name, eNumber, and password
                        int uid = reader.GetInt32(0);
                        String fn = reader.GetString(1);
                        String ln = reader.GetString(2);
                        String en = reader.GetString(3);
                        String ps = reader.GetString(4);

                        // to display the json easier
                        students.Add(new Student(uid, fn, ln, en, ps));
                    }
                }
            }

            result = JsonConvert.SerializeObject(new
            {
                Students = students
            }, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented });

        }

        // tries to check-in to an event
        public void tryCheckIn(SqlConnection connection, int uid, int eid)
        {
            String sql = "SELECT * FROM [CheckIn]";
            using (SqlCommand selectCmd = new SqlCommand(sql, connection))
            {
                using (SqlDataReader reader = selectCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int check_in_uid = reader.GetInt32(3);
                        int check_in_eid = reader.GetInt32(4);

                        // this just checks to see if the user has already checked in and if they have then it leaves
                        if (uid == check_in_uid && eid == check_in_eid) return;
                    }
                }
            }
            DateTime date = DateTime.Now;
            DateTime time = DateTime.Now;
            
            String insertQuery = "INSERT INTO [CheckIn] VALUES ('"  + date.ToShortDateString() + "','" + time.ToShortTimeString() + "','" + uid + "','" + eid + "')";

            SqlCommand insertCmd = new SqlCommand(insertQuery, connection);
            insertCmd.ExecuteNonQuery();
            

        }

        public void OnGet(int id, int cid = 0, DateTime date = new DateTime(), DateTime time = new DateTime(), int uid = 0, int eid = 0)
        {
            current_value = id;
            try
            {
                // database connection string
                String connectionString = "DATABASE_CONNECTION";
                SqlConnection connection = new SqlConnection(connectionString);
                using (connection)
                {
                    // open database connection
                    connection.Open();
                    switch (id)
                    {
                        // depending on the id we try to login or try to checkin, couldn't get the webapi library to work so this is the workaround for now
                        case 1: tryLogin(connection); break;
                        case 2: tryCheckIn(connection, uid, eid); break;
                        default: break;

                    }

                    // close database connection
                    connection.Close();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}