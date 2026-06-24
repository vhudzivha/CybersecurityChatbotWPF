# Cybersecurity Awareness Chatbot (WPF)

## Overview

The Cybersecurity Awareness Chatbot is a desktop application developed using C# and WPF. The chatbot educates users about important cybersecurity topics and provides interactive features such as quizzes, task management, conversation history, and activity logging.

The application is designed to improve cybersecurity awareness while demonstrating object-oriented programming, file handling, database integration, and graphical user interface development.

---

## Features

### Cybersecurity Topics

The chatbot provides information about:

* Password Safety
* Phishing Attacks
* Safe Browsing
* Malware
* Social Engineering
* Two-Factor Authentication (2FA)

### Interactive Quiz

* Multiple choice cybersecurity quiz
* Immediate feedback for each answer
* Score calculation
* Percentage score at the end

### Task Assistant

Users can:

* Add tasks
* View tasks
* Delete tasks

### Activity Log

* Records chatbot activities
* Stores activities in SQLite database
* Displays recent activity history

### Conversation History

* Stores chat messages
* Displays previous conversations
* Exports conversations to a TXT file

### User Memory

The chatbot remembers:

* User name
* Favourite cybersecurity topic

---

## Technologies Used

* C#
* WPF (Windows Presentation Foundation)
* SQLite Database
* XAML
* .NET

---

## Database Tables

### ActivityLog

| Field       | Type    |
| ----------- | ------- |
| Id          | Integer |
| UserMessage | Text    |
| BotResponse | Text    |
| TimeStamp   | Text    |

### Tasks

| Field    | Type    |
| -------- | ------- |
| Id       | Integer |
| TaskName | Text    |

---

## Screenshots

### Main Interface

* Chat area
* User input textbox
* Send button
* Cybersecurity logo

---

## How to Run

1. Open the solution in Visual Studio.
2. Restore NuGet packages.
3. Build the project.
4. Run the application.
5. Start chatting with the Cybersecurity Chatbot.

---

## Example Commands

* start quiz
* add task
* show tasks
* delete task
* show activity log
* conversation history
* save conversation

---
youtube video link/demostration

https://youtu.be/1UFeV9xl6rY

---

## Author

**Madoba Sunnyboy**

Cybersecurity Awareness Chatbot
Part 3 Final Project
