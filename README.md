# Outlook Inbox Management System

A comprehensive inbox management system with **ALL Outlook features** built using ASP.NET Core 8.0 and Angular 19+.

## 🎯 Features

### **Complete Outlook Features Implementation**

#### ✉️ **Email Management**
- ✅ Read, compose, send, reply, reply all, and forward messages
- ✅ Draft management with auto-save
- ✅ Rich text editor (HTML, RTF, Plain Text)
- ✅ Message threading and conversations
- ✅ Quick reply functionality
- ✅ Message importance levels (Low, Normal, High)
- ✅ Sensitivity levels (Normal, Personal, Private, Confidential)
- ✅ Read/unread status tracking
- ✅ Message preview

#### 📁 **Folder Organization**
- ✅ Default folders (Inbox, Sent, Drafts, Deleted, Junk, Archive, Outbox)
- ✅ Custom folder creation
- ✅ Nested folder support
- ✅ Folder icons and colors
- ✅ Unread count per folder
- ✅ Drag & drop to move messages

#### 🏷️ **Categories & Tags**
- ✅ Color-coded categories
- ✅ Multiple categories per message
- ✅ Custom category creation
- ✅ Category-based filtering
- ✅ Default Outlook color categories

#### 🚩 **Flags & Reminders**
- ✅ Flag messages for follow-up
- ✅ Flag due dates
- ✅ Flag completion status
- ✅ Reminder notifications
- ✅ Custom reminder dates

#### 📎 **Attachments**
- ✅ Multiple file attachments
- ✅ Inline attachments
- ✅ Attachment preview
- ✅ Download single/all attachments
- ✅ File type restrictions
- ✅ Size limits

#### 🔍 **Advanced Search**
- ✅ Full-text search
- ✅ Search by sender
- ✅ Search by subject
- ✅ Search by date range
- ✅ Search by attachments
- ✅ Search by importance
- ✅ Multi-criteria search
- ✅ Search within categories

#### 📊 **Sorting & Filtering**
- ✅ Sort by date, sender, subject, importance
- ✅ Ascending/descending order
- ✅ Filter by read/unread
- ✅ Filter by flagged
- ✅ Filter by attachments
- ✅ Filter by categories
- ✅ Filter by date ranges
- ✅ Combined filters

#### 🔄 **Conversation Threading**
- ✅ Automatic conversation grouping
- ✅ Expand/collapse conversations
- ✅ Conversation topic tracking
- ✅ Participant tracking
- ✅ Conversation statistics

#### 📜 **Message Rules & Automation**
- ✅ Rule creation (conditions & actions)
- ✅ Multiple conditions (AND/OR logic)
- ✅ Actions: Move, Delete, Flag, Categorize, Mark as read
- ✅ Rule priorities
- ✅ Enable/disable rules
- ✅ Auto-processing on new messages

#### 👥 **Mentions & Reactions**
- ✅ @Mentions in messages
- ✅ Mention notifications
- ✅ Reactions (Like, Love, Laugh, Wow, Sad, Angry)
- ✅ Reaction tracking
- ✅ User-specific reactions

#### 📊 **Statistics & Analytics**
- ✅ Inbox statistics
- ✅ Unread message count
- ✅ Today's messages
- ✅ This week's messages
- ✅ Average response time
- ✅ Top senders
- ✅ Message trends

#### 🔐 **Security Features**
- ✅ Digital signatures
- ✅ Message encryption
- ✅ Spam detection
- ✅ Junk mail filtering
- ✅ Spam score calculation

#### 📬 **Delivery & Tracking**
- ✅ Delivery receipts
- ✅ Read receipts
- ✅ Delivery status tracking
- ✅ Recipient tracking
- ✅ Bounce detection

#### 🗄️ **Archive & Retention**
- ✅ Archive messages
- ✅ Retention policies
- ✅ Auto-cleanup old messages
- ✅ Empty deleted items
- ✅ Empty junk folder

#### 📤 **Export & Import**
- ✅ Export to EML format
- ✅ Export to PDF
- ✅ Import from EML
- ✅ Bulk operations

#### ⚡ **Bulk Operations**
- ✅ Bulk delete
- ✅ Bulk move
- ✅ Bulk categorize
- ✅ Bulk mark as read/unread
- ✅ Select all functionality

#### 🎨 **UI/UX Features**
- ✅ Multiple layout views (Compact, Normal, Preview)
- ✅ Reading pane (Right/Bottom/Hidden)
- ✅ Outlook-like design
- ✅ Virtual scrolling for performance
- ✅ Responsive design
- ✅ Keyboard shortcuts
- ✅ Dark mode support
- ✅ RTL support (Arabic)

#### 🔔 **Real-time Features**
- ✅ SignalR integration
- ✅ Real-time notifications
- ✅ Live message updates
- ✅ Typing indicators
- ✅ Presence indicators

---

