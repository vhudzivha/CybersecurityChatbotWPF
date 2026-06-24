using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Media;
using System.IO;

namespace CybersecurityChatbotWPF
{
    public partial class MainWindow : Window
    {
        private Chatbot chatbot;

        public MainWindow()
        {
            InitializeComponent();

            // Create database and table
            DatabaseHelper.InitializeDatabase();

            chatbot = new Chatbot();

            // Subscribe to chatbot events
            chatbot.BotResponseReady += Chatbot_BotResponseReady;
            chatbot.QuizEnded += Chatbot_QuizEnded;
            chatbot.RequestUserInput += Chatbot_RequestUserInput;

            // Play greeting audio
            PlayGreetingAudio();

            // Start chatbot
            chatbot.StartChatbot();

            UserInputTextBox.Focus();
        }

        private void PlayGreetingAudio()
        {
            try
            {
                string audioFilePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Audio",
                    "greeting.wav");

                if (File.Exists(audioFilePath))
                {
                    SoundPlayer player = new SoundPlayer(audioFilePath);
                    player.Load();
                    player.Play();
                }
            }
            catch (Exception ex)
            {
                ChatDisplay.Text +=
                    "Bot: Error playing greeting audio: "
                    + ex.Message + Environment.NewLine;
            }
        }

        private void Chatbot_BotResponseReady(object sender, string message)
        {
            Dispatcher.Invoke(() =>
            {
                ChatDisplay.Text += "Bot: " + message + Environment.NewLine;

                ((ScrollViewer)ChatDisplay.Parent).ScrollToEnd();
            });
        }

        private void Chatbot_QuizEnded(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UserInputTextBox.IsEnabled = true;
                SendButton.IsEnabled = true;
                UserInputTextBox.Focus();
            });
        }

        private void Chatbot_RequestUserInput(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UserInputTextBox.Focus();
            });
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void UserInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        private void SendMessage()
        {
            string userMessage = UserInputTextBox.Text.Trim();

            UserInputTextBox.Clear();

            if (string.IsNullOrEmpty(userMessage))
                return;

            ChatDisplay.Text +=
                "You: " + userMessage + Environment.NewLine;

            ((ScrollViewer)ChatDisplay.Parent).ScrollToEnd();

            chatbot.ProcessUserInput(userMessage);
            DatabaseHelper.SaveActivity(
    userMessage,
    "User interacted with chatbot"
);

            UserInputTextBox.Focus();
        }
    }
}