# Realtime AI Chat & Routine Generation – API Guide

> Source of truth:  
> - Controllers: `src/SkinCareSystem.APIService/Controllers/*`  
> - DTOs: `src/SkinCareSystem.Common/DTOs/**`  
> - Services: `src/SkinCareSystem.Services/**`

## A. Endpoint Summary

| HTTP | Route | Auth | Body / Params | Request DTO | Response DTO | Notes |
| ---- | ----- | ---- | ------------- | ----------- | ------------- | ----- |
| POST | `/api/chat/sessions` | Bearer | JSON `{ "userId": "uuid", "title": "optional" }` | `ChatSessionCreateDto` (`src/SkinCareSystem.Common/DTOs/Chat/ChatSessionCreateDto.cs`) | `ApiResponse<ChatSessionDto>` (`src/SkinCareSystem.Common/DTOs/Chat/ChatSessionDto.cs`) | Create chat session – controller `ChatSessionsController.cs` |
| GET | `/api/chat/sessions/{id}?includeMessages=false` | Bearer | Route `{id}` + query `includeMessages` | — | `ApiResponse<ChatSessionDto>` (when `includeMessages=false`). With `true` returns anonymous `{ session_id, ..., messages[] }` | Get session detail |
| GET | `/api/chat/sessions/user/{userId}?pageNumber=1&pageSize=20` | Bearer | Route `{userId}` + paging | — | `ApiResponse<PagedResult<ChatSessionDto>>` | Sessions owned by user |
| POST | `/api/chat/sessions/{sessionId}/messages` | Bearer | `multipart/form-data` fields:<br>• `SessionId` (uuid – must match route)<br>• `UserId` (uuid)<br>• `Content` (string, optional)<br>• `Image` (file, optional)<br>• `ImageUrl` (string, optional)<br>• `GenerateRoutine` (bool) | `ChatMessageCreateDto` (`src/SkinCareSystem.Common/DTOs/Chat/ChatMessageCreateDto.cs`) | `ApiResponse<ChatTurnResponseDto>` (`src/SkinCareSystem.Common/DTOs/Chat/ChatTurnResponseDto.cs`) | Sends user message → RAG + LLM. If `GenerateRoutine=true`, BE persists routine. Controller `ChatMessagesController.cs` |
| GET | `/api/chat/sessions/{sessionId}/messages?pageNumber=1&pageSize=50` | Bearer | Route + paging | — | `ApiResponse<PagedResult<ChatMessageDto>>` (`src/SkinCareSystem.Common/DTOs/Chat/ChatMessageDto.cs`) | List messages in session |
| GET | `/api/chat/messages/{messageId}` | Bearer | Route `{messageId}` | — | `ApiResponse<ChatMessageDto>` | Single message detail |
| GET | `/api/routines/{id}` | Bearer | Route `{id}` | — | `ApiResponse<RoutineDto>` (`src/SkinCareSystem.Common/DTOs/Routine/RoutineDto.cs`) | Routine detail (controller `RoutinesController.cs`) |
| GET | `/api/routine-steps/routine/{routineId}` | Bearer | Route `{routineId}` | — | `ApiResponse<List<RoutineStepDto>>` (`src/SkinCareSystem.Common/DTOs/Routine/RoutineStepDto.cs`) | Steps of routine (`RoutineStepsController.cs`) |

> Ảnh upload: giới hạn 100 MB (`RequestSizeLimit(104857600)` trong `ChatMessagesController`). Mọi endpoint chat yêu cầu user là owner session hoặc admin.

## B. Flow Checklist

### Flow A – Tư vấn từ text/ảnh
1. **Create session** (nếu chưa có): `POST /api/chat/sessions`
2. **Hoặc lấy session cũ**: `GET /api/chat/sessions/user/{userId}` → chọn `sessionId`
3. **Gửi message text/ảnh**: `POST /api/chat/sessions/{sessionId}/messages`  
   Fields: `SessionId`, `UserId`, `Content` (optional), `Image` (optional), `GenerateRoutine=false`
4. **Nhận phản hồi AI** (`ChatTurnResponseDto`): `UserMessage`, `AssistantMessage`, `AnalysisId`, `Confidence`
5. **Xem lịch sử** (tuỳ chọn): `GET /api/chat/sessions/{sessionId}/messages`

### Flow B – Tư vấn + sinh Routine
1. Các bước 1–3 tương tự nhưng gửi `GenerateRoutine=true`
2. API trả `RoutineGenerated=true` + `RoutineId`
3. **Lấy routine**: `GET /api/routines/{routineId}`
4. **Lấy steps**: `GET /api/routine-steps/routine/{routineId}`

## C. Frontend Code (TypeScript + Axios)

```ts
// apiClient.ts
import axios from "axios";

const api = axios.create({
  baseURL: "https://<api-host>",
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("jwt");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

export default api;
```

```ts
// chatApi.ts
import api from "./apiClient";

export interface ChatSessionCreatePayload {
  userId: string;
  title?: string;
}

export const createSession = (payload: ChatSessionCreatePayload) =>
  api.post("/api/chat/sessions", payload);

export const getSessionsByUser = (userId: string, pageNumber = 1, pageSize = 20) =>
  api.get(`/api/chat/sessions/user/${userId}`, { params: { pageNumber, pageSize } });

export const getSession = (id: string, includeMessages = false) =>
  api.get(`/api/chat/sessions/${id}`, { params: { includeMessages } });

export interface SendMessagePayload {
  sessionId: string;
  userId: string;
  content?: string;
  imageFile?: File;
  imageUrl?: string;
  generateRoutine?: boolean;
}

export const sendChatMessage = (payload: SendMessagePayload) => {
  const form = new FormData();
  form.append("SessionId", payload.sessionId);
  form.append("UserId", payload.userId);
  if (payload.content) form.append("Content", payload.content);
  if (payload.imageFile) form.append("Image", payload.imageFile);
  if (payload.imageUrl) form.append("ImageUrl", payload.imageUrl);
  form.append("GenerateRoutine", String(payload.generateRoutine ?? false));

  return api.post(`/api/chat/sessions/${payload.sessionId}/messages`, form, {
    headers: { "Content-Type": "multipart/form-data" },
  });
};

export const getSessionMessages = (sessionId: string, pageNumber = 1, pageSize = 50) =>
  api.get(`/api/chat/sessions/${sessionId}/messages`, { params: { pageNumber, pageSize } });

export const getRoutine = (routineId: string) =>
  api.get(`/api/routines/${routineId}`);

export const getRoutineSteps = (routineId: string) =>
  api.get(`/api/routine-steps/routine/${routineId}`);
```

