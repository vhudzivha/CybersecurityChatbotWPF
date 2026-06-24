using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; // Make sure this is present
using System.Text; // Make sure this is present

namespace CybersecurityChatbotWPF
{

    public class Chatbot
    {

        // Changed to internal for easier access from MainWindow.xaml.cs for state checking
        // A more robust solution would be to expose methods or properties for state management.
        private int quizScore = 0;

        private int totalQuestions = 0;
        internal Dictionary<string, string> userMemory = new Dictionary<string, string>();
        private string currentTopic = "";
        private List<string> conversationHistory = new List<string>();
        private List<ActivityLogEntry> activityLog = new List<ActivityLogEntry>();
        private List<QuizQuestion> quizQuestions = new List<QuizQuestion>();
        private int currentQuizQuestionIndex = 0;
        private int correctAnswers = 0;
        private bool inQuizMode = false;

        // Event to send messages to the UI
        public event EventHandler<string> BotResponseReady;
        public event EventHandler QuizEnded;
        public event EventHandler RequestUserInput; // For scenarios where the bot needs specific input (e.g., task title)

        private Dictionary<string, string[]> generalResponses = new Dictionary<string, string[]>
        {
            { "how are you?", new string[] { "I'm a bot, so I don't have feelings, but I'm here to help!", "I'm functioning optimally.", "Ready to assist you!" } },
            { "what's your purpose?", new string[] { "I provide cybersecurity tips to keep you safe online.", "My purpose is to raise awareness about cybersecurity threats and best practices.", "I'm here to educate you on how to protect yourself in the digital world." } },
            { "what can i ask you about?", new string[] { "You can ask me about password safety, phishing, safe browse, malware, social engineering, and more.", "I can provide information on various cybersecurity topics.", "Feel free to ask me anything related to online security." } },
            { "exit", new string[] { "Goodbye! Stay safe online.", "Thank you for chatting. Be secure!", "Have a safe digital experience!" } }
        };

        private Dictionary<string, string[]> cybersecurityResponses = new Dictionary<string, string[]>
        {
            { "password", new string[] {
                "Make sure to use strong, unique passwords for each account. Avoid using personal details in your passwords.",
                "A strong password should include a mix of uppercase and lowercase letters, numbers, and symbols.",
                "Consider using a password manager to securely store and generate complex passwords.",
                "Enable two-factor authentication (2FA) whenever possible for an extra layer of security on your accounts.",
                "Change your passwords regularly, especially for critical accounts."
            }},
            { "phishing", new string[] {
                "Be cautious of emails, messages, or calls asking for personal information, login credentials, or financial details. Scammers often disguise themselves as trusted organizations.",
                "Never click on suspicious links or download attachments from unknown senders. Verify the sender's authenticity through official channels.",
                "Pay attention to the sender's email address. Look for unusual spellings or domains that don't match the legitimate organization.",
                "If something seems too good to be true (like winning a lottery you didn't enter), it's likely a phishing attempt.",
                "Hover your mouse over links before clicking to see the actual URL. Be wary of URLs that look unfamiliar or use misleading subdomains."
            }},
            { "safe browse", new string[] {
                "Keep your web browser and its extensions up to date to benefit from the latest security patches.",
                "Avoid visiting suspicious or unverified websites. Look for the 'HTTPS' and a padlock icon in the address bar, indicating a secure connection.",
                "Be cautious about downloading files or installing software from untrusted sources.",
                "Use a reputable antivirus and anti-malware software and keep it updated.",
                "Consider using browser extensions that enhance privacy and security, such as ad blockers and tracker blockers."
            }},
            { "malware", new string[] {
                "Malware is malicious software that can harm your device and steal your information. Be cautious of suspicious downloads and links.",
                "Install and regularly update antivirus and anti-malware software to protect your system.",
                "Avoid opening email attachments from unknown senders.",
                "Be wary of software offered for free from unofficial websites.",
                "Regularly scan your system for malware."
            }},
            { "social engineering", new string[] {
                "Social engineering involves manipulating people into divulging confidential information. Be skeptical of unexpected requests for personal details.",
                "Never share your passwords or sensitive information with anyone you don't trust, especially over the phone or email.",
                "Be wary of individuals claiming to be from technical support asking for remote access to your computer.",
                "Verify the identity of anyone asking for sensitive information through official channels.",
                "Educate yourself about common social engineering tactics."
            }}
        };

