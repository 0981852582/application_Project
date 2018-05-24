namespace WebApplication7.Models
{
    public class Message
    {
        public object result { get; set; }
        public string Title { get; set; }
        public bool Error { get; set; }
    }
    public class MessageError
    {
        public static string MessageNotPermission = "Tài khoản không có quyền thực hiện chức năng này.";
    }
}
