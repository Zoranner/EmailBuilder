namespace Zoranner.Upgrader
{
    public class Interface
    {
        private static Interface _Instance;

        public MainWindow MainWindow;

        private Interface()
        {
        }

        public static Interface Instance => _Instance ?? (_Instance = new Interface());

        //public static void ShowMessage(string message, MessageFormType type)
        //{
        //    MessageForm messageForm = new MessageForm(message, type);
        //    messageForm.ShowDialog();
        //}
    }
}