        public Chatbot()
        {
            InitializeQuizQuestions();
            // Indicate that the bot is awaiting the user's name initially
            userMemory["awaitingName"] = "true";
        }

        private void InitializeQuizQuestions()
        {
            quizQuestions.Add(new QuizQuestion
            {
                Question = "What should you do if you receive an email asking for your password?",
                Options = new string[] { "A) Reply with your password", "B) Delete the email", "C) Report the email as phishing", "D) Ignore it" },
                CorrectAnswer = "C",
                Explanation = "Reporting the email helps prevent future phishing attempts."
            });
            quizQuestions.Add(new QuizQuestion
            {
                Question = "True or False: Using the same password for all your accounts is a good security practice.",
                Options = new string[] { "A) True", "B) False" },
                CorrectAnswer = "B",
                Explanation = "Using unique passwords for each account is crucial for security. If one account is compromised, others remain safe."
            });
            quizQuestions.Add(new QuizQuestion
            {
                Question = "Which of the following is an example of strong password?",
                Options = new string[] { "A) 123456", "B) Password123", "C) MyDogSpot", "D) L@zyC@t$3cUr3!" },
                CorrectAnswer = "D",
                Explanation = "Strong passwords use a combination of uppercase, lowercase, numbers, and symbols."
            });
            quizQuestions.Add(new QuizQuestion
            {
                Question = "What does '2FA' stand for in cybersecurity?",
                Options = new string[] { "A) Two-Factor Authorization", "B) Two-Factor Authentication", "C) Double-Factor Access", "D) Dual-Factor Assurance" },
                CorrectAnswer = "B",
                Explanation = "2FA, or Two-Factor Authentication, adds an extra layer of security to your accounts."
            });
            quizQuestions.Add(new QuizQuestion
            {
                Question = "What is malware?",
                Options = new string[] { "A) A type of computer hardware", "B) Malicious software designed to harm or exploit computer systems", "C) A programming language", "D) A type of online game" },
                CorrectAnswer = "B",
                Explanation = "Malware is designed to disrupt, damage, or gain unauthorized access to computer systems."
            });
            quizQuestions.Add(new QuizQuestion
            {
                Question = "True or False: It's safe to click on any link in an email as long as it looks legitimate.",
                Options = new string[] { "A) True", "B) False" },
                CorrectAnswer = "B",
                Explanation = "Always hover over links to check the URL before clicking, especially in suspicious emails."
            });
            quizQuestions.Add(new QuizQuestion
            {
                Question = "Which of the following is a common social engineering tactic?",
                Options = new string[] { "A) Phishing", "B) Brute-force attacks", "C) Malware infection", "D) Denial-of-service attack" },
                CorrectAnswer = "A",
                Explanation = "Phishing is a common social engineering tactic that manipulates individuals into revealing sensitive information."
            });
            quizQuestions.Add(new QuizQuestion
            {
                Question = "What is the purpose of an antivirus program?",
                Options = new string[] { "A) To speed up your computer", "B) To protect against viruses and other malicious software", "C) To create documents", "D) To browse the internet" },
                CorrectAnswer = "B",
                Explanation = "Antivirus software helps detect, prevent, and remove malicious software from your computer."
            });
            quizQuestions.Add(new QuizQuestion
            {
                Question = "True or False: Public Wi-Fi networks are always secure for online banking.",
                Options = new string[] { "A) True", "B) False" },
                CorrectAnswer = "B",
                Explanation = "Public Wi-Fi networks are often unsecured and can be vulnerable to eavesdropping. It's best to avoid sensitive transactions on them."
            });
            quizQuestions.Add(new QuizQuestion
            {
                Question = "What is a firewall used for?",
                Options = new string[] { "A) To store files", "B) To protect a network from unauthorized access", "C) To print documents", "D) To play games" },
                CorrectAnswer = "B",
                Explanation = "A firewall acts as a barrier between your network and the internet, controlling incoming and outgoing network traffic."
            });
        }

        // Method to start the chatbot's initial interaction
        public void StartChatbot()
        {
            OnBotResponseReady("Hello! Welcome to your Cybersecurity Awareness Bot.");
            CheckReminders();
            OnBotResponseReady("You can ask me about cybersecurity topics like passwords, phishing, safe browse, malware, or social engineering.");
            OnBotResponseReady("You can also ask me to 'add task', 'show tasks', 'start quiz', or 'show activity log'.");
            OnBotResponseReady("Enter your name: ");
            // Requesting initial input (name) from the UI
            OnRequestUserInput();
        }