```tsx
// ChatFlowExample.tsx
import { useState } from "react";
import {
  createSession,
  sendChatMessage,
  getRoutine,
  getRoutineSteps,
} from "./chatApi";

export function ChatFlowExample({ userId }: { userId: string }) {
  const [sessionId, setSessionId] = useState<string>();
  const [analysis, setAnalysis] = useState<any>(null);
  const [routine, setRoutine] = useState<any>(null);
  const [steps, setSteps] = useState<any[]>([]);

  const startSession = async () => {
    const { data } = await createSession({ userId, title: "Skin care chat" });
    setSessionId(data.data.sessionId);
  };

  const sendTextOnly = async () => {
    if (!sessionId) return;
    const { data } = await sendChatMessage({
      sessionId,
      userId,
      content: "Da dầu, nhiều mụn ẩn – nên làm gì?",
      generateRoutine: false,
    });
    setAnalysis(data.data);
  };

  const sendWithRoutine = async () => {
    if (!sessionId) return;
    const { data } = await sendChatMessage({
      sessionId,
      userId,
      content: "Muốn routine sáng tối cho da dầu nhạy cảm.",
      generateRoutine: true,
    });
    setAnalysis(data.data);
    if (data.data.routineGenerated && data.data.routineId) {
      const routineRes = await getRoutine(data.data.routineId);
      setRoutine(routineRes.data.data);
      const stepsRes = await getRoutineSteps(data.data.routineId);
      setSteps(stepsRes.data.data);
    }
  };

  return (
    <div>
      <button onClick={startSession}>New Session</button>
      <button onClick={sendTextOnly}>Ask without routine</button>
      <button onClick={sendWithRoutine}>Ask with routine</button>
      {analysis && <pre>{JSON.stringify(analysis, null, 2)}</pre>}
      {routine && <pre>{JSON.stringify(routine, null, 2)}</pre>}
      {steps.length > 0 && <pre>{JSON.stringify(steps, null, 2)}</pre>}
    </div>
  );
}
```

## D. cURL Samples

```bash
# Create session
curl -X POST https://<host>/api/chat/sessions \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{ "userId": "dc9b697f-e5fc-405c-b371-c4871972eeb5", "title": "Skin issues week 45" }'

# Send text message (no routine)
curl -X POST https://<host>/api/chat/sessions/<sessionId>/messages \
  -H "Authorization: Bearer <token>" \
  -F "SessionId=<sessionId>" \
  -F "UserId=dc9b697f-e5fc-405c-b371-c4871972eeb5" \
  -F "Content=Da dầu, nhiều mụn ẩn, nên uống gì?" \
  -F "GenerateRoutine=false"

# Send message + image
curl -X POST https://<host>/api/chat/sessions/<sessionId>/messages \
  -H "Authorization: Bearer <token>" \
  -F "SessionId=<sessionId>" \
  -F "UserId=dc9b697f-e5fc-405c-b371-c4871972eeb5" \
  -F "Content=Phân tích giúp mình với ảnh kèm theo." \
  -F "Image=@face.jpg" \
  -F "GenerateRoutine=false"

# Request consultation + generate routine
curl -X POST https://<host>/api/chat/sessions/<sessionId>/messages \
  -H "Authorization: Bearer <token>" \
  -F "SessionId=<sessionId>" \
  -F "UserId=dc9b697f-e5fc-405c-b371-c4871972eeb5" \
  -F "Content=Muốn routine sáng tối cho da dầu." \
  -F "GenerateRoutine=true"

# Fetch routine
curl -X GET https://<host>/api/routines/<routineId> \
  -H "Authorization: Bearer <token>"

# Fetch routine steps
curl -X GET https://<host>/api/routine-steps/routine/<routineId> \
  -H "Authorization: Bearer <token>"
```

## E. Notes & UX Tips

- All chat/routine endpoints require JWT Bearer auth. Ensure `Authorization: Bearer <token>` header.
- Responses are wrapped with `ApiResponse<T>` (`BaseApiController`). On errors, `success=false`, `message` contains detail, HTTP status depends on `ServiceResult.Status`.
- Upload image limit is 100 MB. Supported field name is `Image`.
- `ChatTurnResponseDto` includes:
  - `UserMessage` & `AssistantMessage` (both `ChatMessageDto`)
  - `AnalysisId`
  - `RoutineGenerated` (bool)
  - `RoutineId` (nullable)
  - `Confidence`
- Session access: only owner or admin may read/send messages. Violations return 403 (`Const.FORBIDDEN_ACCESS_MSG`).
- Swagger available at `GET http://localhost:<port>/swagger/v1/swagger.json`. Generate TS client:
  ```bash
  npx openapi-typescript-codegen \
    --input http://localhost:<port>/swagger/v1/swagger.json \
    --output src/api-client
  ```
  After generation, prefer calling client functions instead of manual Axios wrappers.
