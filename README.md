🗂️ WorkTrack – Task & Project Management System

WorkTrack is a role-based Task and Project Management System built using ASP.NET Core with MVC architecture.
The system helps organizations manage projects, assign and track tasks, and evaluate employee performance by comparing estimated vs actual task duration.

🚀 Features
🔐 User Roles

The system supports two roles:

👨‍💼 Admin

Create, update, and delete projects

View all employee profiles

View all tasks across projects

Analyze task estimation accuracy by comparing estimated duration with actual duration

👨‍💻 Employee

Secure login using JWT authentication

Create and manage personal tasks within assigned projects

Update task progress:

Start Date

End Date

Automatic task status update (Pending → Completed)

View task estimation accuracy

Update profile details (excluding email & password)

Change password separately

🧩 Task Workflow

Task Creation

Tasks are created under specific projects

Default task status: Pending

Each task includes:

Title

Description

Estimated Duration (in days)

Task Management

Task listing, updating, and deletion implemented using Kendo UI

On task start → Start Date is set

On task completion → End Date is set

Status automatically changes to Completed

Task Accuracy Calculation

Actual Duration = End Date - Start Date

Estimation Accuracy is calculated by comparing:

Actual Duration vs Estimated Duration

👤 Profile Management

Employees can:

View and update profile details

Upload a profile image

Email and password updates are restricted

Separate password change functionality is provided


🛠️ Tech Stack

Backend: ASP.NET Core (.NET)

Architecture: MVC

Frontend: Razor Views + Kendo UI

Authentication: JWT (JSON Web Token)

Database: Relational Database (SQL-based)

UI: Simple, clean MVC-based interface

📊 Key Highlights

Role-based access control (Admin & Employee)

Real-time task tracking and duration calculation

Task estimation accuracy analysis

Clean separation of concerns using MVC

Secure authentication with JWT

Scalable project-task-user relationship design

📌 Use Cases

Internal employee task tracking

Project progress monitoring

Performance evaluation based on estimation accuracy

Learning-oriented enterprise application using ASP.NET MVC




🧠 Prepare interview explanation (Admin flow, JWT flow, MVC flow)

🗃️ Draw ER diagram / system architecture explanation
