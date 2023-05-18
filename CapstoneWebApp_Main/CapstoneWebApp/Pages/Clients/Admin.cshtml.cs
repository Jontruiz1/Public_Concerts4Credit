using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using static QRCoder.PayloadGenerator;
using System.Xml.Linq;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;

namespace CapstoneWebApp.Pages.Clients
{
    public class AdminModel : PageModel
    {
        public class Student
        {
            public int studentID { get; private set; }
            public String fName { get; private set; }
            public String lName { get; private set; }
            public String eNumber { get; private set; }
            public DateTime checkInDate { get; private set; }
            public DateTime checkInTime { get; private set; }
            public String checkInTitle { get; private set; }

            public Student(int passedId, String passedfName, String passedlName, String passedEnum, DateTime passedDate, TimeSpan passedTime, String passedTitle)
            {
                studentID = passedId;
                fName = passedfName;
                lName = passedlName;
                eNumber = passedEnum;
                checkInDate = passedDate;
                checkInTime = new DateTime(passedTime.Ticks);
                checkInTitle = passedTitle;
            }
        }
        public class Event
        {

            public int eventID { get; private set; }

            [BindProperty]
            [Required(ErrorMessage = "A title is required for the event.")]
            [StringLength(50, ErrorMessage = "Titles must be between {2} and {1} characters.", MinimumLength = 1)]
            public String eventTitle { get; private set; }

            [BindProperty]
            [Required(ErrorMessage = "A date is required for this event.")]
            [DataType(DataType.Date)]
            public DateTime eventDate { get; private set; }

            [BindProperty]
            [Required(ErrorMessage = "An event time is required for this event.")]
            public DateTime eventTime { get; private set; }

            [BindProperty]
            public String eventDesc { get; private set; }

            [BindProperty]
            [Required(ErrorMessage = "A location is required for the event.")]
            [StringLength(50, ErrorMessage = "Locations must be between {2} and {1} characters.", MinimumLength = 10)]
            public String eventLoc { get; private set; }

            public Event(int passedId, String passedTitle, DateTime passedDate, TimeSpan passedTime, String passedDesc, String passedLoc)
            {
                eventID = passedId;
                eventTitle = passedTitle;
                eventDate = passedDate;
                eventTime = new DateTime(passedTime.Ticks);
                eventDesc = passedDesc;
                eventLoc = passedLoc;
            }

        }

        // initialize the id, profname and students list
        public List<Student> students { get; set; } = new List<Student>();
        public List<Event> events { get; set; } = new List<Event>();
        public Event currEvent = null!;
        public Student currStudent = null!;

        public String profName { get; private set; } = "";
        public int id { get; private set; } = 0;

