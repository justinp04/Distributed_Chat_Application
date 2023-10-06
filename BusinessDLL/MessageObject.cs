using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MessageObject
{
    public int Message_ID { get; set; }
    public int Room_ID { get; set; }
    public int User_ID { get; set; }
    public string Message { get; set; }
    public byte[] File { get; set; }
}