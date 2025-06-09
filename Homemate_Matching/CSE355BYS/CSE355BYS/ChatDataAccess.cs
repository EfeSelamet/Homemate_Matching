using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data; // Required for CommandType

// If ViewModels are in another namespace:
// using Homemate_Matching.Models; // Or whatever namespace you use for ViewModels

namespace Homemate_Matching.Data // Or your preferred namespace for DAL
{
    public class UserViewModel
    {
        public string Username { get; set; } // From RegistrationInfo.username (PK)
        // The IsOnline property will be handled by the Hub, not directly by this DAL method
        public bool IsOnline { get; set; }
    }

    public class MessageViewModel
    {
        // public string MessageId { get; set; } // Your Message table doesn't have a visible ID, so we'll omit this
        public string SenderUsername { get; set; }    // Corresponds to Message.username1
        public string RecipientUsername { get; set; } // Corresponds to Message.username2
        public string MessageContent { get; set; }    // Corresponds to Message.Text
        public DateTime Timestamp { get; set; }       // Corresponds to Message.Date
    }
    public class ChatDataAccess
    {
        private readonly string _connectionString;

        public ChatDataAccess()
        {
            // Read the connection string from web.config
            // Ensure you have a connection string named "conStr" in your web.config
            _connectionString = ConfigurationManager.ConnectionStrings["conStr"]?.ConnectionString;
            if (string.IsNullOrEmpty(_connectionString))
            {
                // Handle missing connection string error, perhaps log it or throw a specific exception
                throw new ConfigurationErrorsException("Connection string 'conStr' not found or is empty in web.config.");
            }
        }

        public List<UserViewModel> GetAllUsers()
        {
            var users = new List<UserViewModel>();
            string query = "SELECT username FROM RegistrationInfo ORDER BY username ASC;";

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    try
                    {
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            users.Add(new UserViewModel
                            {
                                Username = reader["username"].ToString(),
                                IsOnline = false // IsOnline will be determined by the Hub
                            });
                        }
                        reader.Close();


                    }
                    catch (SqlException ex)
                    {
                        // Log error (e.g., using a logging framework or Console.WriteLine for debugging)
                        Console.Error.WriteLine("SQL Error in GetAllUsers: " + ex.Message);
                        // Depending on your error handling strategy, you might re-throw,
                        // throw a custom exception, or return an empty list.
                        throw; // Re-throwing for the Hub to potentially handle or log further
                    }
                }
            }
            return users;
        }
        public List<UserViewModel> GetAllMatchedUsers(string username)
        {
            var users = new List<UserViewModel>();

            // Corrected SQL query string: added 'WHERE' to the first part
            string query = "SELECT username1 FROM dbo.Match WHERE username2 = @username " +
                           "UNION " +
                           "SELECT username2 FROM dbo.Match WHERE username1 = @username;"; // Added semicolon for good practice

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    // Add parameter before the try block
                    cmd.Parameters.AddWithValue("@username", username);

                    try
                    {
                        con.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            // Corrected reading logic: Loop through all results directly
                            while (reader.Read())
                            {
                                users.Add(new UserViewModel
                                {
                                    // The column from UNION will be named 'username1' based on the first SELECT
                                    Username = reader["username1"].ToString(),
                                    IsOnline = false // IsOnline will be determined by the Hub
                                });
                            }
                            // No need for an 'else' here, as an empty list is a valid outcome if no matches are found.
                            // If you want to log when no matches are found, you can do it after the loop:
                            if (users.Count == 0)
                            {
                                Console.WriteLine($"No matches found for user: {username}");
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        // Log error
                        Console.Error.WriteLine("SQL Error in GetAllMatchedUsers: " + ex.Message);
                        // Optionally log the full exception details: Console.Error.WriteLine(ex.ToString());
                        throw; // Re-throwing for the Hub to potentially handle or log further
                    }
                    catch (Exception ex) // Catch other potential exceptions (e.g., network issues)
                    {
                        Console.Error.WriteLine("General Error in GetAllMatchedUsers: " + ex.Message);
                        throw;
                    }
                }
            }
            return users;
        }
        public void SaveMessage(string senderUsername, string recipientUsername, string messageContent, DateTime timestamp)
        {
            // Assuming username1 is the sender and username2 is the recipient in your Message table
            string query = "INSERT INTO Message (username1, username2, Text, Date) VALUES (@sender, @recipient, @content, @timestamp);";

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@sender", senderUsername);
                    cmd.Parameters.AddWithValue("@recipient", recipientUsername);
                    cmd.Parameters.AddWithValue("@content", messageContent);
                    cmd.Parameters.AddWithValue("@timestamp", timestamp);

                    try
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        Console.Error.WriteLine("SQL Error in SaveMessage: " + ex.Message);
                        throw;
                    }
                }
            }
        }

        public List<MessageViewModel> GetMessageHistory(string currentUsername, string otherUsername)
        {
            var messages = new List<MessageViewModel>();
            // Fetches messages where the pair of users are either (current, other) or (other, current)
            string query = @"
                SELECT username1, username2, Text, Date 
                FROM Message 
                WHERE (username1 = @userA AND username2 = @userB) 
                   OR (username1 = @userB AND username2 = @userA) 
                ORDER BY Date ASC;";

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@userA", currentUsername);
                    cmd.Parameters.AddWithValue("@userB", otherUsername);

                    try
                    {
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            DateTime rawDate = Convert.ToDateTime(reader["Date"]);
                            messages.Add(new MessageViewModel
                            {
                                SenderUsername = reader["username1"].ToString(),
                                RecipientUsername = reader["username2"].ToString(),
                                MessageContent = reader["Text"].ToString(),
                                // Assuming the 'Date' column in your SQL table stores UTC time
                                Timestamp = DateTime.SpecifyKind(rawDate, DateTimeKind.Utc)
                            });
                        }
                        reader.Close();


                    }
                    catch (SqlException ex)
                    {
                        Console.Error.WriteLine("SQL Error in GetMessageHistory: " + ex.Message);
                        throw;
                    }
                }
            }
            return messages;
        }
    }
}