        public void LoadEvents(SqlConnection connection)
        {
            String sql = "SELECT Event_ID, Title, Date, Time, Description, Location FROM [Event]";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    int count = 0;
                    while (reader.Read())
                    {
                        // add each event that's currently happening to the events list of the professor
                        int eventID = reader.GetInt32(0);
                        String eventTitle = reader.GetString(1);
                        DateTime eventDate = reader.GetDateTime(2);
                        TimeSpan eventTime = reader.GetTimeSpan(3);
                        String eventDesc = reader.GetString(4);
                        String eventLoc = reader.GetString(5);


                        events.Add(new Event(eventID, eventTitle, eventDate, eventTime, eventDesc, eventLoc));

                        count++;
                    }
                }
            }
        }

        public void LoadStudents(SqlConnection connection)
        {
            // grab students that have checked in and are part of the current professor's courses
            String sql = "SELECT DISTINCT [User].User_ID, [User].First_Name, [User].Last_Name, [User].eNumber, [CheckIn].Date, [CheckIn].Time, [Event].Title " +
                 " FROM Professor" +
                 " INNER JOIN Course" +
                 " ON Professor.Professor_ID = Course.Professor_ID" +
                 " INNER JOIN Enrollment" +
                 " ON Course.Course_ID = Enrollment.Course_ID" +
                 " INNER JOIN [User]" +
                 " ON Enrollment.User_ID = [User].User_ID" +
                 " INNER JOIN CheckIn" +
                 " ON [User].User_ID = CheckIn.User_ID " +
                 " INNER JOIN [Event] " +
                 " ON [User].User_ID = CheckIn.User_ID" +
                 " WHERE (CheckIn.Event_ID = [Event].Event_ID and Professor.Professor_ID = " + id + ")";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    // potentially add a search filter later here
                    int count = 0;
                    while (reader.Read())
                    {
                        // adds each student into the students list for the professor
                        int studId = reader.GetInt32(0);
                        String studfName = reader.GetString(1);
                        String studlName = reader.GetString(2);
                        String studEnumber = reader.GetString(3);
                        DateTime checkInDate = reader.GetDateTime(4);
                        TimeSpan checkInTime = reader.GetTimeSpan(5);
                        String checkInTitle = reader.GetString(6);

                        students.Add(new Student(studId, studfName, studlName, studEnumber, checkInDate, checkInTime, checkInTitle));
                        count++;
                    }
                }
            }
        }
        public IActionResult OnPostDelete(int eid)
        {
            try
            {
                String connectionString = "DATABASE_CONNECTION";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sqlSelect = "SELECT * FROM CheckIn WHERE Event_ID = " + eid;
                    
                    using (SqlCommand command = new SqlCommand(sqlSelect, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // If student has already checked in to the event then we can't delete the event without also deleting the checkin information
                            // Just stop the delete command from executing entirely
                            while (reader.Read())
                            {
                                if (reader.GetInt32(4) == eid) return OnGet();
                            }
                        }

                    }
                    
                    
                    //Console.WriteLine("Delete" + eid);
                    // delete the event that matches the event id
                    String sqlDelete = "DELETE FROM Event WHERE Event_ID = '" + eid + "'";
                    SqlCommand delCommand = new SqlCommand(sqlDelete, connection);
                    delCommand.ExecuteNonQuery();

                    // can add this to a finally clause to reduce redundancy
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Response.Cookies.Append("Error", "Something went wrong but we'll look into it!");
                return RedirectToPage("../Error");
            }
            return OnGet();
        }

        public IActionResult OnPostEdit( int eid, String title, DateTime date, DateTime time, String desc, String loc)
        {
            try
            {
                Console.WriteLine("Edit");

                // update for database connection
                String connectionString = "DATABASE_CONNECTION";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // updates the events information. If the information is the same it shouldn't change
                    String sql = "UPDATE Event SET Title = '"
                        + title + "', Date = '"
                        + date + "', Time = '"
                        + time + "', Description = '"
                        + desc + "', Location = '"
                        + loc + "' WHERE Event_ID = " + eid;
                    // connect to database
                    connection.Open();

                    SqlCommand command = new SqlCommand(sql, connection);
                    command.ExecuteNonQuery();

                    connection.Close();

                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Response.Cookies.Append("Error", "Something went wrong but we'll look into it!");
                return RedirectToPage("../Error");
            }
            return OnGet();
        }

        public IActionResult OnPostAddEvent()
        {
            // this can just be added to the button as an asp-page-handler attribute
            // but noticed too late don't want to mess with the code at this point
            return RedirectToPage("./EventManagement");
        }


        public IActionResult OnPostLogOut()
        {
            // logging out, delete name and id cookies
            Response.Cookies.Delete("fName");
            Response.Cookies.Delete("lName");
            Response.Cookies.Delete("title");
            Response.Cookies.Delete("ID");

            // return to login page
            return RedirectToPage("./Clients");
        }
        
        public IActionResult OnGet()
        {
            // get the name and id cookies
            String profDisplayName = "";

            if (Request.Cookies["title"] != null)
            {
                profDisplayName += (Request.Cookies["title"] + " ");
                profDisplayName += Request.Cookies["lName"];
            }
            else
            {
                profDisplayName += Request.Cookies["fName"];
                profDisplayName += Request.Cookies["lName"];
            }
            profName = profDisplayName;
            try
            {
                id = Int32.Parse(Request.Cookies["ID"]!);

                // database connection string
                String connectionString = "DATABASE_CONNECTION";
                SqlConnection connection = new SqlConnection(connectionString);
                using (connection)
                {
                    // open database connection
                    connection.Open();

                    // load students and events from database into list
                    LoadStudents(connection);
                    LoadEvents(connection);

                    // close database connection
                    connection.Close();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.ToString());
                Response.Cookies.Append("Error", "Something went wrong accessing the server database, we'll look into it!");
                return RedirectToPage("../Error");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Response.Cookies.Append("Error", "Something went wrong but we'll look into it!");
                return RedirectToPage("../Error");
            }
            return Page();


        }
    }
}
