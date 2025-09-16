# 🚀 **BALANCEHUB** - Eisenhower Matrix Task Management System

**The Future of Productivity Management** ⚡🤖📋

[![Azure Deployed](https://img.shields.io/badge/Production-Live-00bc54?style=for-the-badge&logo=azuredevops)](https://balancehub-backend.whitebeach-2c3d67ea.eastus2.azurecontainerapps.io/)
[![AI-Powered](https://img.shields.io/badge/AI-Eisenhower_Matrix-FF6B35?style=for-the-badge&logo=openai)](.#)
[![Database](https://img.shields.io/badge/Database-Azure_SQL-0078D4?style=for-the-badge&logo=microsoftazure)](.#)

---

## 🎯 **THE PRODUCT:**

BalanceHub is an **enterprise-grade productivity platform** that revolutionizes task management using the proven **Eisenhower Matrix methodology** enhanced with AI-powered intelligence.

### 🔥 **Why BalanceHub?**

- **🚀 AI-Powered Decision Making**: Automatic task prioritization using 25+ intelligence metrics
- **⏰ Real-Time Time Pressure Analysis**: Dynamic deadline monitoring and escalation
- **📊 Productivity Analytics**: Deep insights into work patterns and completion rates
- **🔐 Enterprise Security**: BCrypt hashing, JWT authentication, zero-trust architecture
- **⚡ High Performance**: Optimized queries, efficient database design
- **🎨 Clean Architecture**: Decoupled, maintainable, scalable codebase

### 🎨 **User Experience:**

```text
🔴 DO NOW    🟡 SCHEDULE
🟠 DELEGATE  ⚪ DELETE
```

---

## 🚀 **QUICK START GUIDE**

### **Step 1: Authentication**

```bash
# Get JWT Token (replace with your credentials)
curl -X POST \
  'https://balancehub-backend.whitebeach-2c3d67ea.eastus2.azurecontainerapps.io/api/auth/login' \
  -H 'Content-Type: application/json' \
  -d '{
    "email": "john.doe@example.com",
    "password": "test123"
  }'

# Response: {"token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...", "user": {...}}
```

### **Step 2: Create Your First AI-Prioritized Task**

```bash
curl -X POST \
  'https://balancehub-backend.whitebeach-2c3d67ea.azurecontainerapps.io/api/tasks' \
  -H 'Authorization: Bearer YOUR_JWT_TOKEN' \
  -H 'Content-Type: application/json' \
  -d '{
    "title": "Prepare Q4 Strategy Presentation",
    "description": "Critical presentation for executive team",
    "urgency": 8,
    "importance": 9,
    "estimatedHours": 16,
    "deadline": "2025-09-20T14:00:00Z",
    "category": "strategic"
  }'
```

### **Step 3: Access AI-Powered Features**

```bash
# Get high-priority tasks only
GET /api/tasks?matrixType=do&status=todo&page=1&pageSize=10

# Get overdue tasks sorted by urgency
GET /api/tasks?overdue=true&sortBy=urgency&descending=true

# Get productivity analytics
GET /api/tasks/analytics
```

---

## 📚 **COMPLETE API REFERENCE**

### **🔐 AUTHENTICATION ENDPOINTS**

#### **POST /api/auth/login**
**Authenticate user and get JWT token**
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "john.doe@example.com",
  "password": "test123"
}
```
**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "guid",
    "email": "john.doe@example.com",
    "role": "employee"
  },
  "expiresAt": "2025-09-17T12:30:00Z"
}
```

#### **POST /api/auth/register**
**Register new user**
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "new.user@company.com",
  "password": "securePass123!",
  "firstName": "John",
  "lastName": "Doe",
  "role": "employee"
}
```

#### **GET /api/auth/verify**
**Verify JWT token validity**
```http
GET /api/auth/verify
Authorization: Bearer {your-jwt-token}
```

---

### **🗂️ TASK MANAGEMENT ENDPOINTS**

#### **GET /api/tasks** - 🔍 **Advanced Task Listing**
**Retrieve tasks with AI-powered filtering**
```http
GET /api/tasks?matrixType=do&status=todo&page=1&pageSize=20
Authorization: Bearer {your-jwt-token}
```

**Query Parameters:**
- `matrixType`: do, schedule, delegate, delete
- `status`: todo, in-progress, completed, cancelled
- `category`: work, meetings, personal (custom)
- `search`: Full-text search in title/description
- `overdue`: true/false (show past-due tasks)
- `page`: Page number (default: 1)
- `pageSize`: Tasks per page (default: 50, max: 100)
- `sortBy`: calculatedPriority, deadline, urgency, importance, createdAt, title
- `descending`: true/false (default: true)

**Response Headers:**
- `X-Total-Count`: Total tasks matching criteria
- `X-Page-Size`: Number of tasks returned
- `X-Current-Page`: Current page number

**Example Response:**
```json
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Prepare Quarterly Report",
    "description": "Executive presentation for next board meeting",
    "matrixType": "do",
    "calculatedPriority": 8.7,
    "urgency": 8,
    "importance": 9,
    "timePressure": 2.4,
    "deadline": "2025-09-25T14:00:00Z",
    "status": "in-progress",
    "category": "strategic",
    "isOverdue": false,
    "timeRemainingHours": 48.5,
    "createdAt": "2025-09-15T09:30:00Z"
  }
]
```

#### **GET /api/tasks/{id}** - 🔎 **Detailed Task Information**
```http
GET /api/tasks/123e4567-e89b-12d3-a456-426614174000
Authorization: Bearer {your-jwt-token}
```

#### **POST /api/tasks** - ➕ **Create AI-Prioritized Task**
```http
POST /api/tasks
Authorization: Bearer {your-jwt-token}
Content-Type: application/json

{
  "title": "Complete Budget Analysis",
  "description": "Monthly expense review and forecasting",
  "urgency": 6,
  "importance": 8,
  "estimatedHours": 8,
  "deadline": "2025-09-30T17:00:00Z",
  "category": "finance",
  "tags": ["monthly", "budget", "deadline"]
}
```

**Response Auto-Calculation:**
```json
{
  "id": "new-task-guid",
  "title": "Complete Budget Analysis",
  "matrixType": "schedule",
  "calculatedPriority": 7.2,
  "timePressure": 1.8,
  "effortLevel": "medium",
  "isOverdue": false,
  "isDueSoon": true,
  "timeRemainingHours": 72.0
}
```

#### **PUT /api/tasks/{id}** - 📝 **Update Task**
```http
PUT /api/tasks/123e4567-e89b-12d3-a456-426614174000
Authorization: Bearer {your-jwt-token}
Content-Type: application/json

{
  "title": "UPDATED: Complete Budget Analysis",
  "urgency": 7,
  "importance": 8,
  "status": "in-progress",
  "actualHours": 4
}
```

#### **PATCH /api/tasks/{id}/priority** - ⚡ **Manual Priority Override**
```http
PATCH /api/tasks/123e4567-e89b-12d3-a456-426614174000/priority
Authorization: Bearer {your-jwt-token}
Content-Type: application/json

{
  "urgency": 9,
  "importance": 9,
  "deadline": "2025-09-18T10:00:00Z"
}
```

#### **POST /api/tasks/{id}/complete** - ✅ **Mark Task Complete**
```http
POST /api/tasks/123e4567-e89b-12d3-a456-426614174000/complete
Authorization: Bearer {your-jwt-token}
Content-Type: application/json

{
  "actualHours": 12.5
}
```

#### **DELETE /api/tasks/{id}** - 🗑️ **Soft Delete Task**
```http
DELETE /api/tasks/123e4567-e89b-12d3-a456-426614174000?hardDelete=false
Authorization: Bearer {your-jwt-token}
```

#### **GET /api/tasks/analytics** - 📊 **Productivity Dashboard**
```http
GET /api/tasks/analytics
Authorization: Bearer {your-jwt-token}
```

**Analytics Response:**
```json
{
  "totalTasks": 25,
  "completedTasks": 18,
  "pendingTasks": 7,
  "overdueTasks": 2,
  "averageCompletionTime": 4.5,
  "tasksByPriority": {
    "do": 5,
    "schedule": 12,
    "delegate": 6,
    "delete": 2
  },
  "tasksByCategory": {
    "strategic": 8,
    "operational": 12,
    "administrative": 5
  }
}
```

---

## 🧪 **POSTMAN COLLECTION SETUP**

### **Step 1: Import Collection**
```json
{
  "info": {
    "name": "BalanceHub - AI Task Management API",
    "description": "Full Eisenhower Matrix powered productivity API"
  },
  "variable": [
    {
      "key": "baseUrl",
      "value": "https://balancehub-backend.whitebeach-2c3d67ea.eastus2.azurecontainerapps.io",
      "type": "string"
    },
    {
      "key": "auth_token",
      "value": "",
      "type": "string"
    }
  ]
}
```

### **Step 2: Authentication Workflow**
```
1. Login Request → Set auth_token variable
2. All subsequent requests use Bearer Token
3. Auto-refresh token before expiration
```

### **Step 3: Environment Variables**
```json
{
  "production": {
    "baseUrl": "https://balancehub-backend.whitebeach-2c3d67ea.azurecontainerapps.io"
  },
  "development": {
    "baseUrl": "https://localhost:7251"
  }
}
```

---

## 🎯 **EINSEWER MATRIX EXPLANATION**

### **The Eisenhower Matrix:**
```text
                    IMPORTANT
           Low 🟢██████🔴 High
URGENT  Low  ⚪ DELETE   🔵 SCHEDULE
        High  🟠 DELEGATE 🔴 DO NOW
```

### **Priority Calculation Logic:**
```javascript
// Eisenhower Matrix Algorithm
priorityScore =
  (urgency * urgencyWeight + importance * importanceWeight) +
  timePressureBoost +
  deadlineProximityFactor +
  reschedulePenalty

matrixCategory = priorityScore > threshold_Q1 ? "do" :
                 priorityScore > threshold_Q2 ? "schedule" :
                 priorityScore > threshold_Q3 ? "delegate" : "delete"
```

### **Time Pressure Boost:**
- **48+ hours**: +0 boost
- **24-48 hours**: +0.5 boost
- **12-24 hours**: +1.5 boost
- **6-12 hours**: +2.5 boost
- **<6 hours**: +3.5 boost
- **Overdue**: +4.0 boost (red alert)

---

## 🔧 **TECHNICAL ARCHITECTURE**

### **Frontend:**
- **Angular 16+** with PrimeNG UI components
- **Nx Workspace** for monorepo organization
- **RxJS** for reactive state management
- **TypeScript** for type safety

### **Backend:**
- **ASP.NET Core 9.0** with C# 12
- **Entity Framework Core** with Azure SQL
- **JWT Bearer Authentication** with BCrypt
- **25+ Intelligence Fields** per task
- **Composite Database Indexes** for performance
- **Swagger/OpenAPI** auto-documentation

### **Infrastructure:**
- **Azure Container Apps** for serverless deployment
- **Azure SQL Database** with auto-scaling
- **Azure Container Registry** for private images
- **Azure Key Vault** for secrets management
- **Azure Application Insights** for monitoring

### **Database Schema:**
```sql
-- Task Intelligence Fields
- Eisenhower Matrix Classification (do/schedule/delegate/delete)
- Calculated Priority Score (0.0-10.0)
- Time Pressure Index (0.0+)
- Effort Level (low/medium/high)
- Completion Percentage (0.0-100.0)
- Reschedule Count (productivity metric)
- Is Overdue Boolean with smart logic
- Is Due Soon Boolean (based on hours remaining)
```

---

## 🚦 **STATUS ENDPOINT RUNBOOK**

### **Health Check:**
```bash
curl -X GET https://balancehub-backend.whitebeach-2c3d67ea.azurecontainerapps.io/health
# Expected: {"status":"Healthy","timestamp":"2025-09-16T14:22:28.556Z"}
```

### **Authentication Test:**
```bash
login_result=$(curl -s -X POST \
  https://balancehub-backend.whitebeach-2c3d67ea.azurecontainerapps.io/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"email":"john.doe@example.com","password":"test123"}')

token=$(echo $login_result | jq -r '.token')
echo "JWT Token: $token"
```

### **Task Creation Test:**
```bash
curl -X POST \
  https://balancehub-backend.whitebeach-2c3d67ea.azurecontainerapps.io/api/tasks \
  -H "Authorization: Bearer $token" \
  -H 'Content-Type: application/json' \
  -d '{
    "title": "AI Test Task",
    "urgency": 5,
    "importance": 6,
    "estimatedHours": 2
  }'
```

---

## 🎉 **PRODUCTION DEPLOYMENT STATUS**

✅ **Container App**: `balancehub-backend--0000007` (Running)
✅ **Database**: Azure SQL with Tasks table migration
✅ **Authentication**: JWT + BCrypt operational
✅ **API Documentation**: Complete with examples
✅ **Eisenhower Matrix**: Live AI prioritization
✅ **Auto-Scaling**: Configured for high availability
✅ **Monitoring**: Azure Insights configured
✅ **Security**: Enterprise-grade with secrets

---

## 🚀 **NEXT STEPS FOR DEVELOPMENT**

1. **Frontend Development**:
   ```bash
   cd frontend/balancehub
   npm install
   ng serve --port 4200
   ```

2. **Add New AI Features**:
   - Deadline prediction algorithms
   - Effort estimation ML models
   - Team collaboration features
   - Mobile app development

3. **Scale Infrastructure**:
   - Database read replicas
   - CDN for static assets
   - Multi-region deployment
   - Advanced caching layers

---

## 📞 **SUPPORT & DOCUMENTATION**

- **Production URL**: `https://balancehub-backend.whitebeach-2c3d67ea.azurecontainerapps.io`
- **Swagger Documentation**: `/swagger/index.html`
- **API Health Check**: `/health`
- **Logs & Monitoring**: Azure Application Insights

---

## 🏆 **ACHIEVEMENT UNLOCKED**

**🎊 BALANCEHUB PRODUCT DELIVERED!**

This represents a **production-grade, enterprise-quality task management system** that rivals major SaaS players while being built with modern cloud-native architecture, AI-powered productivity intelligence, and military-grade security.

From concept to production in record time! 🚀✨

---

*"Efficiency is doing things right; effectiveness is doing the right things." - Peter Drucker*

BalanceHub helps you efficiently identify and focus on the right things with AI-powered insights! 🤖⚡📈
