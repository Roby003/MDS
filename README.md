# Table of Contents

1. [BoardBloom](#boardbloom)
2. [System Requirements](#system-requirements)
2. [Demo](#demo)
2. [Stories](#stories)
2. [App Design](#app-design)
2. [UML Diagram](#uml-diagram)
3. [Architecture Overview](#architecture-overview)
4. [Data Model](#data-model)
   <!-- - [User](#user)
   - [Bloom](#bloom)
   - [Board](#board)
   - [Comment](#comment)
   - [Like](#like) -->
5. [Controllers](#controllers)
   <!-- - [UserController](#usercontroller)
   - [BloomController](#bloomcontroller)
   - [BoardController](#boardcontroller)
   - [CommentController](#commentcontroller) -->
6. [Views](#views)
8. [Data Access Layer](#data-access-layer)
9. [Security](#security)
10. [Deployment](#deployment)
11. [Testing](#testing)
11. [AI use](#ai-used-to-help-with-development)


# BoardBloom

With BoardBloom, users can create, share, and find "Blooms," or visual material on the web. Users get the option to save Blooms they like on their boards. Boards work as topical collections that let users group their Blooms corresponding to projects or interests. A user may create distinct boards, for instance, for dinner ideas, cars or haircuts. The main forms of engagement on the platform are creating boards and Blooms, while liking and commenting are supported as well.

# Demo

# System Requirements

- .NET 6.0 SDK
- Visual Studio 2022 or later
- SQL Server or any compatible database system
- A web browser (latest versions of Chrome, Firefox, or Edge)

# Stories

1. *As an administrator, I want to create, view, edit, and delete categories directly from the application interface, enabling flexible content organization.*

2. *As a visitor (registered or not), I want to view all bookmarks added to the platform, allowing me to explore content without restrictions.*

3. *As a user, I want to see the most recent and popular posts on the main page for better navigation.*

4. *As a registered user, I need to add posts with title, description, and embedded media (text, images, and videos) to the platform, expanding the content available to the community.*
   
5. *As a registered user, I want to comment on for posts, edit and delete my comments, fostering an interactive and engaged community.*

6. *As a registered user I want to have the ability to like a post, and also to remove my like*

7. *As a registered user, I need to organize posts into bookmarks on my personal page, allowing me to personalize my experience and find content easily.*

8. *As an administrator, I am responsible for content moderation, including the ability to delete inappropriate bookmarks or comments, ensuring the platform remains a safe and welcoming space.*

9. *As an administrator, I want to ban users from the application interface, mainting organizing groups.*

10. *As a developer, I need to design a database schema that supports users, roles, posts, bookmarks, comments, categories, and search functionality, ensuring scalability and performance.*

# Backlog
The development of the app was monitored using Trello.The following screenshot shows the Trello board used to track the progress of the project during development.
![SS aici](/ReadMeImages/Trello.png)

# App Design

- Home Page Design
  - Multiple cards (with photo, user, description, like count and date of posting).

![ss](/ReadMeImages/HomePage.png)

- View Bloom Design 
  - Card (with photo, user, description, like count and date of posting)
  - Comment list
  - Form for adding comments
  - One button for accessing user profile(the username)

![ss](/ReadMeImages/ViewBloom.png)
  
- Other options for blooms
  - Form for editing comments  

    ![ss](/ReadMeImages/EditComment.png)

  - Form for adding bloom to board

    ![ss](/ReadMeImages/AddBloomToBoard.png)

- Create Bloom Design
  - Two buttons(for choosing the type of post)
  - Two text fields(one for description and one for image url/text(for text post)) 
  - One button for preview

![ss](/ReadMeImages/CreateBloom.png)

- Edit Bloom Design
  - One field for description 
  - One field for image url/text(for text post)

![ss](/ReadMeImages/EditBloom.png)

- Preview Bloom
  - One card showing the bloom
  - Button for returning to editing 
  - Button for applying the changes/posting  

![ss](/ReadMeImages/PreviewBloom.png)

- Create Board Design 
  - Two fields(one for Name and one for Description)
  - One button to create the board
  
![ss](/ReadMeImages/CreateBoard.png)

- Boards Design
  - Cards showing the preview of boards(clickable-redirects to view board)
  - Button for adding a new board

![ss](/ReadMeImages/Boards.png)

- View Board Design
  - Image preview of the saved blooms (clickable-redirects to view bloom)
  - Two buttons for editing and deleting(shown by hovering on the 3 dots)
  - Remove button for deleting a bloom from the board (shown by hovering on the bloom image)

![ss](/ReadMeImages/ViewBoards.png)

- Edit Board Design
  - Two fields(one for Name and one for Description)
  - One button to save the board

![ss](/ReadMeImages/EditBoard.png)

- Profile Page Design
 -....

![ss]()

# UML Diagram
```mermaid
classDiagram
        class User {
        + username: string
        + email: string
        + password: string
        + role: string
    }

    class Admin {
        + username: string
        + email: string
        + password: string
        + role: string
    }

    class Bloom {
        +Int bloomId
        +String title
        +String description
        +String imageUrl
        +Date createdAt
        +User createdBy
        +addComment()
        +addLike()
    }

    class Board {
        +Int boardId
        +String name
        +String description
        +Date createdAt
        +User createdBy
        +addBloom()
        +removeBloom()
    }

    class Like {
        +Int likeId
        +User user
        +Bloom bloom
    }

    class Comment {
        +Int commentId
        +String text
        +User user
        +Bloom bloom
        +Date createdAt
    }

    User <|-- Admin
    User -->  Bloom
    User  -->  Board
    User  -->  Like
    User  -->  Comment
    Board  -->  Bloom
    Bloom  -->  Like
    Bloom  -->  Comment

```


# Architecture Overview
BoardBloom is built using the MVC pattern to separate concerns:

- Model: Represents the data and business logic.
- View: Represents the presentation layer.
- Controller: Handles user input and interactions.

# Data Models

## User
Represents the users of the application.

#### Properties:

- UserId (int): Unique identifier for the user.
- Username (string): User's username.
- Email (string): User's email address.
- PasswordHash (string): Hash of the user's password.
- Role: Role of the user (Admin or User).

## Bloom
Represents individual posts created by users.

### Properties:

- BloomId (int): Unique identifier for the bloom.
- Title (string): Title of the bloom.
- Content (string): Description of the bloom.
- ImageUrl (string): URL of the image associated with the bloom.
- TotalLikes (int): A counter holding the number of likes of the bloom.
- UserId (int): Foreign key referencing the user who created the bloom.
- Date (DateTime): Date and time when the bloom was created.

## Board
Represents collections of Blooms.

### Properties:

- BoardId (int): Unique identifier for the board.
- Name (string): Title of the board.
- Note (string): Description of the board.
- UserId (int): Foreign key referencing the user who created the board.

## BloomBoard
Represent the associative relationship between blooms and boards.

### Properties:

- BloomBoardId (int): Unique identifier for the bloomboard.
- BoardId (int): Unique identifier for the board.
- BloomId (int): Unique identifier for the bloom.
- Date (DateTime): Date and time when the relationship was created.

## Comment
Represents comments on Blooms.

### Properties:

- CommentId (int): Unique identifier for the comment.
- Content (string): Content of the comment.
- UserId (int): Foreign key referencing the user who created the comment.
- BloomId (int): Foreign key referencing the bloom the comment is associated with.
- Date (DateTime): Date and time when the comment was created.

## Like
Represents likes on Blooms.

### Properties:

- LikeId (int): Unique identifier for the like.
- UserId (int): Foreign key referencing the user who liked the bloom.
- BloomId (int): Foreign key referencing the bloom that was liked.

# Controllers


## UserController
Handles user-related operations.

### Actions:

- Register(): Register a new user.
- Login(): Authenticate a user.
- Logout(): Log out the current user.
- UserProfile(int id): View a user's profile. 

## BloomController
Handles bloom-related operations.

### Actions:

- Index(): View all blooms.
- Show(int id): View details of a specific bloom.
- New(): Create a new bloom.
- Previwe(Bloom bloom): Allows the user to see how a bloom would look like before posting it.
- Edit(int id): Edit an existing bloom.
- Delete(int id): Delete a bloom.

## BoardController
Handles board-related operations.

### Actions:

- Index(): View all boards.
- Show(int id): View details of a specific board.
- New(): Create a new board.
- Edit(int id): Edit an existing board.
- Delete(int id): Delete a board.

## CommentController
Handles comment-related operations.

### Actions:

- New(int bloomId): Create a new comment on a bloom.
- Delete(int id): Delete a comment.

# Views
Views in BoardBloom are responsible for rendering the user interface. Each controller action has a corresponding view that presents the data and allows user interaction. The views are implemented using Razor syntax.

# Data Access Layer
The Data Access Layer (DAL) handles communication with the database. Entity Framework Core is used for Object-Relational Mapping (ORM), allowing for simple CRUD operations on the database.

### Example Context: ApplicationDbContext
```c#
namespace BoardBloom.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<BloomBoard> BloomBoards{ get; set; }
        public DbSet<Bloom> Blooms { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<BloomBoard>()
        .HasKey(bb => bb.Id);  

            modelBuilder.Entity<BloomBoard>()
             .HasOne(bb => bb.Bloom)
             .WithMany(b => b.BloomBoards)
             .HasForeignKey(bb => bb.BloomId);

            modelBuilder.Entity<BloomBoard>()
                .HasOne(bb => bb.Board)
                .WithMany(b => b.BloomBoards)
                .HasForeignKey(bb => bb.BoardId);
        }
    }
}
```
# Security
BoardBloom implements security measures to protect user data and ensure safe interactions:

- Authentication: Users must log in to access certain features.
- Authorization: Role-based access control to restrict actions to Admin or User roles.
- Data Protection: Sensitive data, such as passwords, is hashed and stored securely.

# Deployment
To deploy BoardBloom:

- Build the project in Visual Studio.
- Configure the database connection string in the appsettings.json file.
- Run database migrations to set up the database schema.
- Launch the application and verify all features are working correctly.

# Testing
Testing is crucial to ensure the reliability and performance of BoardBloom. Implement unit tests and integration tests to cover the following areas:

- Models: Validate data integrity and business logic.
- Controllers: Test all controller actions for expected behavior.
- Views: Ensure views render correctly with the right data.
- Security: Verify authentication and authorization mechanisms.
- Example Unit Test: BloomControllerTests


# AI used to help with development

- Example of ChatGPT-4 used to help with editing the layout of the app : https://chatgpt.com/share/bd5575a0-e540-4ae9-9089-208313fe3f0e
- We also used Github Copilot for in editor code suggestions and  completions and here are 2 examples: 
![](/ReadMeImages/Copilot1.png)
![](/ReadMeImages/Copilot2.png)


- ChaptGPT-4o for creating the UML Diagram for documentation: https://chatgpt.com/share/f4804ad3-87cd-4412-8f40-2be17e36e33e
