# MiniSocialApp — Kiến trúc hệ thống & Luồng hoạt động

## 1. Tổng quan kiến trúc

```mermaid
graph TB
    subgraph "WinForms App (.NET Framework 4.8)"
        Program["Program.cs<br/>Application.Run(login)"]
        Login["login.cs<br/>Form đăng nhập"]
        Form1["Form1.cs<br/>Main Form + WebView2"]
        CUS["CurrentUserStore<br/>(static global)"]
        MH["MessageHandler<br/>Router JS ↔ C#"]
        
        subgraph "Controllers"
            PC["PostController"]
            LC["LikeController"]
        end
        
        subgraph "Services"
            US["UserService"]
            PS["PostService"]
            LS["LikeService"]
            SS["StorageService"]
        end
        
        subgraph "Core"
            FC["FirestoreContext"]
        end
    end
    
    subgraph "Frontend (WebView2)"
        HTML["home.html"]
        JS["app.js"]
        CSS["style.css"]
    end
    
    subgraph "Cloud Services"
        FS["Google Firestore<br/>(Database)"]
        SB["Supabase Storage<br/>(File Storage)"]
    end
    
    Program --> Login
    Login --> US
    Login --> Form1
    Form1 --> MH
    MH --> PC
    MH --> LC
    PC --> PS
    LC --> LS
    PS --> SS
    US --> FC --> FS
    PS --> FC
    LS --> FC
    SS --> SB
    Form1 --> HTML
    HTML --> JS
    JS <-->|"PostMessage"| Form1
```

---

## 2. Luồng hoạt động từng chức năng

---

### 🔐 Chức năng 1: Đăng nhập / Đăng ký

```mermaid
sequenceDiagram
    actor User
    participant Login as login.cs
    participant US as UserService
    participant FS as Firestore
    participant CUS as CurrentUserStore
    participant Form1 as Form1.cs
    
    User->>Login: Nhập tên + SĐT, nhấn "Đăng nhập"
    Login->>Login: Validate (tên rỗng? SĐT 9-11 số?)
    Login->>Login: btnLogin.Enabled = false
    
    Login->>US: LoginOrCreate(name, phone)
    US->>FS: Query "users" WHERE phone = ?
    
    alt User đã tồn tại
        FS-->>US: Trả về document user
    else User chưa tồn tại
        US->>US: Tạo userId = Guid.NewGuid()
        US->>FS: SetAsync("users/{userId}", userData)
        FS-->>US: OK
    end
    
    US-->>Login: Dictionary user data
    Login->>CUS: CurrentUserStore.User = user
    Login->>Form1: new Form1() → Show()
    Login->>Login: this.Hide()
    
    Note over Form1: Form1_Load bắt đầu...
```