        // Main method to process user input from the UI
        public void ProcessUserInput(string question)
        {
            question = question.ToLower().Trim();

            if (string.IsNullOrWhiteSpace(question))
            {
                OnBotResponseReady("Please enter a valid question.");
                return;
            }

            conversationHistory.Add($"User: {question}");
            LogActivity($"User input: \"{question}\"");

            if (userMemory.ContainsKey("awaitingName"))
            {
                string name = question.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    OnBotResponseReady("Name cannot be empty. Please enter your name: ");
                    OnRequestUserInput();
                }
                else
                {
                    userMemory["name"] = name;
                    userMemory.Remove("awaitingName");
                    OnBotResponseReady($"Hello, {name}! Welcome to your Cybersecurity Awareness Bot.");
                    OnBotResponseReady("You can ask me about cybersecurity topics like passwords, phishing, safe browse, malware, or social engineering.");
                    OnBotResponseReady("You can also ask me to 'add task', 'show tasks', 'start quiz', or 'show activity log'.");
                    OnBotResponseReady("What cybersecurity topic are you most interested in? ");
                    OnRequestUserInput(); // Requesting favorite topic
                    userMemory["awaitingFavoriteTopic"] = "true";
                }
                return;
            }
            if (userMemory.ContainsKey("awaitingFavoriteTopic"))
            {
                if (question == "start quiz")
                {
                    userMemory.Remove("awaitingFavoriteTopic");
                    StartQuiz();
                    return;
                }

                OnBotResponseReady($"Great! I'll remember that you're interested in {question}.");
                userMemory.Remove("awaitingFavoriteTopic");
                return;
            }
            if (inQuizMode)
            {
                ProcessQuizAnswer(question);
                return;
            }

            bool found = false;

