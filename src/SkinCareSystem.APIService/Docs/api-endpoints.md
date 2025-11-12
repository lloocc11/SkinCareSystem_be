# Hướng dẫn API cho Frontend – SkinCareSystem

Tài liệu này tóm tắt toàn bộ endpoint hiện có và cách FE nên gọi. Tất cả API trả về cấu trúc `ServiceResult`:

```json
{
  "success": true,
  "data": { /* payload */ },
  "message": "Get data success"
}
```

`success` phụ thuộc HTTP status; `message` bám theo các hằng số trong `SkinCareSystem.Common.Enum.ServiceResultEnums.Const`. Khi debug, cần xem cả HTTP status, `success` và nội dung `data`.

Header mặc định:

```
Content-Type: application/json
Authorization: Bearer <JWT>
```

Riêng upload file (chat image, medical document) dùng `multipart/form-data`.

---

## 0. Tổng quan Auth & Role

- Base route: `/api/**`
- Endpoint public: `/api/auth/local/register`, `/api/auth/local/login`, `/api/auth/providers/google/token`, `/api/auth/tokens/dev` (dev only).
- Các endpoint khác yêu cầu JWT (`Authorization: Bearer ...`).
- Claims:
  - `nameidentifier` = `user_id`.
  - `role` = `admin | specialist | user`.
- Phân quyền chính:
  - `admin`: quản trị cao nhất.
  - `specialist`: chuyên viên, có thể tạo routine template, chat với user.
  - `user`: người dùng cuối.
- `ChatSessions` (model chung cho mọi kênh chat) hiển thị thêm các trường mới:
  - `channel`: `ai` (user ↔ AI), `ai_admin` (admin/specialist ↔ AI builder), `specialist` (user ↔ specialist).
  - `state`: `open`, `waiting_specialist`, `assigned`, `closed`.
  - `specialistId`, `assignedAt`, `closedAt`: dùng cho channel `specialist`.
  - Tạo session: `ai`/`ai_admin` → `state=open`; `specialist` → `state=waiting_specialist`. Đóng phiên ghi `closedAt` + `state=closed`.
  - Các endpoint chuyên biệt cho specialist nằm ở mục **5**.

---

## 1. Flow Auth & User

### 1.1 Đăng ký local
- **POST** `/api/auth/local/register`
- **Auth**: Public
- **Body**
```json
{
  "fullName": "Nguyen An",
  "email": "an@example.com",
  "password": "Passw0rd!",
  "skinType": "oily",
  "dateOfBirth": "1998-07-10"
}
```
- **Response**: ServiceResult (201/200) chứa thông tin user + token (nếu service trả về).

### 1.2 Đăng nhập local
- **POST** `/api/auth/local/login`
- **Body** `{ "email": "...", "password": "..." }`
- **Response**
```json
{
  "success": true,
  "data": {
    "token": "jwt",
    "expiresAtUtc": "2024-12-31T00:00:00Z"
  },
  "message": "Login success"
}
```

### 1.3 Đăng nhập Google
- **POST** `/api/auth/providers/google/token`
- **Body**: theo `GoogleAuthRequestDto`.
- **Response**: JWT + dữ liệu user. Nếu tạo user mới → HTTP 201 + `Location` header.

### 1.4 Dev login
- **POST** `/api/auth/tokens/dev`
- Chỉ hoạt động ở môi trường Development.

### 1.5 Lấy thông tin user hiện tại
- **GET** `/api/users/me` – trả `UserDto` của người gọi.

### 1.6 Cập nhật profile cá nhân
- **PUT** `/api/users/{id}/profile`
- Chỉ user `{id}` được phép.

### 1.7 Admin quản lý user
- **GET** `/api/users?pageNumber=&pageSize=` (admin)
- **GET** `/api/users/{id}` (admin)
- **POST** `/api/users` (admin)
- **PUT** `/api/users/{id}` (admin)
- **DELETE** `/api/users/{id}` (admin)

---

## 2. Flow Admin quản lý Routine Template

