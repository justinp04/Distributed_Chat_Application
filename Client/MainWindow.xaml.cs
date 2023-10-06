using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChatBot_DLL;
using Microsoft.Win32;
using static System.Net.WebRequestMethods;

namespace Client
{
    public partial class MainWindow : Window
    {
        private Business_Interface foob;
        private readonly string Unexpected_Error = "ERROR_UNKNOWN";
        private int Current_Room_ID = 0;
        private int Private_Room_ID = 0;
        private int Current_User_ID = 0;
        private int Private_Target_User_ID = 0;
        private readonly int True = 1;
        private readonly int False = 0;


        public MainWindow()
        {
            InitializeComponent();
            ChannelFactory<Business_Interface> foobFactory;
            NetTcpBinding tcp = new NetTcpBinding();
            string URL = "net.tcp://localhost:8100/DataService";
            foobFactory = new ChannelFactory<Business_Interface>(tcp, URL);
            foob = foobFactory.CreateChannel();

           
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh_User_List();
            Refresh_Chat_Room_List();
            Refresh_Message_Line();
            Refresh_Private_Message_Line();
        }

        private async void Sign_Up_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                int i = 0;

                String usernameText = Username_Text.Text, passwordText = Password_Text.Text;

                // Create an async task for sign up
                Task<bool> task = new Task<bool>(() => createUser(usernameText, passwordText));
                bool succeeded;

                task.Start();

                if(await Task.WhenAny(task, Task.Delay(3000)) == task)
                {
                    //State that user sign up has suceeded
                    succeeded = await task;
                }
                else
                {
                    throw new TimeoutException();
                }