            if (question.Contains("save conversation"))
            {
                SaveConversation();

                found = true;
            }
            // Check for Task Assistant commands
            if (question.Contains("add task") || question.Contains("create task") || question.Contains("new task"))
            {
                AddTaskPrompt();
                found = true;
            }
            else if (question.Contains("show tasks") || question.Contains("list tasks") || question.Contains("what are my tasks"))
            {
           
                DatabaseHelper db = new DatabaseHelper();
                OnBotResponseReady(db.ShowTasks());
                found = true;
            }
            else if (question.Contains("complete task")
          || question.Contains("mark task complete"))
            {
                OnBotResponseReady(
                    "This feature is under development.");

                found = true;
            }
            else if (question.Contains("delete task") || question.Contains("remove task"))
            {
                DeleteTaskPrompt();
                found = true;
            }
            // Check for Quiz commands
            else if (question.Contains("start quiz") || question.Contains("begin quiz") || question.Contains("quiz me"))
            {
                StartQuiz();
                found = true;
            }
            // Check for Activity Log commands
            else if (question.Contains("show activity log") || question.Contains("what have you done") || question.Contains("activity history"))
            {
                DisplayActivityLog();
                found = true;
            }
            // Check for general conversation history
            else if (question.Contains("conversation history") || question.Contains("chat history"))
            {
                DisplayConversationHistory();
                found = true;
            }
            // Existing cybersecurity and general responses
            if (!found)
            {
                foreach (var keywordResponsePair in cybersecurityResponses)
                {
                    if (question.Contains(keywordResponsePair.Key))
                    {
                        currentTopic = keywordResponsePair.Key;
                        Random random = new Random();
                        string response = keywordResponsePair.Value[random.Next(keywordResponsePair.Value.Length)];
                        OnBotResponseReady(response);
                        conversationHistory.Add($"Bot: {response}");
                        LogActivity($"Provided info on {keywordResponsePair.Key}.");
                        found = true;
                        break;
                    }
                }
            }
            if (!found)
            {
                foreach (var generalResponsePair in generalResponses)
                {
                    if (question == generalResponsePair.Key)
                    {
                        currentTopic = generalResponsePair.Key;
                        Random random = new Random();
                        string response = generalResponsePair.Value[random.Next(generalResponsePair.Value.Length)];
                        OnBotResponseReady(response);
                        conversationHistory.Add($"Bot: {response}");
                        LogActivity($"Responded to general query: \"{generalResponsePair.Key}\".");
                        found = true;
                        if (generalResponsePair.Key == "exit")
                        {
                            SaveConversation();
                            // In WPF, exiting means closing the window, handled by UI.
                        }
                        break;
                    }
                }
            }
            if (!found && !string.IsNullOrEmpty(currentTopic))
            {
                if (question.Contains("more") || question.Contains("details") || question.Contains("explain"))
                {
                    string additionalInfo = "";
                    switch (currentTopic)
                    {
                        case "password":
                            additionalInfo = "For more details on password safety, consider using a passphrase, which is a long sentence that's easy to remember but hard to guess. Also, enable multi-factor authentication whenever possible for an extra layer of security.";
                            break;
                        case "phishing":
                            additionalInfo = "To further protect yourself from phishing, be extremely cautious of any communication that creates a sense of urgency or requires immediate action. Always verify requests through official and independent channels.";
                            break;
                        case "safe browse":
                            additionalInfo = "When Browse safely, ensure your firewall is enabled and configured correctly. Be wary of accepting security certificates from untrusted sources.";
                            break;
                        case "malware":
                            additionalInfo = "To prevent malware infections, be careful about the software you install, even if it seems legitimate. Read reviews and download from official sources only.";
                            break;
                        case "social engineering":
                            additionalInfo = "Remember that social engineers often exploit human emotions like fear or greed. Take your time to think before acting on any unusual request.";
                            break;
                        default:
                            break;
                    }
                    OnBotResponseReady(additionalInfo);
                    conversationHistory.Add($"Bot: {additionalInfo}");
                    LogActivity($"Provided more details on {currentTopic}.");
                    found = true;
                }
            }
            if (!found && question.Contains("remember") && userMemory.ContainsKey("name"))
            {
                string memoryResponse = $"Yes, {userMemory["name"]}, I remember you!";
                OnBotResponseReady(memoryResponse);
                conversationHistory.Add($"Bot: {memoryResponse}");
                LogActivity($"Affirmed remembering user's name.");
                found = true;
            }
            if (!found && question.Contains("interested in") && userMemory.ContainsKey("favoriteTopic"))
            {
                string favoriteTopicResponse = $"Since you are interested in {userMemory["favoriteTopic"]}, here's another tip related to it...";
                OnBotResponseReady(favoriteTopicResponse);
                conversationHistory.Add($"Bot: {favoriteTopicResponse}");
                LogActivity($"Provided tip related to user's favorite topic: {userMemory["favoriteTopic"]}.");
                switch (userMemory["favoriteTopic"])
                {
                    case "password":
                        OnBotResponseReady("Consider using a password strength checker tool online to evaluate the robustness of your passwords.");
                        conversationHistory.Add($"Bot: Consider using a password strength checker tool online to evaluate the robustness of your passwords.");
                        break;
                    case "phishing":
                        OnBotResponseReady("Be aware that phishing attempts can also occur via SMS (smishing) or phone calls (vishing).");
                        conversationHistory.Add($"Bot: Be aware that phishing attempts can also occur via SMS (smishing) or phone calls (vishing).");
                        break;
                    case "safe browse":
                        OnBotResponseReady("Regularly clear your browse history, cookies, and cache to protect your privacy.");
                        conversationHistory.Add($"Bot: Regularly clear your browse history, cookies and cache to protect your privacy.");
                        break;
                    case "malware":
                        OnBotResponseReady("Enable automatic updates for your operating system and applications to patch security vulnerabilities.");
                        conversationHistory.Add($"Bot: Enable automatic updates for your operating system and applications to patch security vulnerabilities.");
                        break;
                    case "social engineering":
                        OnBotResponseReady("Be cautious of sharing too much personal information on social media platforms, as this can be used for social engineering attacks.");
                        conversationHistory.Add($"Bot: Be cautious of sharing too much personal information on social media platforms.");
                        break;
                    default:
                        OnBotResponseReady("That's an interesting topic!");
                        conversationHistory.Add($"Bot: That's an interesting topic!");
                        break;
                }
                found = true;
            }
            if (!found && (question.Contains("worried") || question.Contains("concerned") || question.Contains("anxious")))
            {
                string empathyResponse = "It's completely understandable to feel that way. Cybersecurity can seem overwhelming, but I'm here to help you understand and stay safe. What specific concerns do you have?";
                OnBotResponseReady(empathyResponse);
                conversationHistory.Add($"Bot: {empathyResponse}");
                LogActivity($"Provided empathy response for user's concerns.");
                found = true;
            }
            else if (!found && (question.Contains("curious") || question.Contains("learn more") || question.Contains("tell me more")))
            {
                string curiosityResponse = "That's great that you're curious! Learning about cybersecurity is the first step to staying protected. What specifically are you interested in exploring further?";
                OnBotResponseReady(curiosityResponse);
                conversationHistory.Add($"Bot: {curiosityResponse}");
                LogActivity($"Encouraged user's curiosity.");
                found = true;
            }
            else if (!found && (question.Contains("frustrated") || question.Contains("confused") || question.Contains("difficult")))
            {
                string frustrationResponse = "I understand it can be frustrating. Let's take it one step at a time. What part are you finding difficult? I'll try to explain it more clearly or provide a different perspective.";
                OnBotResponseReady(frustrationResponse);
                conversationHistory.Add($"Bot: {frustrationResponse}");
                LogActivity($"Addressed user's frustration.");
                found = true;
            }

    
            else if (question.StartsWith("delete task "))
            {
                string idText =
                    question.Replace("delete task ", "");

                int id;

                if (int.TryParse(idText, out id))
                {
                    DatabaseHelper db =
                        new DatabaseHelper();

                    db.DeleteTask(id);

                    OnBotResponseReady(
                        "Task deleted successfully!");
                }
                else
                {
                    OnBotResponseReady(
                        "Please enter a valid task number.");
                }

                found = true;
            }

