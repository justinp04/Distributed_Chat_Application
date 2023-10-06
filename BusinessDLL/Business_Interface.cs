using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Drawing;

namespace ChatBot_DLL
{
    [ServiceContract]
    public interface Business_Interface
    {
        [OperationContract]
        int Find_By_Username(string Username);

        [OperationContract]
        string Find_Username_By_ID(int User_ID);

        [OperationContract]
        int Find_By_Room_Name(string Name);

        [OperationContract]
        string User_Login(string Username, string Password);

        [OperationContract]
        bool Create_User(string username, string password);

        [OperationContract]
        string Create_Chat_Room(string room_name, int bool_val);

        [OperationContract]
        string Join_Chat_Room(int User_ID, int Room_ID);

        [OperationContract]
        string Leave_Chat_Room(int User_ID, int Room_ID);

        [OperationContract]
        string Send_Message_To_Room(string Message, int User_ID, int Room_ID);

        [OperationContract]
        string Send_File_Message_To_Room(string Message, int User_ID, int Room_ID, byte[] File);

        [OperationContract]
        List<RoomObject> View_All_Chat_Rooms();

        [OperationContract]
        List<string> View_All_Users();

        [OperationContract]
        List<MessageObject> Update_Message_Line(int Room_ID);
    }

}