                if (succeeded)
                {
                    Chat_Room_List.ItemsSource = new List<RoomObject>();
                    User_List_Box.ItemsSource = new List<string>();
                    Message_Line_List.ItemsSource = new List<MessageObject>();
                    Private_Message_Line.ItemsSource = new List<MessageObject>();
                    MessageBox.Show("User " + Username_Text.Text + " has been created.");
                }
                else
                {
                    MessageBox.Show("Username \"" + Username_Text.Text + "\" already exists, please choose a different username.");
                }
            }
            catch (TimeoutException ex)
            {
                MessageBox.Show("ERROR 408: Request Timeout");
            }
        }

        private Boolean createUser(string username, string password)
        {
            bool succeeded = foob.Create_User(username, password);
            return succeeded;
        }

        private async void Log_In_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int i = 0;

                String usernameText = Username_Text.Text, passwordText =Password_Text.Text;

                // Create async task for login
                Task<string> task = new Task<string>(() => userLogin(usernameText, passwordText));
                string Username = "";

                task.Start();

                if(await Task.WhenAny(task, Task.Delay(3000)) == task)
                {
                    // Updat the username based on the string returned from userLogin
                    Username = await task;
                }
                else
                {
                    throw new TimeoutException();
                }

                if (int.TryParse(Username, out i))//If username does exist
                {
                    MessageBox.Show("You have successfully logged in. Welcome " + Username_Text.Text);
                    Refresh_Chat_Room_List();
                    Refresh_User_List();
                    Message_Line_List.ItemsSource = new List<MessageObject>();
                    Private_Message_Line.ItemsSource = new List<MessageObject>();
                    EnableButtons();
                    Current_User_ID = foob.Find_By_Username(Username_Text.Text);

                    // Clear textViews text after signing in
                    Username_Text.Text = "";
                    Password_Text.Text = "";
                }
                else
                {
                    MessageBox.Show("User \"" + Username_Text.Text + "\" does not exist, please create an account instead.");
                }
            }
            catch (TimeoutException timeoutException)
            {
                MessageBox.Show("ERROR 408: Request Timeout");
            }
        }

        private string userLogin(string pUsername, string password)
        {
            string username = foob.User_Login(pUsername, password);
            return username;
        }

        private void Log_Out_Click(object sender, RoutedEventArgs e)
        {
            Chat_Room_List.ItemsSource = new List<RoomObject>();
            User_List_Box.ItemsSource = new List<string>();
            Message_Line_List.ItemsSource = new List<MessageObject>();
            Private_Message_Line.ItemsSource = new List<MessageObject>();
            Current_Room_ID = 0;
            Private_Room_ID = 0;
            Current_User_ID = 0;
            Private_Target_User_ID = 0;
            DisableButtons();

            MessageBox.Show("You have logged out!");
        }

        private void EnableButtons()
        {
            Send_Button.IsEnabled = true;
            Upload_File_Button.IsEnabled = true;
            Create_Button.IsEnabled=true;
            Private_Upload_File_Button.IsEnabled = true;
            Private_Send_Button.IsEnabled = true;
        }

        private void DisableButtons()
        {
            Send_Button.IsEnabled = false;
            Upload_File_Button.IsEnabled = false;
            Create_Button.IsEnabled = false;
            Private_Upload_File_Button.IsEnabled = false;
            Private_Send_Button.IsEnabled = false;
        }

        //Creates a chat room for you, not a private room but a public one
        private void Create_Room_Click(object sender, RoutedEventArgs e)
        {
            string Outcome = foob.Create_Chat_Room(New_Room_Text.Text, False);
            if (!Outcome.Equals(Unexpected_Error))//If room name doesn't exist yet
            {
                Refresh_Chat_Room_List();
                Current_Room_ID = foob.Find_By_Room_Name(New_Room_Text.Text);

                // Clear the room ID textView
                New_Room_Text.Text = "";

                MessageBox.Show("You have successfully created a new chat room in: " + New_Room_Text.Text);
            }
            else
            {
                MessageBox.Show("Chat Room \"" + New_Room_Text.Text + "\" already exists, please create a new chat room name instead.");
            }
        }

        private void Upload_File_Button_Click(object sender, RoutedEventArgs e)
        {
            string filePath;

            // Get the user to select a file.
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = "C:\\";
            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 4;

            if (openFileDialog.ShowDialog() == true)
            {
          
                filePath = openFileDialog.FileName;

                // We must conver the file to a byte array
                
           
                
           
                
                byte[] file = fileToByteArray(filePath);

                if (file.Length < 65536)
                {

                    int index = Chat_Room_List.SelectedIndex;
                    RoomObject temp = (RoomObject)(Chat_Room_List.Items[index]);
                    Current_Room_ID = temp.Room_ID;

                    String fileName = System.IO.Path.GetFileName(filePath);

                    foob.Send_File_Message_To_Room("\'" + fileName + "\' has been uploaded. Click to download", Current_User_ID, Current_Room_ID, file);

                    MessageBox.Show("File has been uploaded");

                    Refresh_Message_Line();
                }
                else
                {
                    MessageBox.Show("Error, file size exceeded maximum size");
                }
        
            }
            else
            {
                MessageBox.Show("No valid file was selected");
            }
        }

        private void Upload_Private_Button_Click(object sender, RoutedEventArgs e)
        {
            string filePath;

            // Get the user to select a file.
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = "C:\\";
            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 4;

            if (openFileDialog.ShowDialog() == true)
            {

                filePath = openFileDialog.FileName;

                // We must conver the file to a byte array


                byte[] file = fileToByteArray(filePath);

                if (file.Length < 65536)
                {

                    int index = User_List_Box.SelectedIndex;
                    string Target_Username = (User_List_Box.Items[index]).ToString();//Obtaining the target username
                    Private_Target_User_ID = (foob.Find_By_Username(Target_Username));//Obtaining target's user ID
                    string Current_Username = foob.Find_Username_By_ID(Current_User_ID);//Obtaining the Current Username
                    string Private_Room_Name = Create_Private_Chat_Name(Current_Username, Target_Username);//This creates a private room name unique to only these two users.

                    Private_Room_ID = foob.Find_By_Room_Name(Private_Room_Name);

                    String fileName = System.IO.Path.GetFileName(filePath);

                    foob.Send_File_Message_To_Room("\'" + fileName + "\' has been uploaded. Click to download", Current_User_ID, Private_Room_ID, file);

                    MessageBox.Show("File has been uploaded");

                    Refresh_Private_Message_Line();
                }
                else
                {
                    MessageBox.Show("Error, file size exceeded maximum size");
                }

            }
            else
            {
                MessageBox.Show("No valid file was selected");
            }
        }

        //Refreshing ListViews
        private void Refresh_Chat_Room_List()
        {
            List<RoomObject> Room_List = foob.View_All_Chat_Rooms();
            Room_List.Reverse();
            Chat_Room_List.ItemsSource = Room_List;
        }

        private void Refresh_User_List()
        {
            List<string> User_List = foob.View_All_Users();
            User_List_Box.ItemsSource = User_List;
        }

        private void Refresh_Message_Line()
        {
            List<MessageObject> Message_Line = foob.Update_Message_Line(Current_Room_ID);
            Message_Line_List.ItemsSource = Message_Line;
        }

        private void Refresh_Private_Message_Line()
        {
            List<MessageObject> Private_Messages = foob.Update_Message_Line(Private_Room_ID);
            Private_Message_Line.ItemsSource = Private_Messages;
        }
        //Refreshing ListViews


        //Sends a message in the chat room
        private void Send_Message_Click(object sender, RoutedEventArgs e)
        {
            string message = Enter_Message_Box.Text;
            byte[] files = new byte[100];

            // If it is not empty
            if (!string.IsNullOrEmpty(message))
            {
                string text = foob.Send_File_Message_To_Room(Enter_Message_Box.Text, Current_User_ID, Current_Room_ID, files);
                Refresh_Message_Line();

                // Clear message text
                Enter_Message_Box.Text = "";
            }
            else
            {
                MessageBox.Show("Please enter your message!");
            }
        }

        // The following function is to allow users to send messages using the enter button
        private void enter_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            string message = Enter_Message_Box.Text;
            byte[] files = new byte[100];

            if (Enter_Message_Box.IsFocused && e.Key.Equals(Key.Enter))
            {
                if(Enter_Message_Box.Text != "" || string.IsNullOrWhiteSpace(Enter_Message_Box.Text))
                {
                    string text = foob.Send_File_Message_To_Room(Enter_Message_Box.Text, Current_User_ID, Current_Room_ID, files);
                    Refresh_Message_Line();

                    // Clear message text
                    Enter_Message_Box.Text = "";
                }
            }
        }

        //The following functions are for the sole purpose of having placeholder text
        private void Username_Text_LostFocus(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(Username_Text.Text))
            {
                Username_Text.Visibility = Visibility.Collapsed;
                watermark_username.Visibility = Visibility.Visible;
            }
        }

        //The following functions are for the sole purpose of having placeholder text
        private void watermark_username_GotFocus(object sender, RoutedEventArgs e)
        {
            watermark_username.Visibility=Visibility.Collapsed;
            Username_Text.Visibility=Visibility.Visible;
            Username_Text.Focus();

        }

        //The following functions are for the sole purpose of having placeholder text
        private void Password_Text_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Password_Text.Text))
            {
                Password_Text.Visibility = Visibility.Collapsed;
                watermark_password.Visibility = Visibility.Visible;
            }
        }

        //The following functions are for the sole purpose of having placeholder text
        private void watermark_password_GotFocus(object sender, RoutedEventArgs e)
        {
            watermark_password.Visibility = Visibility.Collapsed;
            Password_Text.Visibility = Visibility.Visible;
            Password_Text.Focus();
        }

        //The following functions are for the sole purpose of having placeholder text
        private void New_Room_Text_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(New_Room_Text.Text))
            {
                New_Room_Text.Visibility = Visibility.Collapsed;
                Watermark_New_Room.Visibility = Visibility.Visible;
            }
        }

        //The following functions are for the sole purpose of having placeholder text
        private void Watermark_New_Room_GotFocus(object sender, RoutedEventArgs e)
        {
            Watermark_New_Room.Visibility = Visibility.Collapsed;
            New_Room_Text.Visibility = Visibility.Visible;
            New_Room_Text.Focus();
        }

        //Changing chat rooms and message line you are currently in
        private void Change_Chat_Room(object sender, MouseButtonEventArgs e)
        {
            int index = Chat_Room_List.SelectedIndex;
            try
            {
                if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Released)
                {
                    RoomObject temp = (RoomObject)(Chat_Room_List.Items[index]);
                    Current_Room_ID = temp.Room_ID;

                    Refresh_Message_Line();
                }
            }
            catch(ArgumentOutOfRangeException es)//For some strange reason, the index always starts as -1 when selecting an item in the listview, so this exception is to catch that.
            {

            }
            catch(Exception ex)
            {

            }
        }


        private byte[] fileToByteArray(string filePath)
        {
            byte[] byteArray = System.IO.File.ReadAllBytes(filePath);
            return byteArray;
        }

        private bool ValidImage(byte[] data)
        {
            bool isValid = false;

            try
            {
                System.Drawing.Image img;

                MemoryStream ms = new MemoryStream(data);
                img = System.Drawing.Image.FromStream(ms);
        
                isValid = true;
            }
            catch (ArgumentOutOfRangeException e)
            {
                MessageBox.Show(e.Message);
                isValid = false;
            }

            return isValid;
        }


        private void SaveAsImage(byte[] data)
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = "JPG (*.JPG)|*.jpg";
            System.Drawing.Image img;
            MemoryStream ms = new MemoryStream(data);
            img = System.Drawing.Image.FromStream(ms);

            if (sf.ShowDialog() == true)
            {
                img.Save(sf.FileName);
            }
        }

        private void SaveAsText(byte[] data)
        {
            string s = Encoding.UTF8.GetString(data);
            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";

            if (sf.ShowDialog() == true)
            {
                System.IO.File.WriteAllText(sf.FileName, s);
            }
        }

        private void ByteArrayTofile(byte[] data)
        {

            try
            {
               
                SaveAsText(data);
            }
            catch (UnauthorizedAccessException e)
            {
                MessageBox.Show(e.Message);

            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message);

            }
            catch (ArgumentOutOfRangeException e)
            {
                MessageBox.Show(e.Message);
            }
            catch (ArgumentException e)
            {
                MessageBox.Show(e.Message);
            }

        }

        //Changing private chat line and creates a private chat line if one hasn't already been made
        private void Change_User(object sender, MouseButtonEventArgs e)
        {
            int index = User_List_Box.SelectedIndex;

            try
            {
                if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Released)
                {
                    string Target_Username = (User_List_Box.Items[index]).ToString();//Obtaining the target username
                    Private_Target_User_ID = (foob.Find_By_Username(Target_Username));//Obtaining target's user ID
                    string Current_Username = foob.Find_Username_By_ID(Current_User_ID);//Obtaining the Current Username
                    string Private_Room_Name = Create_Private_Chat_Name(Current_Username, Target_Username);//This creates a private room name unique to only these two users.
                    string error = foob.Create_Chat_Room(Private_Room_Name, True);//This will only create a new private room if the user hasn't already made one

                    Private_Room_ID = foob.Find_By_Room_Name(Private_Room_Name);

                    Refresh_Private_Message_Line();
                }
            }
            catch (ArgumentOutOfRangeException es)//For some strange reason, the index always starts as -1 when selecting an item in the listview, so this exception is to catch that.
            {

            }
            catch (Exception ex)
            {

            }
        }
     
        private string Create_Private_Chat_Name(string User, string target)
        {
            int result = string.Compare(User, target);
            string new_name = "";
            if (result == -1)
            {
                new_name = (User + ":" + target);

            }
            else
            {
                new_name = (target + ":" + User);
            }
            return new_name;
        }

        private void Send_Private_Message_Click(object sender, RoutedEventArgs e)
        {
            string message = Enter_Private_Message_Box.Text;
            byte[] files = new byte[100];

            // If it is not empty
            if (!string.IsNullOrEmpty(message))
            {
                string text = foob.Send_File_Message_To_Room(Enter_Private_Message_Box.Text, Current_User_ID, Private_Room_ID, files);
                Refresh_Private_Message_Line();
                Enter_Private_Message_Box.Text = "";
            }
            else
            {
                MessageBox.Show("Please enter your message!");
            }
        }

        private void enter_KeyDown_Private(object sender, System.Windows.Input.KeyEventArgs e)
        {
            string message = Enter_Private_Message_Box.Text;
            byte[] files = new byte[100];

            if (Enter_Private_Message_Box.IsFocused && e.Key.Equals(Key.Enter))
            {
                if (Enter_Private_Message_Box.Text != "" || string.IsNullOrWhiteSpace(Enter_Private_Message_Box.Text))
                {
                    string text = foob.Send_File_Message_To_Room(Enter_Private_Message_Box.Text, Private_Target_User_ID, Private_Room_ID, files);
                    Refresh_Private_Message_Line();

                    // Clear message text
                    Enter_Private_Message_Box.Text = "";
                }
            }
        }

        private void Download_File_Click(object sender, MouseButtonEventArgs e)
        {
            int index = Message_Line_List.SelectedIndex;
            try
            {
                if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Released)
                {
                    MessageObject temp = (MessageObject)(Message_Line_List.Items[index]);

                    string substring = "Click to download";

                    if(temp.Message.Contains(substring)) 
                    {
                        byte[] file_obj = temp.File;

                        ByteArrayTofile(file_obj);
                    }
                }
            }
            catch (ArgumentOutOfRangeException es)//For some strange reason, the index always starts as -1 when selecting an item in the listview, so this exception is to catch that.
            {

            }
            catch (Exception ex)
            {

            }

        }
        private void Download_File_Click_Private(object sender, MouseButtonEventArgs e)
        {
            int index = Private_Message_Line.SelectedIndex;
            try
            {
                if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Released)
                {
                    MessageObject temp = (MessageObject)(Private_Message_Line.Items[index]);
                    string substring = "Click to download";

                    if (temp.Message.Contains(substring))
                    {
                        byte[] file_obj = temp.File;

                        ByteArrayTofile(file_obj);
                    }
                }
            }
            catch (ArgumentOutOfRangeException es)//For some strange reason, the index always starts as -1 when selecting an item in the listview, so this exception is to catch that.
            {

            }
            catch (Exception ex)
            {

            }

        }
    }
}
