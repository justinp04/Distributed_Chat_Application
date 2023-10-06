using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ChatBot_DLL
{
    internal class Database
    {
        private SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\ChatBot_Database.mdf;Integrated Security=True");
        private readonly string UserLogin_Error = "ERROR_USER_LOGIN";
        private readonly string Unexpected_Error = "ERROR_UNKNOWN";

        public int Find_Username(string Username)
        {
            conn.Open();
            string val = "";

            try
            {
                if (!(string.IsNullOrEmpty(Username)))//Checks if the username and password fields aren't empty
                {
                    SqlCommand cmd = new SqlCommand("SELECT * From User_Table WHERE Username = '" + Username + "'", conn);//Should only be one unique username per user
                    cmd.Parameters.AddWithValue("id", val);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())//Username and password found
                    {
                        val = reader["User_ID"].ToString();//Getting user data
                    }
                    else//If data is not found
                    {
                        val = "-1";
                    }
                }
            }
            catch
            {
                conn.Close();
            }

            conn.Close();

            return Int32.Parse(val);
        }

        public string Find_Username_By_ID(int ID)
        {
            conn.Open();
            string val = "";

            try
            {
            
                SqlCommand cmd = new SqlCommand("SELECT * From User_Table WHERE User_ID = '" + ID + "'", conn);//Should only be one unique username per user
                cmd.Parameters.AddWithValue("id", val);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    val = reader["Username"].ToString();//Getting username through ID
                }
                else//If data is not found
                {
                    val = Unexpected_Error;
                }
                
            }
            catch
            {
                conn.Close();
            }

            conn.Close();

            return val;
        }



        public int Find_Room_Name(string Room_Name)
        {
            conn.Open();
            string text = "";
            int ID = -1;

            try
            {
                if (!(string.IsNullOrEmpty(Room_Name)))//Checks if the username and password fields aren't empty
                {
                    SqlCommand cmd = new SqlCommand("SELECT * From Room_Table WHERE Name = '" + Room_Name + "'", conn);//Should only be one unique room name per room
                    cmd.Parameters.AddWithValue("id", text);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())//Username and password found
                    {
                        text = reader["Room_ID"].ToString();//Getting user data
                        ID = Int32.Parse(text);
                    }
                    else//If data is not found
                    {
                        text = Unexpected_Error;
                        ID = -1;
                    }
                }
            }
            catch
            {
                conn.Close();
            }

            conn.Close();

            return ID;
        }

        public bool CreateUser(string username, string password)
        {
            conn.Open();

            bool isUnique = false;

            try
            {
                if (!(string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)))//Checks if any of the strings are empty
                {
                    SqlDataAdapter sda = new SqlDataAdapter("SELECT * From User_Table WHERE Username = '" + username + "'", conn);//Checks if username is unique or not

                    DataTable dtable = new DataTable();
                    sda.Fill(dtable);//Creates a table of data

                    if (dtable.Rows.Count > 0)//If a username already exists
                    {
                        isUnique = false;
                    }
                    else//If username does not exist
                    {
                        isUnique = true;
                        SqlCommand cmd = new SqlCommand("INSERT INTO User_Table (Username, Password) VALUES('" + username + "', '" + password + "');", conn);
                        cmd.ExecuteNonQuery();

                    }
                }

            }
            catch
            {
                conn.Close();
            }

            conn.Close();
            return isUnique;

        }

        //Verifies if:
        // 1). Checks if a user with a valid username and password exists
        public string CheckUserLogin(string Username, string Password)
        {
            string text = UserLogin_Error;
            conn.Open();

            try
            {
                if (!(string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password)))//Checks if the username and password fields aren't empty
                {
                    SqlCommand cmd = new SqlCommand("SELECT * From User_Table WHERE Username = '" + Username + "' AND Password = '" + Password + "'", conn);
                    cmd.Parameters.AddWithValue("id", text);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())//Username and password found
                    {
                        text = reader["User_ID"].ToString();//Getting user data
                    }
                    else//If data is not found
                    {
                        text = UserLogin_Error;
                    }
                }

            }
            catch
            {
                text = UserLogin_Error;

                conn.Close();
            }


            conn.Close();
            return text;
        }

        public string CreateChatRoom(string room_name, int bool_val)
        {
            string text = Unexpected_Error;
            conn.Open();

            try
            {
                if (!string.IsNullOrEmpty(room_name))
                {
                    SqlDataAdapter sda = new SqlDataAdapter("SELECT * From Room_Table WHERE Name = '" + room_name + "'", conn);//Checks if room name is unique or not

                    DataTable dtable = new DataTable();
                    sda.Fill(dtable);//Creates a table of data

                    if (dtable.Rows.Count > 0)//If a room name already exists
                    {
                        text = Unexpected_Error;
                    }
                    else//If room name does not exist
                    { 
                        SqlCommand cmd = new SqlCommand("INSERT INTO Room_Table (Name, Private) VALUES('" + room_name + "', " + bool_val + ");", conn);
                        cmd.ExecuteNonQuery();
                        text = "Success";
                    }
                }
            }
            catch
            {
                text = Unexpected_Error;
                conn.Close();
            }

            conn.Close();
            return text;
        }

        public string JoinChatRoom(int User_ID, int Room_ID)
        {
            string text = "";
            conn.Open();

            try
            {

                SqlCommand cmd = new SqlCommand("INSERT INTO Client_Table (User_ID, Room_ID) VALUES('" + User_ID + "', '"+Room_ID+"');", conn);
                cmd.ExecuteNonQuery();
            }
            catch
            {
                text = Unexpected_Error;
                conn.Close();
            }

            conn.Close();
            return text;
        }

        public string LeaveChatRoom(int User_ID, int Room_ID)
        {
            string text = "";
            conn.Open();

            try
            {
                SqlCommand cmd = new SqlCommand("DELETE FROM Client_Table WHERE User_ID = '"+ User_ID +"' AND Room_ID = '"+Room_ID+"';", conn);
                cmd.ExecuteNonQuery();
            }
            catch
            {
                text = Unexpected_Error;
                conn.Close();
            }

            conn.Close();
            return text;
        }


        public List<RoomObject> ViewAllChatRooms()
        {
            List<RoomObject> rooms = new List<RoomObject>();
            int i = 0;

            conn.Open();

            try
            {
                string selectquery = "SELECT * FROM Room_Table WHERE Private = 0;"; // Finding room IDs for a user for non private chat rooms
                SqlCommand cmd = new SqlCommand(selectquery, conn);
                SqlDataReader reader1;
                reader1 = cmd.ExecuteReader();
                while (reader1.Read())
                {
                    RoomObject temp = new RoomObject();
                    temp.Room_ID = (Int32.Parse(reader1["Room_ID"].ToString()));
                    temp.Name = (reader1["Name"].ToString());

                    rooms.Add(temp);
                    i++;
                }
            }
            catch
            {
                conn.Close();
            }

            conn.Close();

            return rooms;
        }

        public List<string> ViewAllUsers()
        {
            List<string> User_List = new List<string>();

            conn.Open();

            try
            {
                string selectquery = "SELECT * FROM User_Table;"; // Finding room IDs for a user
                SqlCommand cmd = new SqlCommand(selectquery, conn);
                SqlDataReader reader1;
                reader1 = cmd.ExecuteReader();
                while (reader1.Read())
                {
                    string temp = "";
                    temp = (reader1["Username"].ToString());

                    User_List.Add(temp);
                }
            }
            catch
            {
                conn.Close();
            }

            conn.Close();

            return User_List;
        }


        public string SendMessageToRoom(string Message, int User_ID, int Room_ID)//To use this method, make sure you use CreateChatRoom once and JoinChatRoom twice on each participant to make a channel to privately message
        {
            
            string text = "";
            conn.Open();

            try
            {
                if (!string.IsNullOrEmpty(Message))
                {
                    SqlCommand cmd = new SqlCommand("INSERT INTO Message_Table (Room_ID, User_ID, Message) VALUES ('" + Room_ID + "', '" + User_ID + "', '" + Message + "');", conn);
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                text = Unexpected_Error;
                conn.Close();
            }

            conn.Close();
            return text;

        }

        public string SendFileMessageToRoom(string Message, int User_ID, int Room_ID, byte[] File)
        {    
            string text = "Nothing Happened";

            string username = Find_Username_By_ID(User_ID);

            conn.Open();

            try
            {
                if (!string.IsNullOrEmpty(Message))//Checks if file and message is not null or not
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO Message_Table (Room_ID, User_ID, Message, Files) VALUES (@Room_ID, @User_ID, @Message, @Files);", conn))
                    {
                        cmd.Connection = conn;

                        Message = username + ": "+ Message;

                        cmd.Parameters.AddWithValue("@Room_ID", Room_ID);
                        cmd.Parameters.AddWithValue("@User_ID", User_ID);
                        cmd.Parameters.AddWithValue("@Message", Message);
                        cmd.Parameters.AddWithValue("@Files", File);

                        text = " Pre-Test: Message: " + Message + " User ID: " + User_ID + " Room ID: " + Room_ID;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch(SqlException e)
            {
                text = e.Message;
                conn.Close();
            }

            conn.Close();
            return text;
        }


        public List<MessageObject> UpdateMessageLine(int Room_ID)//Updates messages in one room
        {
            List<MessageObject> messages = new List<MessageObject>();
            int i = 0;

            conn.Open();
            string selectquery = "SELECT * FROM Message_Table WHERE Room_ID = " + Room_ID;
            SqlCommand cmd = new SqlCommand(selectquery, conn);
            SqlDataReader reader1;
            reader1 = cmd.ExecuteReader();
            while (reader1.Read())
            {
                MessageObject temp = new MessageObject();

                temp.Message_ID = (Int32.Parse(reader1["Message_ID"].ToString()));
                temp.Room_ID = (Int32.Parse(reader1["Room_ID"].ToString()));
                temp.User_ID = (Int32.Parse(reader1["User_ID"].ToString()));
                temp.Message = (reader1["Message"].ToString());
                temp.File = (ObjectToByteArray(reader1["Files"]));

                messages.Add(temp);

                i++;
            }
            conn.Close();


            return messages;
        }



        private static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }

}