### 2.1 CRUD Routine
- **GET** `/api/routines` (admin)
- **GET** `/api/routines/user/{userId}`
- **GET** `/api/routines/{id}`
- **POST** `/api/routines` (admin,specialist)
```json
{
  "userId": "owner",
  "description": "...",
  "targetSkinType": "oily",
  "targetConditions": "acne",
  "routineType": "template",
  "status": "draft",
  "routineSteps": [
    {
      "instruction": "Rửa mặt dịu nhẹ",
      "stepOrder": 1,
      "timeOfDay": "morning",
      "frequency": "daily"
    }
  ]
}
```
- **PUT** `/api/routines/{id}`
- **DELETE** `/api/routines/{id}` → thực chất chuyển sang trạng thái archived.

### 2.2 Routine step
- **GET** `/api/routine-steps/routine/{routineId}`
- **GET** `/api/routine-steps/{id}`
- **POST** `/api/routine-steps`
- **PUT** `/api/routine-steps/{id}`
- **DELETE** `/api/routine-steps/{id}`

### 2.3 (Deprecated) ingest tài liệu RAG

Tất cả endpoint liên quan đến `MedicalDocuments`, `DocumentChunks`, `MedicalDocumentAssets` nay đã tắt và trả **410 Gone**:

- `POST /api/ingest/documents`
- `POST /api/ingest/documents/{docId}/embed`
- `GET /api/documents/*`, `GET /api/document-chunks/*`, `POST /api/documents/{docId}/assets`, ...

Lý do: AI routine builder đã chuyển sang thuần LLM + image context, không còn truy vấn vector trên DocumentChunks. FE không cần hiển thị UI ingest nữa; nếu gọi nhầm endpoint cũ, hãy hiển thị thông báo “Tính năng RAG đã bị vô hiệu hóa, vui lòng dùng LLM routine builder mới”.

---

## 3. Flow tư vấn AI cho user

### 3.1 Chat session
- **POST** `/api/chat/sessions`
  - Body `{ "userId": "..." , "title": "..." }`
  - Response: `ChatSessionDto`.
- **GET** `/api/chat/sessions/{id}?includeMessages={bool}`
- **GET** `/api/chat/sessions/user/{userId}?pageNumber=&pageSize=`

### 3.2 Gửi & nhận tin nhắn
- **POST** `/api/chat/sessions/{sessionId}/messages`
  - `multipart/form-data`: `Content`, `Image` (file) hoặc `ImageUrl`.
  - User thường: message lưu với `ChatMessages.user_id = callerId` → sau đó AI trả lời (`user_id = null`).
  - Specialist: gửi cùng endpoint, backend nhận diện role và chỉ lưu message người thật (không gọi AI).
  - Admin: có thể gửi thay user (backend gán `user_id = session.UserId`).
  - Response: `ChatTurnResponseDto` chứa cả tin nhắn user + AI, danh sách routine gợi ý.
- **GET** `/api/chat/sessions/{sessionId}/messages?pageNumber=&pageSize=`
- **GET** `/api/chat/messages/{messageId}`

### 3.3 User chọn routine gợi ý
- Sau khi AI trả về routine template, user gọi:
  - **POST** `/api/routine-instances` (body `RoutineInstanceCreateDto`) hoặc
  - **POST** `/api/routine-instances/routine/{routineId}/instances` để tự tạo instance từ template.

---

## 4. Flow Routine Tracking

### 4.1 RoutineInstance
- **GET** `/api/routine-instances/user/{userId}?pageNumber=&pageSize=` – danh sách kế hoạch của user.
- **GET** `/api/routine-instances/{id}`
- **GET** `/api/routine-instances/routine/{routineId}` – dùng cho admin theo dõi các cá nhân đã áp dụng template.
- **POST** `/api/routine-instances`
- **POST** `/api/routine-instances/routine/{routineId}/instances` – user tự start routine.
- **PUT** `/api/routine-instances/{id}`
- **PATCH** `/api/routine-instances/{id}/status`
- **POST** `/api/routine-instances/{id}/recalculate-adherence`
- **DELETE** `/api/routine-instances/{id}`