**File liên quan:**
- [login.cs](file:///c:/KHANG/trenlop/HK2%20năm%203/NT106%20(2)%20-%20Lập%20trình%20mạng%20căn%20bản/Project/MiniSocialApp/MiniSocialApp/login.cs) → UI + validation
- [UserService.cs](file:///c:/KHANG/trenlop/HK2%20năm%203/NT106%20(2)%20-%20Lập%20trình%20mạng%20căn%20bản/Project/MiniSocialApp/MiniSocialApp/Services/UserService.cs) → Logic tìm/tạo user

**Firestore operations:**
- `Query("users").WhereEqualTo("phone", phone)` — 1 READ
- `SetAsync("users/{userId}", user)` — 1 WRITE (chỉ khi tạo mới)

---

### 🏠 Chức năng 2: Khởi tạo Main Form + Load Feed

```mermaid
sequenceDiagram
    participant Form1 as Form1.cs
    participant WV as WebView2
    participant JS as app.js
    participant MH as MessageHandler
    participant PC as PostController
    participant PS as PostService
    participant FS as Firestore

    Form1->>WV: EnsureCoreWebView2Async()
    Form1->>WV: Navigate("UI/home.html")
    
    WV-->>Form1: NavigationCompleted event
    Form1->>WV: PostMessage({type: "USER_UPDATED", data: {userName, avatar}})
    Form1->>Form1: StartFeedPolling() (timer 20s)
    
    WV-->>JS: message event → "USER_UPDATED"
    JS->>JS: Cập nhật avatar, placeholder, sidebar
    JS->>JS: showLoadingSkeleton()
    JS->>WV: PostMessage({type: "GET_FEED"})
    
    WV-->>Form1: WebMessageReceived
    Form1->>MH: Handle(json)
    MH->>PC: GetFeed()
    PC->>PS: GetFeed()
    PS->>FS: Query "posts" ORDER BY createdAt DESC
    PS->>FS: Query "follows" WHERE followerId = currentUserId
    PS->>PS: Lọc visibility (public/followers/private)
    PS->>FS: N × GetSnapshot("posts/{id}/likes/{userId}") [song song]
    FS-->>PS: isLiked cho từng bài
    PS-->>PC: List<Dictionary> posts
    PC-->>MH: {type: "FEED_DATA", data: posts}
    MH-->>Form1: JSON string
    Form1->>WV: PostWebMessageAsJson(result)
    
    WV-->>JS: message event → "FEED_DATA"
    JS->>JS: renderFeed(posts) → diff render
```

**Firestore operations mỗi lần GET_FEED:**
- `Query("posts").OrderByDescending("createdAt")` — 1 READ (tất cả posts)
- `Query("follows").WhereEqualTo("followerId", userId)` — 1 READ
- `GetSnapshot("posts/{id}/likes/{userId}")` × N bài — N READs (song song)
- **Tổng: 2 + N reads** (N = số bài sau khi lọc visibility)

---

### 📝 Chức năng 3: Đăng bài viết

```mermaid
sequenceDiagram
    actor User
    participant JS as app.js
    participant WV as WebView2
    participant Form1 as Form1.cs
    participant MH as MessageHandler
    participant PC as PostController
    participant PS as PostService
    participant SS as StorageService
    participant SB as Supabase Storage
    participant FS as Firestore
    
    User->>JS: Nhấn "Đăng bài"
    JS->>JS: Validate (content rỗng?)
    JS->>JS: Hiện spinner "Đang đăng..."
    JS->>WV: PostMessage({type: "CREATE_POST", data: {content, imagePath, visibility}})
    
    WV-->>Form1: WebMessageReceived
    Form1->>MH: Handle(json)
    MH->>PC: CreatePost(data)
    PC->>PS: CreatePost(content, imagePath, visibility)
    
    alt Có ảnh đính kèm
        PS->>SS: new StorageService()
        SS->>SS: ReadAllBytes(filePath)
        SS->>SB: POST /storage/v1/object/images/{guid}.jpg
        SB-->>SS: 200 OK
        SS-->>PS: mediaUrl = ".../public/images/{guid}.jpg"
    end
    
    PS->>PS: Lấy userId, userName, avatar từ CurrentUserStore
    PS->>FS: AddAsync("posts", postData)
    FS-->>PS: documentId
    PS-->>PC: postId
    PC-->>MH: {type: "CREATE_POST_SUCCESS", data: {postId}}
    MH-->>Form1: JSON string
    Form1->>WV: PostWebMessageAsJson(result)
    
    WV-->>JS: message event → "CREATE_POST_SUCCESS"
    JS->>JS: Reset form, ẩn create section
    JS->>JS: showToast("Đăng bài thành công!")
    JS->>JS: showLoadingSkeleton() → loadFeed()
```

**API operations:**
- Supabase: `POST /storage/v1/object/images/{filename}` — 1 HTTP call (chỉ khi có ảnh)
- Firestore: `AddAsync("posts", data)` — 1 WRITE

---

### 👍 Chức năng 4: Like / Unlike bài viết

```mermaid
sequenceDiagram
    actor User
    participant JS as app.js
    participant WV as WebView2
    participant Form1 as Form1.cs
    participant MH as MessageHandler
    participant LC as LikeController
    participant LS as LikeService
    participant FS as Firestore
    
    User->>JS: Nhấn nút "Thích"
    
    Note over JS: ✨ Optimistic UI Update
    JS->>JS: Toggle icon (far ↔ fas) + class "liked"
    JS->>WV: PostMessage({type: "TOGGLE_LIKE", data: {postId}})
    
    WV-->>Form1: WebMessageReceived
    Form1->>MH: Handle(json)
    MH->>LC: ToggleLike(data)
    LC->>LS: ToggleLike(postId)
    LS->>FS: GetSnapshot("posts/{postId}/likes/{userId}")
    
    alt Đã like (document tồn tại) → UNLIKE
        LS->>FS: DeleteAsync("posts/{postId}/likes/{userId}")
        LS->>FS: UpdateAsync("posts/{postId}", likeCount: -1)
        LS-->>LC: {postId, liked: false}
    else Chưa like → LIKE
        LS->>FS: SetAsync("posts/{postId}/likes/{userId}", {userId, likedAt})
        LS->>FS: UpdateAsync("posts/{postId}", likeCount: +1)
        LS-->>LC: {postId, liked: true}
    end
    
    LC-->>MH: {type: "LIKE_UPDATED", data: {postId, liked}}
    MH-->>Form1: JSON string
    Form1->>WV: PostWebMessageAsJson(result)
    
    WV-->>JS: message event → "LIKE_UPDATED"
    JS->>JS: Cập nhật likeCount trên DOM (count ± 1)
```

**Firestore operations:**
- `GetSnapshot("posts/{postId}/likes/{userId}")` — 1 READ
- `DeleteAsync` hoặc `SetAsync` — 1 WRITE (like doc)
- `UpdateAsync("posts/{postId}", likeCount: ±1)` — 1 WRITE
- **Tổng: 1 READ + 2 WRITEs**

---

### 🖼️ Chức năng 5: Chọn ảnh đính kèm

```mermaid
sequenceDiagram
    actor User
    participant JS as app.js
    participant WV as WebView2
    participant Form1 as Form1.cs
    
    User->>JS: Nhấn "Chọn ảnh"
    JS->>WV: PostMessage({type: "CHOOSE_IMAGE"})
    
    WV-->>Form1: WebMessageReceived
    Form1->>Form1: BeginInvoke (UI thread)
    Form1->>User: OpenFileDialog (*.jpg, *.jpeg, *.png)
    User->>Form1: Chọn file hoặc Cancel
    
    Form1->>WV: PostMessage({type: "IMAGE_SELECTED", data: {path}})
    
    WV-->>JS: message event → "IMAGE_SELECTED"
    
    alt Có chọn file
        JS->>JS: Hiển thị preview ảnh (file:///path)
        JS->>JS: selectedImagePath = path
    else Cancel
        JS->>JS: Xóa preview, selectedImagePath = null
    end
```

**API:** Không có — chỉ giao tiếp local giữa JS ↔ C#

---

### 🚪 Chức năng 6: Đăng xuất

```mermaid
sequenceDiagram
    actor User
    participant JS as app.js
    participant WV as WebView2
    participant Form1 as Form1.cs
    participant CUS as CurrentUserStore
    
    User->>JS: Nhấn menu "Đăng xuất"
    JS->>JS: confirm("Bạn có chắc muốn đăng xuất?")
    
    alt User xác nhận
        JS->>WV: PostMessage({type: "LOGOUT"})
        WV-->>Form1: WebMessageReceived
        Form1->>Form1: BeginInvoke (UI thread)
        Form1->>CUS: CurrentUserStore.User = null
        Form1->>Form1: IsRestarting = true
        Form1->>Form1: Application.Restart()
        Note over Form1: App khởi động lại → hiện login form
    end
```

**API:** Không có — chỉ xóa state local và restart app

---

### 🔄 Chức năng 7: Auto-refresh Feed (Polling)

```mermaid
sequenceDiagram
    participant Timer as Timer (20s)
    participant Form1 as Form1.cs
    participant MH as MessageHandler
    participant JS as app.js
    
    loop Mỗi 20 giây
        Timer->>Form1: Tick event
        Form1->>MH: Handle('{"type":"GET_FEED"}')
        MH-->>Form1: JSON feed data
        Form1->>JS: PostWebMessageAsJson(result)
        JS->>JS: renderFeed() → diff render
        Note over JS: Chỉ thêm bài mới<br/>Cập nhật likeCount bài cũ<br/>Giữ scroll + UI state
    end
```

---

## 3. Tổng hợp API / Dịch vụ bên ngoài

Hệ thống sử dụng **2 dịch vụ cloud** với tổng cộng **7 loại API call**:

### API 1: Google Cloud Firestore (Database NoSQL)

| # | Operation | Collection | Chức năng | Loại | Khi nào gọi |
|---|-----------|------------|-----------|------|-------------|
| 1 | `Query.WhereEqualTo` | `users` | Tìm user theo SĐT | READ | Đăng nhập |
| 2 | `SetAsync` | `users/{userId}` | Tạo user mới | WRITE | Đăng ký (lần đầu) |
| 3 | `AddAsync` | `posts` | Tạo bài viết mới | WRITE | Đăng bài |
| 4 | `Query.OrderByDescending` | `posts` | Lấy tất cả bài viết | READ | Load feed / Polling |
| 5 | `Query.WhereEqualTo` | `follows` | Lấy danh sách following | READ | Load feed (lọc visibility) |
| 6 | `GetSnapshotAsync` | `posts/{id}/likes/{userId}` | Kiểm tra user đã like? | READ | Load feed (× N bài) |
| 7 | `SetAsync` | `posts/{id}/likes/{userId}` | Thêm like | WRITE | Like bài |
| 8 | `DeleteAsync` | `posts/{id}/likes/{userId}` | Xóa like | WRITE | Unlike bài |
| 9 | `UpdateAsync` | `posts/{id}` | Cập nhật likeCount ± 1 | WRITE | Like/Unlike |

> **Thư viện:** `Google.Cloud.Firestore` v4.2.0 (NuGet)  
> **Xác thực:** Service Account JSON key (Firebase Admin SDK)  
> **Protocol:** gRPC (HTTP/2) qua `Grpc.Core`

### API 2: Supabase Storage (Object Storage)

| # | Operation | Endpoint | Chức năng | Loại | Khi nào gọi |
|---|-----------|----------|-----------|------|-------------|
| 1 | Upload file | `POST /storage/v1/object/images/{filename}` | Upload ảnh bài viết | WRITE | Đăng bài có ảnh |
| 2 | Public URL | `GET /storage/v1/object/public/images/{filename}` | Hiển thị ảnh | READ | Render feed (trình duyệt tự gọi) |

> **Thư viện:** `System.Net.Http.HttpClient` (tự gọi REST)  
> **Xác thực:** API Key qua header `apikey` + `Authorization: Bearer`  
> **Protocol:** HTTPS (REST API)  
> **Bucket:** `images`

---

## 4. Cấu trúc dữ liệu Firestore

```mermaid
erDiagram
    users {
        string userId PK
        string userName
        string avatar
        string phone
        int followersCount
        int followingCount
        int postCount
        timestamp createdAt
    }
    
    posts {
        string postId PK
        string content
        string mediaUrl
        string userId FK
        string userName
        string avatar
        string visibility "public | followers | private"
        int likeCount
        int commentCount
        timestamp createdAt
    }
    
    likes {
        string odcumentId "= userId"
        string userId
        timestamp likedAt
    }
    
    follows {
        string documentId PK
        string followerId FK
        string followingId FK
    }
    
    users ||--o{ posts : "tạo"
    posts ||--o{ likes : "subcollection"
    users ||--o{ follows : "followerId"
```

| Collection | Đường dẫn | Mô tả |
|------------|-----------|-------|
| `users` | `/users/{userId}` | Thông tin người dùng |
| `posts` | `/posts/{postId}` | Bài viết |
| `likes` | `/posts/{postId}/likes/{userId}` | Subcollection — ai đã like bài nào |
| `follows` | `/follows/{docId}` | Quan hệ follow (followerId → followingId) |

---

## 5. Giao tiếp JS ↔ C# (WebView2 Bridge)

Toàn bộ giao tiếp qua **PostMessage** (JSON):

### JS → C# (6 message types)

| Type | Data | Xử lý bởi | Mô tả |
|------|------|------------|-------|
| `GET_FEED` | *(không có)* | `PostController.GetFeed()` | Lấy danh sách bài viết |
| `CREATE_POST` | `{content, imagePath, visibility}` | `PostController.CreatePost()` | Đăng bài mới |
| `TOGGLE_LIKE` | `{postId}` | `LikeController.ToggleLike()` | Like/Unlike |
| `CHOOSE_IMAGE` | *(không có)* | `Form1` trực tiếp | Mở dialog chọn file |
| `LOGOUT` | *(không có)* | `Form1` trực tiếp | Đăng xuất |

### C# → JS (5 message types)

| Type | Data | Mô tả |
|------|------|-------|
| `USER_UPDATED` | `{userName, avatar}` | Gửi thông tin user sau login |
| `FEED_DATA` | `[{postId, content, ...}]` | Danh sách bài viết |
| `CREATE_POST_SUCCESS` | `{postId}` | Đăng bài thành công |
| `LIKE_UPDATED` | `{postId, liked}` | Kết quả like/unlike |
| `IMAGE_SELECTED` | `{path}` | Đường dẫn ảnh đã chọn |

---

## 6. Tóm tắt số lượng API

| Dịch vụ | Số operations | Protocol | Xác thực |
|---------|---------------|----------|----------|
| **Google Firestore** | 9 loại (READ/WRITE) | gRPC (HTTP/2) | Service Account JSON |
| **Supabase Storage** | 2 loại (Upload + Public URL) | HTTPS REST | API Key + Bearer Token |
| **Tổng** | **11 loại API call** | | |
