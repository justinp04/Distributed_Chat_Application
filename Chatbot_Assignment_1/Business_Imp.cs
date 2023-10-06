using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Drawing;
using ChatBot_DLL;

namespace Chatbot_Assignment_1
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    internal class Business_Imp : Business_Interface
    {
        private Database_Requests req = new Database_Requests();

        public int Find_By_Username(string Username)
        {
            return req.Find_By_Username(Username);
        }

        public string Find_Username_By_ID(int User_ID)
        {
            return req.Find_Username_By_ID(User_ID);
        }

        public int Find_By_Room_Name(string Name)
        {
            return req.Find_By_Room_Name(Name);
        }

        public string User_Login(string Username, string Password)
        {
            return req.Verify_UserLogin(Username, Password);
        }

        public bool Create_User(string username, string password)
        {
            return req.Create_User(username, password);
        }

        public string Create_Chat_Room(string room_name, int bool_val)
        {
            return req.Create_Chatroom(room_name, bool_val);
        }

        public string Join_Chat_Room(int User_ID, int Room_ID)
        {
            return req.Join_Chatroom(User_ID, Room_ID);
        }

        public string Leave_Chat_Room(int User_ID, int Room_ID)
        {
            return req.Leave_Chatroom(User_ID, Room_ID);
        }

        public string  Send_Message_To_Room(string Message, int User_ID, int Room_ID)
        {
            return req.Send_Message_To_Room(Message, User_ID, Room_ID);
        }

        public string Send_File_Message_To_Room(string Message, int User_ID, int Room_ID, byte[] File)
        {
            return req.SendFileMessageToRoom(Message, User_ID, Room_ID, File);
        }

        public List<RoomObject> View_All_Chat_Rooms()
        {
            return req.View_All_ChatRooms();
        }
        public List<string> View_All_Users()
        {
            return req.View_All_Users();
        }

        public List<MessageObject> Update_Message_Line(int Room_ID)
        {
            return req.UpdateMessageLine(Room_ID);

        }
    }
}