### 4.2 RoutineProgress
- **GET** `/api/routine-progress/instance/{instanceId}`
- **GET** `/api/routine-progress/instance/{instanceId}/log`
- **GET** `/api/routine-progress/{id}`
- **POST** `/api/routine-progress`
```json
{
  "routineInstanceId": "uuid",
  "routineStepId": "uuid",
  "status": "completed",
  "completedAt": "2024-09-05T07:00:00Z",
  "photoUrl": "https://...",
  "note": "Da hơi đỏ",
  "irritationLevel": 2,
  "moodNote": "Ổn"
}
```
- **PUT** `/api/routine-progress/{id}`
- **DELETE** `/api/routine-progress/{id}`

### 4.3 Feedback
- **GET** `/api/feedbacks/user/{userId}`
- **GET** `/api/feedbacks/routine/{routineId}`
- **GET** `/api/feedbacks/step/{stepId}`
- **GET** `/api/feedbacks/{id}`
- **POST** `/api/feedbacks`
- **PUT** `/api/feedbacks/{id}`
- **DELETE** `/api/feedbacks/{id}`

---

## 5. Flow chat user ↔ specialist

Các phiên `channel = "specialist"` dùng chung `ChatSessions`, nhưng có thêm guard và endpoint riêng:

- User owner luôn đọc/ghi được (trừ khi phiên đã `closed`).
- Specialist phải claim (assign) trước khi trả lời.
- AI không tham gia, tất cả tin nhắn lưu với `ChatMessages.user_id = callerId`.

### 5.1 Danh sách phiên dành cho specialist
- **GET** `/api/chat/specialist-sessions?state=waiting_specialist&pageNumber=&pageSize=`
- **Auth**: `admin` hoặc `specialist`.
- Response: `PagedResult<ChatSessionDto>` gồm các trường `channel`, `state`, `specialistId`, `assignedAt`, `closedAt`. Nếu không có dữ liệu → 404.
- Hỗ trợ `state=assigned` kèm `mine=true` để lấy các phiên đã gán cho chính chuyên viên đang đăng nhập.

### 5.2 Claim/assign session
- **POST** `/api/chat/sessions/{sessionId}/assignments`
- **Auth**: `admin` hoặc `specialist`.
- Không có body, backend tự dùng `callerId`.
- Điều kiện:
  - Session phải `channel='specialist'`.
  - `state='waiting_specialist'`. Nếu đã có `specialistId` khác → HTTP 409. Admin có thể override (gán lại).
- Thành công → trả `ChatSessionDto` với `state='assigned'`, `specialistId=callerId`, `assignedAt` UTC.

### 5.3 Gửi tin nhắn
- Dùng endpoint chung `POST /api/chat/sessions/{sessionId}/messages` (multipart).
- User owner và specialist được assign đều có thể gửi. Admin chỉ đọc được log.
- Nếu `state='closed'` → HTTP 409. Nếu chưa assign, chỉ user được gửi (specialist chưa claim sẽ bị 403).
- Payload/response giống mục 3.2 nhưng **không** có `ChatTurnResponseDto` – backend trả `ServiceResult` của `ChatMessageDto`.

### 5.4 Đóng phiên
- **POST** `/api/chat/sessions/{sessionId}/closures`
- **Auth**: admin, user owner, hoặc specialist được gán.
- Điều kiện:
  - Không thể đóng khi còn `state='waiting_specialist'`.
  - Nếu đã `closed` → HTTP 409.
- Thành công: cập nhật `state='closed'`, `closedAt=now`.

---

## 6. Flow AI tạo Routine Template (Admin/Specialist)

AI builder hiện thuần LLM + ảnh, không còn đọc DocumentChunks. Mọi response đều `isRagBased=false`, `citations=[]`, `source` = `llm` (hoặc `llm_upload`, `llm_text` tùy endpoint). Các endpoint ingest tài liệu cũ trả 410 như đã nêu ở mục 2.3.