## 🏗️ Architecture

### **Backend (ASP.NET Core 8.0)**

```
Backend/
├── Controllers/
│   └── InboxController.cs          # Complete REST API
├── Models/
│   ├── Message.cs                  # Core message model
│   ├── MessageFolder.cs            # Folder management
│   ├── MessageCategory.cs          # Categories
│   ├── MessageRecipient.cs         # Recipients
│   ├── MessageAttachment.cs        # Attachments
│   ├── MessageRule.cs              # Automation rules
│   ├── MessageReaction.cs          # Reactions
│   ├── MessageMention.cs           # Mentions
│   ├── MessageTracking.cs          # Delivery tracking
│   ├── ConversationThread.cs       # Threading
│   └── ApplicationUser.cs          # User model
├── Services/
│   ├── InboxService.cs             # Main inbox service
│   ├── MessageRuleEngine.cs        # Rule processing
│   └── NotificationService.cs      # Notifications
├── Data/
│   └── ApplicationDbContext.cs     # EF Core DbContext
├── DTOs/
│   └── [All DTOs]                  # Data transfer objects
├── Hubs/
│   └── InboxHub.cs                 # SignalR hub
├── Helpers/
│   └── AutoMapperProfile.cs        # AutoMapper config
└── Program.cs                      # App configuration
```

### **Frontend (Angular 19+)**

```
Frontend/src/app/
├── components/
│   └── inbox/
│       ├── inbox.component.ts      # Main inbox component
│       ├── inbox.component.html    # Inbox template
│       └── inbox.component.scss    # Inbox styles
├── services/
│   └── inbox.service.ts            # API communication
├── models/
│   ├── message.model.ts            # Message interfaces
│   ├── folder.model.ts             # Folder interfaces
│   ├── query-parameters.model.ts   # Query parameters
│   ├── rule.model.ts               # Rule interfaces
│   └── statistics.model.ts         # Statistics interfaces
└── app.module.ts                   # Main module
```

---

## 🚀 Getting Started

### **Prerequisites**

- .NET 8.0 SDK or later
- Node.js 18+ and npm
- SQL Server (LocalDB or full version)
- Visual Studio 2022 or VS Code
- Angular CLI 19+

### **Backend Setup**

1. **Navigate to Backend folder:**
   ```bash
   cd Backend
   ```

2. **Restore NuGet packages:**
   ```bash
   dotnet restore
   ```

3. **Update connection string** in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=OutlookInboxManagement;Trusted_Connection=True;TrustServerCertificate=True"
     }
   }
   ```

4. **Create and apply migrations:**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

5. **Run the backend:**
   ```bash
   dotnet run
   ```

   Backend will be available at: `https://localhost:7001`
   Swagger UI: `https://localhost:7001`

### **Frontend Setup**

1. **Navigate to Frontend folder:**
   ```bash
   cd Frontend
   ```

2. **Install dependencies:**
   ```bash
   npm install
   ```

3. **Update API URL** in `src/environments/environment.ts` if needed:
   ```typescript
   export const environment = {
     production: false,
     apiUrl: 'https://localhost:7001',
     signalRUrl: 'https://localhost:7001/hubs/inbox'
   };
   ```

4. **Run the Angular app:**
   ```bash
   npm start
   ```

   Frontend will be available at: `http://localhost:4200`

---

## 📝 API Endpoints

### **Messages**
- `GET /api/inbox` - Get inbox messages with pagination
- `GET /api/inbox/{id}` - Get single message
- `POST /api/inbox` - Create draft
- `POST /api/inbox/{id}/send` - Send message
- `POST /api/inbox/{id}/reply` - Reply to message
- `POST /api/inbox/{id}/reply-all` - Reply all
- `POST /api/inbox/{id}/forward` - Forward message
- `DELETE /api/inbox/{id}` - Delete message

### **Folders**
- `GET /api/inbox/folders` - Get all folders
- `POST /api/inbox/folders` - Create folder
- `PUT /api/inbox/folders/{id}` - Update folder
- `DELETE /api/inbox/folders/{id}` - Delete folder

### **Categories**
- `GET /api/inbox/categories` - Get categories
- `POST /api/inbox/categories` - Create category
- `POST /api/inbox/{id}/categories/{categoryId}` - Assign category

### **Search**
- `GET /api/inbox/search` - Advanced search
- `GET /api/inbox/search/content` - Search by content
- `GET /api/inbox/search/sender` - Search by sender

### **Rules**
- `GET /api/inbox/rules` - Get rules
- `POST /api/inbox/rules` - Create rule

### **Bulk Operations**
- `POST /api/inbox/bulk/delete` - Bulk delete
- `POST /api/inbox/bulk/move` - Bulk move
- `POST /api/inbox/bulk/read` - Bulk mark as read

### **Statistics**
- `GET /api/inbox/statistics` - Get inbox statistics

