using CapstoneWebApp.Pages.Clients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QRCoder;
using System.Drawing;
using System.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;
using System;
using System.ComponentModel.Design.Serialization;

namespace CapstoneWebApp.Pages
{
    public class IndexModel : PageModel
    {
        public DateTime date { get; private set; } = new DateTime();
        public TimeSpan time { get; private set; } = new TimeSpan();
        public byte[] image { get; private set; } = new byte[] { };
        public String title { get; private set; } = "";
        public String key { get; private set; } = "";
        public String currKey { get; private set; } = "";

        public void OnGet()
        {
            try
            {
                // database connection string
                String connectionString = "DATABASE_CONNECTION";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // connect to database
                    connection.Open();

                    // pull event id, title, date, and time of the most recent event
                    String sql = "SELECT Event_ID, Title, Date, Time FROM Event";
                    
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                key = "" + reader.GetInt32(0);
                                title = "" + reader.GetString(1);
                                date = reader.GetDateTime(2);
                                time = reader.GetTimeSpan(3);

                                // the time also doesn't dynamically update so I think that as long as the day of the event and the current day are the same
                                // as well current time being within an hour or 2, it should display qr code

                                DateTime dateTime = DateTime.Now;
                                TimeSpan t1 = new TimeSpan(-2, 0, 0);
                                TimeSpan d1 = new TimeSpan(0, 0, 0);
                                d1 = time.Add(t1);
                                TimeSpan t2 = new TimeSpan(2, 0, 0);
                                TimeSpan d2 = new TimeSpan(0, 0, 0);
                                d2 = time.Add(t2);
                                if (date.Date == dateTime.Date)                                         //checks if event is happening today
                                {
                                    if ((dateTime.TimeOfDay >= d1) && (dateTime.TimeOfDay <= d2))         //checks if event is happening within 2 hours
                                    {
                                        currKey = key;
                                    }
                                }
                            }
                        }
                    }
                    // close databae connection
                    connection.Close();
                    
                    // generate a png qr code
                    QRCodeGenerator qrGenerator = new QRCodeGenerator();

                    // error correction capability on the qr code compensates for dirt and other miscellaneous materials
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(currKey, QRCodeGenerator.ECCLevel.L);

                    // generate the byte array which represents the QR code
                    PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                    byte[] img = qrCode.GetGraphic(20);
                    image = img;
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}