### 6.1 Tạo routine draft từ mô tả + ảnh (khuyến nghị)
- **POST** `/api/ai/routines/drafts`
- **Auth**: `admin` hoặc `specialist`
- **Cách 1 – multipart (ưu tiên)**
  - Fields: `query` (bắt buộc), `targetSkinType`, `targetConditions[]`, `maxSteps` (<=20), `numVariants` (<=3), `autoSaveAsDraft` (default true), `images[]` (jpg/png/webp, optional).
- **Cách 2 – JSON**
  - Cùng schema, thay `images[]` bằng `imageUrls[]` (URL Cloudinary đã có).
- **Ví dụ multipart**
```bash
curl -X POST http://localhost:5000/api/ai/routines/drafts \
  -H "Authorization: Bearer <token>" \
  -F "query=Routine trị mụn viêm" \
  -F "targetSkinType=oily" \
  -F "targetConditions=acne" \
  -F "images=@/path/to/acne.jpg"
```
- **Response**
```json
{
  "success": true,
  "data": {
    "routineId": "uuid-or-null",
    "source": "llm",
    "isRagBased": false,
    "citations": [],
    "routine": {
      "description": "... (có disclaimer)...",
      "targetSkinType": "oily",
      "targetConditions": ["acne"],
      "steps": [
        { "order": 1, "instruction": "...", "timeOfDay": "morning", "frequency": "daily" }
      ],
      "source": "llm",
      "isRagBased": false
    }
  }
}
```
- Nếu `autoSaveAsDraft=true`, backend lưu `routine_type='template'`, `status='draft'`, owner = caller.

### 6.2 Generate routine từ file upload (LLM-only)
- **POST** `/api/ai/routines/drafts/documents` (multipart)
- Field bắt buộc: `files[]`. Backend trích xuất text rồi gọi LLM.
- Response tương tự 6.1 nhưng `source="llm_upload"`.

### 6.3 Generate routine từ text (chat)
- **POST** `/api/ai/routines/drafts/text`
- Body: `{ "prompt": "...", "context": "...", "targetSkinType": "...", "targetConditions": [], "autoSaveAsDraft": true }`
- Response `source="llm_text"`.

### 6.4 Publish / update / archive draft
- **PUT** `/api/ai/routines/{routineId}`
- **POST** `/api/ai/routines/{routineId}/publish`
- **POST** `/api/ai/routines/{routineId}/archive`

> Lưu ý: mọi trường `citations` giờ luôn rỗng; FE không cần hiển thị liên kết tài liệu nữa.

---

## 7. Quy ước lỗi & header

- Mã quan trọng:
  - `200`: thành công (`SUCCESS_*`)
  - `201`: tạo mới (`SUCCESS_REGISTER_CODE`)
  - `400`: dữ liệu sai (`ERROR_VALIDATION_CODE`, `FAIL_*`)
  - `401`: chưa đăng nhập (`UNAUTHORIZED_ACCESS_CODE`)
  - `403`: sai quyền (`FORBIDDEN_ACCESS_CODE`)
  - `404`: không thấy dữ liệu (`WARNING_NO_DATA_CODE`)
  - `409`: dữ liệu đã tồn tại (`WARNING_DATA_EXISTED_CODE`)
  - `500`: lỗi hệ thống (`ERROR_EXCEPTION`)
- Khi gửi ảnh chat: FE gửi trực tiếp file qua `/api/chat/sessions/{id}/messages`, backend tự upload Cloudinary.  
  Với các trường dạng `photoUrl` (routine progress, feedback) → FE tự upload lên CDN rồi truyền URL.
- Nhắc lại header:

```
Authorization: Bearer <jwt-token>
```

Thiếu token → 401, sai role → 403.

---

### Checklist gọi API
1. Đăng nhập local/Google → lấy JWT.
2. Gọi `/api/users/me` để lấy profile + role.
3. Với mọi request yêu cầu bảo mật, thêm header `Authorization: Bearer ...`.
4. Tuân thủ role tương ứng cho từng endpoint như đã ghi trong tài liệu này.
