using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot_DLL
{
    public class Database_Requests
    {
        private Database data = new Database();

        public int Find_By_Username(string Username)
        {
            return data.Find_Username(Username);
        }

        public string Find_Username_By_ID(int User_ID)
        {
            return data.Find_Username_By_ID(User_ID);
        }

        public int Find_By_Room_Name(string Name)
        {
            return data.Find_Room_Name(Name);
        }

        public string Verify_UserLogin(string Username, string Password)
        {
            return data.CheckUserLogin(Username, Password);
        }

        public bool Create_User(string username, string password)
        {
            return data.CreateUser(username, password);
        }

        public string Create_Chatroom(string room_name, int bool_val)
        {
            return data.CreateChatRoom(room_name, bool_val);
        }

        public string Join_Chatroom(int User_ID, int Room_ID)
        {
            return data.JoinChatRoom(User_ID, Room_ID);
        }

        public string Leave_Chatroom(int User_ID, int Room_ID)
        {
            return data.LeaveChatRoom(User_ID, Room_ID);
        }

        public List<RoomObject> View_All_ChatRooms()
        {
            return data.ViewAllChatRooms();
        }

        public List<string> View_All_Users()
        {
            return data.ViewAllUsers();
        }

        public string Send_Message_To_Room(string Message, int User_ID, int Room_ID)
        {
            return data.SendMessageToRoom(Message, User_ID, Room_ID);
        }

        public string SendFileMessageToRoom(string Message, int User_ID, int Room_ID, byte[] File)
        {
            return data.SendFileMessageToRoom(Message, User_ID, Room_ID, File);
        }

        public List<MessageObject> UpdateMessageLine(int Room_ID)
        {
            return data.UpdateMessageLine(Room_ID);
        }






    }
}
