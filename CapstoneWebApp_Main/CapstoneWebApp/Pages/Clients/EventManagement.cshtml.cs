using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace CapstoneWebApp.Pages.Clients
{
    public class EventManagementModel : PageModel
    {
        List<Tuple<String, String, String>> evnt = new List<Tuple<String, String, String>>();


        [BindProperty]
        [Required(ErrorMessage = "A title is required for the event.")]
        [StringLength(50, ErrorMessage = "Titles must be between {2} and {1} characters.", MinimumLength = 1)]
        public String title { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "A date is required for this event.")]
        [DataType(DataType.Date)]
        public DateTime date { get; set; } = DateTime.Now;

        [BindProperty]
        [Required(ErrorMessage = "An event time is required for this event.")]
        public TimeSpan time { get; set; } = TimeSpan.MaxValue;

        [BindProperty]
        public String desc { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage ="A location is required for the event.")]
        [StringLength(50, ErrorMessage = "Locations must be between {2} and {1} characters.", MinimumLength =10)]
        public String loc { get; set; } = string.Empty;



        public IActionResult OnPostCreateEvent()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            else
            {
                try
                {
                    // database connection string
                    String connectionString = "DATABASE_CONNECTION";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        // connect to the database and insert new values into it
                        string insertQuery = "INSERT INTO [Event] VALUES ('" + title + "','" + date + "','" + time + "','" + desc.Replace("'", "''") + "','" + loc + "')";

                        SqlCommand command = new SqlCommand(insertQuery, connection);
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }
                catch (SqlException ex)
                {
                    Response.Cookies.Append("Error", "Something went wrong accessing the server database, we'll look into it!");
                    Console.WriteLine(ex.ToString());   
                    return RedirectToPage("../Error");
                }
                catch (Exception)
                {
                    Response.Cookies.Append("Error", "Something went wrong but we'll look into it!");
                    return RedirectToPage("../Error");
                }
            }
            return Page();
        }
    }
}