---

## 🎨 UI Features

### **Layout Options**
- Compact view
- Normal view
- Preview view

### **Reading Pane**
- Right side
- Bottom
- Hidden

### **Keyboard Shortcuts**
- `Ctrl+N` - New message
- `Ctrl+R` - Reply
- `Ctrl+Shift+R` - Reply All
- `Ctrl+F` - Forward
- `Delete` - Delete selected
- `Ctrl+Q` - Mark as read
- `Ctrl+U` - Mark as unread

---

## 🗃️ Database Schema

The system uses Entity Framework Core with the following main entities:

- **Messages** - Core message storage
- **MessageFolders** - Folder hierarchy
- **MessageCategories** - Categories
- **MessageRecipients** - To/Cc/Bcc recipients
- **MessageAttachments** - File attachments
- **MessageRules** - Automation rules
- **MessageReactions** - User reactions
- **MessageMentions** - @Mentions
- **MessageTracking** - Delivery tracking
- **ConversationThreads** - Message threading

---

## 🔧 Configuration

### **Backend Configuration** (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "Jwt": {
    "Key": "YourSecretKey",
    "Issuer": "OutlookInboxManagement",
    "Audience": "Users",
    "ExpiryInDays": 7
  },
  "Application": {
    "MaxAttachmentSize": 10485760,
    "AllowedFileTypes": [".pdf", ".doc", ".docx"],
    "MessageRetentionDays": 365,
    "EnableSpamFilter": true
  }
}
```

### **Frontend Configuration** (`environment.ts`)

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7001',
  signalRUrl: 'https://localhost:7001/hubs/inbox'
};
```

---

## 📦 NuGet Packages (Backend)

- Microsoft.AspNetCore.OpenApi
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.AspNetCore.Identity.EntityFrameworkCore
- AutoMapper.Extensions.Microsoft.DependencyInjection
- Swashbuckle.AspNetCore
- Microsoft.AspNetCore.SignalR

---

## 📦 NPM Packages (Frontend)

- @angular/core ^19.0.0
- @angular/material ^19.0.0
- @angular/cdk ^19.0.0
- @microsoft/signalr ^8.0.0
- rxjs ^7.8.1

---

## 🧪 Testing

### **Backend Testing**
```bash
cd Backend
dotnet test
```

### **Frontend Testing**
```bash
cd Frontend
npm test
```

---

## 📄 Default Credentials

When the application first runs, it creates a default admin user:

- **Email:** `admin@outlookinbox.com`
- **Password:** `Admin@123`

---

## 🛠️ Development Tools

- **Backend:** Visual Studio 2022, VS Code, Rider
- **Frontend:** VS Code with Angular Language Service
- **Database:** SQL Server Management Studio (SSMS)
- **API Testing:** Swagger UI, Postman
- **Version Control:** Git

---

## 🌟 Key Features Highlights

### **Performance Optimizations**
- Virtual scrolling for large message lists
- Lazy loading of conversations
- Caching strategies
- Database indexing on key fields
- SignalR for real-time updates

### **Security**
- JWT authentication
- Role-based authorization
- CORS configuration
- SQL injection prevention
- XSS protection

### **Scalability**
- Pagination support
- Bulk operations
- Efficient database queries
- Response caching

---

## 📚 Documentation

- **API Documentation:** Available at `https://localhost:7001` (Swagger UI)
- **Code Comments:** Comprehensive inline documentation
- **Architecture Diagrams:** See `/docs` folder (to be created)

---

## 🤝 Contributing

This is a demonstration project showcasing a comprehensive Outlook-like inbox management system.

---

## 📧 Support

For questions or issues, please refer to the documentation or create an issue in the repository.

---

## 📝 License

This project is created for demonstration purposes.

---

## 🎯 Roadmap

### **Part 1 - Completed** ✅
- Complete inbox management system
- All Outlook email features
- Advanced search and filtering
- Rules and automation
- Real-time updates

### **Part 2 - Calendar Integration** (Future)
- Full Outlook calendar features
- Events and appointments
- Recurring events
- Calendar sharing

### **Part 3 - Contacts Management** (Future)
- Contact management
- Contact groups
- Import/export contacts

### **Part 4 - Tasks & To-Do Lists** (Future)
- Task management
- To-do lists
- Task assignments

### **Part 5 - Notes** (Future)
- OneNote-like notes
- Note categories
- Rich text notes

### **Part 6 - Advanced Features** (Future)
- Focused inbox
- Scheduling assistant
- Email templates
- Vacation responder

---

## 🙏 Acknowledgments

Built with:
- ASP.NET Core 8.0
- Angular 19+
- Angular Material
- Entity Framework Core
- SignalR
- AutoMapper

---

**Version:** 1.0.0
**Last Updated:** 2025
**Author:** Rasel
**Technology Stack:** ASP.NET Core 8.0 + Angular 19+