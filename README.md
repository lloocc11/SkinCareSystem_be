# SkinCareSystem_be

## Consultation & Routine Overview

> Chi tiết đầy đủ kèm sequence diagram: [docs/consultation_flows.md](docs/consultation_flows.md)

1. **Chat → AI tư vấn**: người dùng tạo phiên (`POST /api/chat/sessions`), gửi text/ảnh (`POST /api/chat/sessions/{id}/messages` & `/messages/upload`), sau đó gọi `POST /consultations`. Dịch vụ chạy RAG (OpenAI embedding + pgvector + Cloudinary assets) rồi GPT-4o mini trả JSON tư vấn. Kết quả lưu ở `AIAnalysis`, `Routines`, `RoutineSteps` và trả về ngay.
2. **Ảnh người dùng**: hỗ trợ qua Cloudinary (fallback local). URL ảnh lưu ở `ChatMessages.image_url` và được thêm vào context khi gọi GPT, nên tư vấn dựa trên ảnh hoạt động.
3. **Routine sinh ra**: Có – mỗi tư vấn tạo bản ghi trong `Routines` (mặc định `status = active`) và các `RoutineSteps` liên kết.
4. **Consultation Service**: nhận input, gọi RAG, synthesize JSON từ GPT, lưu vào DB, trả `{ analysisId, routineId, confidence, advice, context }` cho client.
5. **Consent**: hiện không bắt buộc. Bỏ qua luồng consent không ảnh hưởng các flow tư vấn/routine.
6. **Hai flow yêu cầu**: đều đã có endpoint
   - Tư vấn text/ảnh: `POST /api/chat/sessions`, `POST /api/chat/sessions/{id}/messages`, `POST /api/chat/sessions/{id}/messages/upload`, `POST /consultations`.
   - Sinh lộ trình: tự động trong `/consultations`; truy xuất bằng `GET /api/routines/{id}`, `GET /api/routinesteps/routine/{routineId}`.
7. **RAG usage**: embed (OpenAI) → ANN search (`DocumentChunks` pgvector, HNSW) → lấy chunk + ảnh (`MedicalDocumentAssets`) → prompt GPT-4o mini (JSON schema) → lưu AIAnalysis/Routine → trả về response chuẩn `ApiResponse<T>`.
