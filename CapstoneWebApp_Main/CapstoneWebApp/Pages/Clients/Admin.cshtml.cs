using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Reflection.Metadata.Ecma335;

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
            public TimeSpan checkInTime { get; private set; }
            public String checkInTitle { get; private set; }

            public Student(int passedId, String passedfName, String passedlName, String passedEnum, DateTime passedDate, TimeSpan passedTime, String passedTitle)
            {
                studentID = passedId;
                fName = passedfName;
                lName = passedlName;
                eNumber = passedEnum;
                checkInDate = passedDate;
                checkInTime = passedTime;
                checkInTitle = passedTitle;
            }

        }
        public class Event
        {
            public int eventID { get; private set; }
            public String eventTitle { get; private set; }
            public DateTime eventDate { get; private set; }
            public TimeSpan eventTime { get; private set; }
            public String eventDesc { get; private set; }

            public String eventLoc { get; private set; }

            public Event(int passedId, String passedTitle, DateTime passedDate, TimeSpan passedTime, String passedDesc, String passedLoc)
            {
                eventID = passedId;
                eventTitle = passedTitle;
                eventDate = passedDate;
                eventTime = passedTime;
                eventDesc = passedDesc;
                eventLoc = passedLoc;
            }

        }

        public List<Student> students { get; set; } = new List<Student>();
        public List<Event> events { get; set; } = new List<Event>();
        public String profName { get; private set; } = "";
        public int id { get; private set; } = 0;

        // initialize the id, profname and students list

        // function that passes in the name and id of the prof from the Clients cshtml 
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
            // get the name and id cookies (if existing, should be, but idk)
            String profDisplayName = "";


            // i so badly want to turn this into a ternary statement but it would be so unreadable
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
            id = Int32.Parse(Request.Cookies["ID"]!);

            try
            {
                // database connection string
                String connectionString = "DatabaseConnectionString";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // connect to database
                    connection.Open();

                    // pull user id, name, enumber, course id and checkin id from each user
                    // String sql = "SELECT User_ID, First_Name, Last_Name, eNumber FROM [User]";
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
                            // I was thinking maybe we shouldn't display too many students at once
                            // and let the professor search for a specific student if, say, we had like
                            // 2000 students
                            
                            // the search can probably be added later
                            int count = 0;
                            while (reader.Read() && count < 10) {
                                // adds each student into the students list for the professor

                                // (maybe) TODO: make it so only the students that are actually in the professors classes are added to the students list
                                // maybe find the intersection between the professor's classes and the students? Not sure how to make this work
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

                    sql = "SELECT Event_ID, Title, Date, Time, Description, Location FROM [Event]";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            int count = 0;
                            while (reader.Read() && count < 10)
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


                    // close databae connection
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
