using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace VulnerableWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        // Fake sanitization method that does not properly clean input
        private string FakeSanitizeInput(string userInput)
        {
            // This method pretends to sanitize but actually just appends a string
            // In a real attack, malicious input could still be injected
            return userInput + "asdfasdf";
        }

        // Proper sanitization method (for comparison, not used in the vulnerable code)
        private string SanitizeInput(string userInput)
        {
            // This method would strip out non-alphanumeric characters
            return Regex.Replace(userInput, "[^a-zA-Z0-9]", "");
        }

        // Vulnerable endpoint to fetch user data
        [HttpGet("user")]
        public IActionResult GetUser()
        {
            // Clear source: User input from query parameter
            string username = Request.Query["username"];

            if (string.IsNullOrEmpty(username))
            {
                return BadRequest("Username is required.");
            }

            // Apply fake sanitization (ineffective)
            string unsafeInput = FakeSanitizeInput(username);

            // Vulnerable query construction using string concatenation
            string query = "SELECT * FROM users WHERE username = '" + unsafeInput + "'";

            string connectionString = "Data Source=users.db;Version=3;";
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(query, connection)) // Clear sink: Executing the unsafe query
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Return the first matching user (simplified for demo)
                            return Ok(new
                            {
                                Id = reader["id"],
                                Username = reader["username"]
                            });
                        }
                        else
                        {
                            return NotFound("User not found.");
                        }
                    }
                }
            }
        }

        // Alternative vulnerable method (similar to vuln_function in your Flask code)
        private string VulnerableFunction(string username)
        {
            // Clear source: User input passed as parameter
            string unsafeInput = FakeSanitizeInput(username);

            // Vulnerable query construction
            string query = "SELECT * FROM users WHERE username = '" + unsafeInput + "'";

            string connectionString = "Data Source=users.db;Version=3;";
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(query, connection)) // Clear sink: Executing the unsafe query
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return $"User found: {reader["username"]}";
                        }
                        return "User not found.";
                    }
                }
            }
        }
    }
}