            else if (question == "show activity log")
            {
                DatabaseHelper db = new DatabaseHelper();

                string logs = db.ShowActivityLog();

                OnBotResponseReady("--- Activity Log ---");

                OnBotResponseReady(logs);

                OnBotResponseReady("--- End of Activity Log ---");

                found = true;
            }

            else if (question.StartsWith("add task "))
            {
                string task = question.Replace("add task ", "");

                DatabaseHelper db = new DatabaseHelper();

                db.AddTask(task);

                DatabaseHelper.SaveActivity(
                    question,
                    "Task added successfully!");

                OnBotResponseReady("Task added successfully!");

                found = true;
            }
            if (!found)
            {
                string unknownResponse = "I'm not sure I understand. Could you try rephrasing your question or asking about passwords, phishing, safe browse, malware, social engineering, tasks, or the quiz?";
                OnBotResponseReady(unknownResponse);
                conversationHistory.Add($"Bot: {unknownResponse}");
                LogActivity($"Responded with unknown command message.");
            }
        }

        private void LogActivity(string description)
        {
            activityLog.Add(new ActivityLogEntry(description));

            DatabaseHelper.SaveActivity(
                description,
                "Activity Recorded"
            );
        }
        private void DisplayActivityLog()
        {
            StringBuilder logOutput = new StringBuilder();
            logOutput.AppendLine("--- Activity Log ---");
            if (activityLog.Any())
            {
                var recentLogs = activityLog.Skip(Math.Max(0, activityLog.Count() - 10)).Take(10);
                foreach (var entry in recentLogs)
                {
                    logOutput.AppendLine($"- {entry.Timestamp:HH:mm:ss} - {entry.Description}");
                }
                if (activityLog.Count > 10)
                {
                    logOutput.AppendLine("... (more entries available, type 'show all activity log' to see more)");
                }
            }
            else
            {
                logOutput.AppendLine("No activities logged yet.");
            }
            logOutput.AppendLine("--- End of Activity Log ---");
            OnBotResponseReady(logOutput.ToString());
            LogActivity("Displayed activity log.");
        }

        private void AddTaskPrompt()
        {
            OnBotResponseReady("Okay, let's add a new cybersecurity task. What is the title of the task? (e.g., 'Update antivirus software'): ");
            userMemory["awaitingTaskTitle"] = "true";
            OnRequestUserInput(); // Signal UI to get task title
        }

