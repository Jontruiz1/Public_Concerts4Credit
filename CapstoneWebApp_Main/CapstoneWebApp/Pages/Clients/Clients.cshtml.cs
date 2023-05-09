using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;

namespace CapstoneWebApp.Pages.Clients
{
    public class ClientsModel : PageModel
    {
        public String email { get; private set; } = string.Empty;
        public String password { get; private set; } = string.Empty;
        public String fName { get; private set; } = string.Empty;
        public String lName { get; private set; } = string.Empty;
        public String title { get; private set; } = string.Empty;

        public int id { get; private set; } = int.MinValue;

        // sets the email and password as required fields
        [BindProperty]
        [EmailAddress]
        [StringLength(255,ErrorMessage ="Emails must be between {2} and {1} characters.", MinimumLength =10 )]
        [Required(ErrorMessage ="Please enter an email address.")]
        public string inputUser { get; set; } = string.Empty;

        [BindProperty]
        [StringLength(50, ErrorMessage ="Password must be between {2} and {1} characters.",MinimumLength=6)]
        [Required(ErrorMessage = "Please enter a password")]
        public string inputPass { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            if (Request.Cookies["fName"] != null)
            {
                return RedirectToPage("./Admin");
            }
            return Page();
        }
        public IActionResult OnPost()
        {
            var nameCheck = inputUser;
            var passCheck = inputPass;

            if (!ModelState.IsValid)
            {
                return Page();
            }
            else
            {
                try
                {
                    // database connection string
                    String connectionString = "DatabaseConnectionString";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        // connect to the databaes
                        connection.Open();
                        String sql = "SELECT * FROM Professor";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    // get the name, email, password, id from the database
                                    id = reader.GetInt32(0);
                                    fName = reader.GetString(1);
                                    lName = reader.GetString(2);
                                    title = reader.GetString(3);
                                    email = reader.GetString(4);
                                    password = reader.GetString(5);

                                    // check the email and password to database information
                                    if (nameCheck.Equals(email) && passCheck.Equals(password)) {
                                        connection.Close();

                                        // save the name and id cookies, not password, bad idea
                                        // technically incredibly insecure based on what i read about 'injecting cookies' into a website but idc right now

                                        Response.Cookies.Append("fName", fName);
                                        Response.Cookies.Append("lName", lName);
                                        Response.Cookies.Append("title", title);
                                        Response.Cookies.Append("ID", id.ToString());
                                        return RedirectToPage("./Admin");
                                    }
                                }
                            }
                        }
                    }
                }
                catch (SqlException)
                {
                    Response.Cookies.Append("Error", "Something went wrong accessing the server database, we'll look into it!");
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
