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

### 2.3 Upload tài liệu y khoa (cho RAG)
- **POST** `/api/ingest/documents` (admin,specialist, multipart)
  - Trường form: `title`, `source`, `status`, `files[]`.
  - Backend upload Cloudinary, trích xuất text (txt/md/csv, pdf/docx TODO) → ghi vào `MedicalDocuments`.
- **POST** `/api/ingest/documents/{docId}/embed`
  - Body `{ "chunkSize": 1000, "chunkOverlap": 150, "embeddingModel": "text-embedding-3-small" }`.
  - Xóa chunk cũ và chunk lại, lưu vector `Pgvector`.

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

- `ChatSessions` dùng chung cho chat AI và chat với specialist. `ChatSessions.user_id` luôn là user sở hữu.
- `ChatMessages`:
  - User: `user_id = userId`.
  - Specialist: `user_id = specialistId` (gửi cùng endpoint `/messages`).
  - AI: `user_id = null`.
- Hiện chưa có endpoint riêng để quản lý phiên chat của specialist. FE có thể dựa vào các endpoint chung (list session, load messages).

### Endpoint còn thiếu (đề xuất)
1. `GET /api/chat/sessions/assigned` – trả danh sách session đang thuộc specialist hoặc cần xử lý.
2. `POST /api/chat/sessions/{sessionId}/assign` – gán specialist cho session.
3. `GET /api/chat/sessions/metrics` – thống kê số session chờ, unread, v.v.

Các endpoint này **chưa tồn tại**, cần ghi chú “TODO” nếu FE cần.

---

## 6. Flow AI tạo Routine Template (Admin/Specialist)

Các endpoint dưới đây đã implement. RAG fallback: nếu không có DocumentChunks hoặc không match, hệ thống vẫn trả routine bằng LLM, `citations = []`, `source = "llm"`.

### 6.1 Generate routine từ mô tả chung
- **POST** `/api/ai/routines/generate`
- **Auth**: `admin` hoặc `specialist`
- **Body**
```json
{
  "query": "Routine trị mụn viêm cho da dầu",
  "targetSkinType": "oily",
  "targetConditions": ["acne","inflammation"],
  "k": 12,
  "maxSteps": 10,
  "numVariants": 1,
  "autoSaveAsDraft": true,
  "embeddingModel": "text-embedding-3-small"
}
```
- **Response**
```json
{
  "success": true,
  "data": {
    "routineId": "uuid hoặc null nếu không lưu",
    "isRagBased": true,
    "source": "rag",
    "routine": {
      "description": "... kèm disclaimer ...",
      "targetSkinType": "oily",
      "targetConditions": ["acne","inflammation"],
      "steps": [
        { "order": 1, "instruction": "...", "timeOfDay": "evening", "frequency": "daily" }
      ],
      "isRagBased": true,
      "source": "rag"
    },
    "citations": [
      { "docId": "uuid", "chunkId": "uuid", "score": 0.82 }
    ]
  },
  "message": "Save data success"
}
```
(Nếu không có RAG → `isRagBased=false`, `source="llm"`, `citations=[]`)

### 6.2 Generate routine từ tài liệu cụ thể
- **POST** `/api/ai/routines/generate-from-docs`
- **Body**
```json
{
  "query": "Routine làm dịu kích ứng sau tretinoin",
  "docIds": ["uuid1","uuid2"],
  "targetSkinType": "sensitive",
  "targetConditions": ["irritation"],
  "k": 20,
  "autoSaveAsDraft": true
}
```
- Backend chỉ RAG trong danh sách doc_ids; nếu chưa embed → fallback LLM-only.
- Response giống 6.1.

### 6.3 Generate routine từ file upload
- **POST** `/api/ai/routines/generate-from-upload`
- **Auth**: `admin,specialist`
- **Content-Type**: `multipart/form-data`
- **Form fields**
  - `files[]` (bắt buộc, 1..n): hỗ trợ `.txt, .md, .pdf, .doc, .docx, .csv, .tsv`.
  - `query` (optional string)
  - `targetSkinType` (optional string)
  - `targetConditions` (optional, gửi nhiều field `targetConditions=acne` hoặc chuỗi `acne,redness`)
  - `autoSaveAsDraft` (bool, default true)
- **Behavior**:
  - Backend trích xuất text từ file (txt đọc trực tiếp; pdf/docx nếu chưa hỗ trợ sẽ trả lỗi 400).
  - Gọi LLM với `AdditionalContext` là nội dung file, không phụ thuộc RAG.
  - Nếu `autoSaveAsDraft == true`, ghi routine_type=`template`, status=`draft`, user_id = caller.
- **Response**
```json
{
  "success": true,
  "data": {
    "routineId": "uuid-or-null",
    "isRagBased": false,
    "source": "llm_upload",
    "routine": {
      "description": "...",
      "targetSkinType": "oily",
      "targetConditions": ["acne"],
      "steps": [
        { "order": 1, "instruction": "...", "timeOfDay": "evening", "frequency": "daily" }
      ],
      "isRagBased": false,
      "source": "llm_upload"
    },
    "citations": []
  },
  "message": "Generate routine from upload success"
}
```

### 6.4 Publish / update / archive draft
- **PUT** `/api/ai/routines/{routineId}` – `AiRoutineUpdateRequestDto` (ghi đè mô tả + toàn bộ steps).
- **POST** `/api/ai/routines/{routineId}/publish`
- **POST** `/api/ai/routines/{routineId}/archive`

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