        public void SetTaskTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                OnBotResponseReady("Task title cannot be empty. Task not added. Please enter a valid title:");
                OnRequestUserInput();
                return;
            }

            userMemory["currentTaskTitle"] = title;
            OnBotResponseReady("Do you want to set a reminder for this task? (yes/no): ");
            userMemory["awaitingReminderChoice"] = "true";
            OnRequestUserInput(); // Signal UI for reminder choice
        }

        public void SetReminderChoice(string choice)
        {
            string title = userMemory["currentTaskTitle"];
            userMemory.Remove("awaitingReminderChoice");

            if (choice.ToLower() == "yes")
            {
                OnBotResponseReady("When should I remind you? (e.g., 'tomorrow', 'in 3 days', '2025-12-25'): ");
                userMemory["awaitingReminderInput"] = "true";
                OnRequestUserInput(); // Signal UI for reminder date
            }
            else
            {
                DatabaseHelper db = new DatabaseHelper();

                db.AddTask(title);

                OnBotResponseReady($"Task '{title}' has been added successfully.");
                LogActivity($"Added task: \"{title}\" with no reminder.");
                userMemory.Remove("currentTaskTitle");
            }
        }

        private void CheckReminders()
        {
            if (userMemory.ContainsKey("currentTaskTitle"))
            {
                OnBotResponseReady(
                    "🔔 Remember to complete your cybersecurity task!");
            }
        }

        public void SetReminderDate(string reminderInput)
        {
            string title = userMemory["currentTaskTitle"];

            userMemory.Remove("awaitingReminderInput");
            userMemory.Remove("currentTaskTitle");

            DateTime? reminderDate = null;

            if (reminderInput.ToLower() == "tomorrow")
            {
                reminderDate = DateTime.Now.AddDays(1);
            }
            else if (DateTime.TryParse(reminderInput,
                                       out DateTime parsedDate))
            {
                reminderDate = parsedDate;
            }

            DatabaseHelper db =
                new DatabaseHelper();

            db.AddTask(title);

            OnBotResponseReady(
                $"Task '{title}' added successfully.");

            if (reminderDate.HasValue)
            {
                OnBotResponseReady(
                $"Reminder set for: {reminderDate.Value.ToShortDateString()}");
            }

            LogActivity(
            $"Added task '{title}' with reminder.");
        }



        private void DeleteTaskPrompt()
        {
            DatabaseHelper db = new DatabaseHelper();

            OnBotResponseReady(db.ShowTasks());

            OnBotResponseReady(
                "Type: delete task 1");

            OnRequestUserInput();
      
            OnBotResponseReady("Enter the number of the task to delete: ");
            userMemory["awaitingTaskDeletionNumber"] = "true";
            OnRequestUserInput();
        }





        // Starts cybersecurity quiz
        private void StartQuiz()
        {
            if (!quizQuestions.Any())
            {
                OnBotResponseReady("No quiz questions available. Please check the chatbot's configuration.");
                return;
            }
            inQuizMode = true;
            quizScore = 0;

            totalQuestions = quizQuestions.Count;

            currentQuizQuestionIndex = 0;

            correctAnswers = 0;
            OnBotResponseReady("Starting the Cybersecurity Quiz! Answer the following questions. Type 'exit quiz' to stop at any time.");
            LogActivity("Started quiz.");
            AskNextQuizQuestion();
        }

        private void AskNextQuizQuestion()
        {
            if (currentQuizQuestionIndex < quizQuestions.Count)
            {
                QuizQuestion currentQuestion = quizQuestions[currentQuizQuestionIndex];
                StringBuilder questionText = new StringBuilder();
                questionText.AppendLine($"Question {currentQuizQuestionIndex + 1}: {currentQuestion.Question}");
                foreach (string option in currentQuestion.Options)
                {
                    questionText.AppendLine(option);
                }
                OnBotResponseReady(questionText.ToString());
            }
            else
            {
                EndQuiz();
            }
        }

        private void ProcessQuizAnswer(string userAnswer)
        {
            if (userAnswer.ToLower() == "exit quiz")
            {
                EndQuiz(true);
                return;
            }
            QuizQuestion currentQuestion = quizQuestions[currentQuizQuestionIndex];
            string correctAnswer = currentQuestion.CorrectAnswer.ToLower();

            if (userAnswer.ToUpper() == currentQuestion.CorrectAnswer.ToUpper())
            {
                quizScore++;

                correctAnswers++;

                OnBotResponseReady("✅ Correct!");

                OnBotResponseReady(currentQuestion.Explanation);

                LogActivity($"Answered quiz question {currentQuizQuestionIndex + 1} correctly.");
            }
            else

            {
                OnBotResponseReady("❌ Incorrect!");

                OnBotResponseReady(
                    "Correct answer: " +
                    currentQuestion.CorrectAnswer);

                OnBotResponseReady(
                    currentQuestion.Explanation);

                LogActivity($"Answered quiz question {currentQuizQuestionIndex + 1} incorrectly.");
            }
            currentQuizQuestionIndex++;
            AskNextQuizQuestion();
        }

        private void EndQuiz(bool prematurely = false)
        {
            inQuizMode = false;
            StringBuilder quizResult = new StringBuilder();
            if (prematurely)
            {
                quizResult.AppendLine("Quiz ended prematurely.");
            }
            quizResult.AppendLine("");

            quizResult.AppendLine(
            "========== QUIZ RESULTS ==========");

            quizResult.AppendLine(
            $"Score: {quizScore}/{totalQuestions}");

        

            quizResult.AppendLine("");

            double percentage = 0;

            if (currentQuizQuestionIndex > 0)
            {
                percentage =
                (double)correctAnswers
                / currentQuizQuestionIndex * 100;
            }
            quizResult.AppendLine(
        $"Percentage: {percentage:F0}%");
            if (double.IsNaN(percentage)) // Handle division by zero if no questions were answered
            {
                percentage = 0;
            }
            if (percentage >= 80)
            {
                quizResult.AppendLine("Excellent! You have a great understanding of cybersecurity.");
            }
            else if (percentage >= 50)
            {
                quizResult.AppendLine("Good effort! Keep learning to improve your cybersecurity knowledge.");
            }
            else
            {
                quizResult.AppendLine("You might want to review some cybersecurity basics. I'm here to help!");
            }
            OnBotResponseReady(quizResult.ToString());
            LogActivity($"Finished quiz. Score: {correctAnswers}/{currentQuizQuestionIndex}.");
            currentQuizQuestionIndex = 0;
            correctAnswers = 0;
            QuizEnded?.Invoke(this, EventArgs.Empty); // Notify UI that quiz ended
        }

        private void SaveConversation()
        {
            try
            {
                string filePath = "conversation.txt";
                File.WriteAllLines(filePath, conversationHistory);
                OnBotResponseReady($"Conversation saved to {filePath}");
            }
            catch (Exception ex)
            {
                OnBotResponseReady($"Error saving conversation: {ex.Message}");
            }
        }

        private void DisplayConversationHistory()
        {
            StringBuilder historyOutput = new StringBuilder();
            historyOutput.AppendLine("--- Conversation History ---");
            foreach (string line in conversationHistory)
            {
                historyOutput.AppendLine(line);
            }
            historyOutput.AppendLine("--- End of History ---");
            OnBotResponseReady(historyOutput.ToString());
            LogActivity("Displayed conversation history.");
        }

        // Helper method to raise the BotResponseReady event
        protected virtual void OnBotResponseReady(string message)
        {
            BotResponseReady?.Invoke(this, message);
        }

        // Helper method to raise the RequestUserInput event
        protected virtual void OnRequestUserInput()
        {
            RequestUserInput?.Invoke(this, EventArgs.Empty);
        }

        // Nested classes for Activity Log, Cybersecurity Task, and Quiz Question
        public class ActivityLogEntry
        {
            public DateTime Timestamp { get; private set; }
            public string Description { get; private set; }
            public ActivityLogEntry(string description)
            {
                Timestamp = DateTime.Now;
                Description = description;
            }
        }

        public class CybersecurityTask
        {
            public string Title { get; private set; }
            public DateTime? ReminderDate { get; private set; }
            public string ReminderDescription { get; private set; }
            public bool IsCompleted { get; private set; }
            public CybersecurityTask(string title, DateTime? reminderDate = null, string reminderDescription = null)
            {
                Title = title;
                ReminderDate = reminderDate;
                ReminderDescription = reminderDescription;
                IsCompleted = false;
            }
            public void MarkComplete()
            {
                IsCompleted = true;
            }
        }

        public class QuizQuestion
        {
            public string Question { get; set; }
            public string[] Options { get; set; }
            public string CorrectAnswer { get; set; } // Store as "A", "B", "C", "D"
            public string Explanation { get; set; }
        }
    }
}