# SolarSystem ASP.NET Core API Migration Blueprint (Controller-by-Controller)

> Purpose: exact behavior map for rebuilding the API in Laravel while keeping Next.js frontend compatibility.
>
> Scope scanned: **all controllers in `SolarSystem.WebApi/Controllers`**

---

## 1) Global API Behavior (Applies to All Controllers)

## Base Routing
- Controllers use: `[Route("api/[controller]")]`
- Effective base paths:
  - `/api/Account`
  - `/api/Admin`
  - `/api/Customer`
  - `/api/CustomerFeedback`
  - `/api/Product`
  - `/api/ProjectCards`
  - `/api/Section`

## Serialization / Casing
- API uses ASP.NET Core controllers with `System.Text.Json` default web behavior.
- **Response JSON casing is camelCase** (important for Laravel resources):
  - `Id` -> `id`
  - `SectionId` -> `sectionId`
  - `ImageRelativePath` -> `imageRelativePath`
- Custom anonymous data objects already use camel-like names (`adminId`, `productId`, `sectionId`).

## Date Format
- Date/time fields are JSON serialized as ISO 8601 strings.
- Example: login response `expiration` is a UTC token expiry (`token.ValidTo`) and should be emitted as ISO 8601 (typically with `Z`).

## Response Wrapping Pattern
- Mixed style (must preserve exactly):
  - Some endpoints return raw DTO arrays/objects.
  - Some return `SuccessResponseDto` wrapper:
    ```json
    {
      "success": true,
      "message": "...",
      "data": { ... } | null
    }
    ```
  - Error wrapper commonly `ErrorResponseDto`:
    ```json
    {
      "success": false,
      "message": "...",
      "errors": { "field": ["..."] } | null
    }
    ```
  - Some validation failures return `BadRequest(ModelState)` directly (ModelState dictionary / validation payload), not `ErrorResponseDto`.

## Auth / JWT (Global)
- Auth middleware: JWT Bearer.
- Header format expected:
  - `Authorization: Bearer <token>`
- Token validation:
  - issuer: `SolarSystemApi`
  - audience: `SolarSystemClient`
  - signed with symmetric key from `Jwt:Key`
  - lifetime validated (`ClockSkew = 0`)
- Login endpoint generates claims:
  - `ClaimTypes.Name` = admin username
  - `ClaimTypes.Role` = role string (`MasterAdmin`, `CreateDeleteAdmin`, `ViewAdmin`)
  - custom claim `AdminId` = admin id string

> Laravel note: ensure generated JWT contains equivalent name/role/adminId claims so frontend/admin flows and role guards behave the same.

## Error Codes Presence
- `400`, `401`, `404` are explicitly used.
- `422` is **not explicitly returned** by current ASP.NET code. If Laravel defaults to 422 on validation, that can break strict parity; prefer matching existing behavior (usually 400).

---

## 2) AccountController

Base route: `/api/Account`

### POST `/api/Account/login`

## Auth
- Public (no `[Authorize]`)

## Request Body (JSON)
```json
{
  "username": "string (required, max 100)",
  "password": "string (required, max 100)"
}
```

## Success `200 OK`
```json
{
  "token": "jwt-string",
  "expiration": "2026-...Z",
  "username": "adminUser",
  "role": "MasterAdmin|CreateDeleteAdmin|ViewAdmin"
}
```

## Error Responses
- `400 Bad Request`: invalid model -> ModelState payload
- `401 Unauthorized`:
```json
{
  "success": false,
  "message": "اسم المستخدم أو كلمة المرور غير صحيحة",
  "errors": null
}
```
- `500 Internal Server Error`:
```json
{
  "success": false,
  "message": "خطأ في المصادقة",
  "errors": null
}
```
- `404`: not used
- `422`: not used

---

## 3) AdminController

Base route: `/api/Admin`

Controller-level auth: `[Authorize(Roles = "MasterAdmin,CreateDeleteAdmin,ViewAdmin")]`

### GET `/api/Admin`
- Auth required (any of: `MasterAdmin`, `CreateDeleteAdmin`, `ViewAdmin`)
- No request body

Success `200 OK` (array):
```json
[
  {
    "id": 1,
    "username": "admin",
    "role": "MasterAdmin"
  }
]
```

Errors:
- `401` unauthorized (missing/invalid token)
- `403` forbidden (valid token but role not allowed)
- `400/404/422` not explicitly used

### POST `/api/Admin/Register`
- `[AllowAnonymous]` (public, despite controller-level authorize)

Request body:
```json
{
  "username": "string (required)",
  "password": "string (required, min 6)"
}
```

Success `200 OK`:
```json
{
  "success": true,
  "message": "تم تسجيل <username> بنجاح",
  "data": {
    "adminId": 123
  }
}
```

Errors:
- `400` invalid model OR username exists:
```json
{
  "success": false,
  "message": "اسم المستخدم موجود بالفعل",
  "errors": null
}
```
- `401/404/422` not explicitly used

### PUT `/api/Admin/UpdateRole`
- Auth required roles: `MasterAdmin` or `CreateDeleteAdmin`

Request body:
```json
{
  "id": 123,
  "role": "MasterAdmin|CreateDeleteAdmin|ViewAdmin"
}
```

Success `200 OK`:
```json
{
  "success": true,
  "message": "تم تحديث الرتبة بنجاح",
  "data": null
}
```

Errors:
- `400` invalid model OR invalid role string
- `404` admin not found
- `401/403` auth-related
- `422` not used

### DELETE `/api/Admin/{id}`
- Path param: `id` (int)
- Auth required roles: `MasterAdmin` or `CreateDeleteAdmin`

Success `200 OK`:
```json
{
  "success": true,
  "message": "تم الحذف بنجاح",
  "data": null
}
```

Errors:
- `404`:
```json
{
  "success": false,
  "message": "Admin not found",
  "errors": null
}
```
- `400` when attempting to delete MasterAdmin
- `401/403` auth-related
- `422` not used

---

## 4) CustomerController

Base route: `/api/Customer`

### GET `/api/Customer?lang=en`
- Public (`[AllowAnonymous]`)
- Query:
  - `lang` optional, default `en`

Success `200 OK` (array of `CustomerDto`):
```json
[
  {
    "id": 1,
    "name": "localized name",
    "job": "localized job",
    "translations": [
      {
        "id": 10,
        "languageCode": "en",
        "name": "...",
        "job": "...",
        "customerId": 1
      }
    ]
  }
]
```

Errors:
- `400/401/404/422` not explicitly used for this endpoint

### GET `/api/Customer/{id}?lang=en`
- Public
- Path param: `id`

Success `200 OK`: single `CustomerDto`

Errors:
- `404`:
```json
{
  "success": false,
  "message": "Customer not found",
  "errors": null
}
```
- `400/401/422` not used

### POST `/api/Customer`
- Auth required (`[Authorize]`, any authenticated admin)

Request body:
```json
{
  "nameEn": "required",
  "jobEn": "optional",
  "nameAr": "required",
  "jobAr": "optional"
}
```

Behavior:
- Creates `Customer` row first.
- Creates 2 translation rows (`en`, `ar`) always.

Success `201 Created` (`CreatedAtAction` to GET by id):
```json
{
  "id": 1,
  "name": "english-or-default",
  "job": "...",
  "translations": [ ... ]
}
```

Errors:
- `400` invalid model state
- `401` missing/invalid token
- `422` not used
- `404` not used

### PUT `/api/Customer`
- Auth required

Request body:
```json
{
  "id": 1,
  "nameEn": "required",
  "jobEn": "optional",
  "nameAr": "required",
  "jobAr": "optional"
}
```

Behavior:
- Updates existing en/ar translations or inserts missing ones.

Success `200 OK`:
```json
{
  "success": true,
  "message": "Customer updated successfully",
  "data": null
}
```

Errors:
- `400` invalid model
- `404` customer not found
- `401` unauthorized
- `422` not used

### DELETE `/api/Customer/{id}`
- Auth required

Success `200 OK`:
```json
{
  "success": true,
  "message": "Customer deleted successfully",
  "data": null
}
```

Errors:
- `404` customer not found
- `401` unauthorized
- `400/422` not used

---

## 5) CustomerFeedbackController

Base route: `/api/CustomerFeedback`

### GET `/api/CustomerFeedback?lang=en`
- Public
- Query: `lang` optional default `en`

Success `200 OK` (array of `CustomerFeedbackDto`):
```json
[
  {
    "id": 1,
    "feedback": "localized feedback",
    "customerId": 5,
    "customerName": "localized customer name",
    "translations": [
      {
        "id": 20,
        "languageCode": "en",
        "feedbackText": "...",
        "customerFeedbackId": 1
      }
    ]
  }
]
```

### GET `/api/CustomerFeedback/{id}?lang=en`
- Public

Success `200 OK`: single `CustomerFeedbackDto`

Errors:
- `404` feedback not found (`ErrorResponseDto`)

### POST `/api/CustomerFeedback`
- Auth required

Request body:
```json
{
  "customerId": 5,
  "feedbackEn": "required",
  "feedbackAr": "required"
}
```

Behavior:
- Validates customer exists.
- Creates feedback row + en/ar translation rows.

Success `201 Created`:
- Returns created feedback DTO via `CreatedAtAction`.

Errors:
- `400` invalid model OR customer does not exist (`Cannot create feedback for a non-existing customer`)
- `401` unauthorized
- `404` not used for create
- `422` not used

### PUT `/api/CustomerFeedback`
- Auth required

Request body:
```json
{
  "id": 1,
  "customerId": 5,
  "feedbackEn": "required",
  "feedbackAr": "required"
}
```

Behavior:
- Validates feedback exists.
- Validates customer exists.
- Updates or inserts en/ar translations.

Success `200 OK`:
```json
{
  "success": true,
  "message": "Feedback updated successfully",
  "data": null
}
```

Errors:
- `400` invalid model OR customer not found
- `404` feedback not found
- `401` unauthorized
- `422` not used

### DELETE `/api/CustomerFeedback/{id}`
- Auth required

Success `200 OK`:
```json
{
  "success": true,
  "message": "Feedback deleted successfully",
  "data": null
}
```

Errors:
- `404` feedback not found
- `401` unauthorized
- `400/422` not used

---

## 6) ProductController

Base route: `/api/Product`

### GET `/api/Product/full/{id}`
- Auth required roles: `MasterAdmin,CreateDeleteAdmin`
- Path param: `id`

Success `200 OK` (`ProductDetailDto` + absolute image URLs):
```json
{
  "id": 1,
  "price": 1000.0,
  "sectionId": 2,
  "translations": [
    {
      "id": 10,
      "languageCode": "en",
      "name": "...",
      "mainDesc": "...",
      "subDesc": "...",
      "productId": 1
    }
  ],
  "images": [
    {
      "id": 50,
      "relativePath": "https://host/images/uuid.jpg",
      "productId": 1
    }
  ]
}
```

Errors: `404` product not found, plus `401/403` auth-related.

### GET `/api/Product?sectionId=&pageNumber=1&pageSize=10&lang=en`
- Public
- Query:
  - `sectionId` optional nullable int
  - `pageNumber` default 1; forced minimum 1
  - `pageSize` default 10; forced minimum 10 when invalid (<1)
  - `lang` default `en`

Success `200 OK` (`ProductPagedResponseDto`):
```json
{
  "items": [
    {
      "id": 1,
      "price": 1000.0,
      "sectionId": 2,
      "name": "localized",
      "mainDesc": "...",
      "subDesc": "...",
      "images": [
        {
          "id": 50,
          "relativePath": "https://host/images/uuid.jpg",
          "productId": 1
        }
      ]
    }
  ],
  "totalCount": 34,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 4
}
```

### GET `/api/Product/{id}?lang=en`
- Public

Success `200 OK`: `ProductDto` with absolute image URLs

Errors:
- `404` message Arabic: `المنتج غير موجود.`

### POST `/api/Product/{productId}/translation`
- Auth required roles: `MasterAdmin,CreateDeleteAdmin`
- Path: `productId`

Request body (`ProductTranslationDto`):
```json
{
  "id": 0,
  "languageCode": "en|ar",
  "name": "required",
  "mainDesc": "optional",
  "subDesc": "optional",
  "productId": 123
}
```

Rule: body `productId` must equal route `productId`.

Success `200 OK`: created translation DTO.

Errors:
- `400` invalid/null/mismatched translation data
- `401/403` auth-related

### PUT `/api/Product/translation`
- Auth required roles: `MasterAdmin,CreateDeleteAdmin`

Request body: same `ProductTranslationDto` with `id > 0` required.

Success `200 OK`: updated translation DTO.

Errors:
- `400` invalid data
- `401/403` auth-related

### POST `/api/Product`
- Auth required roles: `MasterAdmin,CreateDeleteAdmin`
- Content-Type: `multipart/form-data`

Form fields:
- `price` (required, >0)
- `sectionId` (required)
- `translationsJson` (optional JSON string representing `ProductTranslationDto[]`)
- Files list under `files` or `files[]`

Success `200 OK`:
```json
{
  "success": true,
  "message": "تمت إضافة المنتج بنجاح",
  "data": {
    "productId": 123
  }
}
```

Behavior notes:
- Product created first, then images saved to `/wwwroot/images`.
- If `translationsJson` missing/invalid/empty, default translations are auto-created:
  - en: `name="Product"`, empty descriptions
  - ar: `name="منتج"`, empty descriptions

Errors:
- `400` invalid model
- `401/403` auth-related

### PUT `/api/Product`
- **AllowAnonymous** (important current behavior!)
- Content-Type: `multipart/form-data`

Form fields:
- `id` (required >0)
- `price` (required >0)
- `sectionId` (required)
- `translationsJson` optional (`ProductTranslationDto[]` JSON string)
- files optional under `files`/`files[]`

Success `200 OK`:
```json
{
  "success": true,
  "message": "تم تحديث بيانات المنتج بنجاح",
  "data": null
}
```

Errors:
- `400` invalid data (`بيانات غير صالحة.`)
- `404` product not found (`المنتج غير موجود.`)

### DELETE `/api/Product/DeleteImage/{imageId}`
- Auth required roles: `MasterAdmin,CreateDeleteAdmin`

Success `200 OK`: success wrapper.

Errors:
- `404` image not found (`الصورة غير موجودة.`)

### DELETE `/api/Product/{id}`
- Auth required roles: `MasterAdmin,CreateDeleteAdmin`

Success `200 OK`:
```json
{
  "success": true,
  "message": "تم حذف المنتج وكافة الملفات المرتبطة به",
  "data": null
}
```

Errors:
- `404` product not found (`Product not found`)

---

## 7) ProjectCardsController

Base route: `/api/ProjectCards`

Controller has `[Authorize(Roles = "MasterAdmin,CreateDeleteAdmin,ViewAdmin")]`, but some actions override with `[AllowAnonymous]`.

### GET `/api/ProjectCards?lang=en`
- Public (`[AllowAnonymous]`)

Success `200 OK`: array of `ProjectHomePageCardDto`
```json
[
  {
    "id": 1,
    "imageRelativePath": "https://host/images/projects/uuid.jpg",
    "title": "localized title",
    "locationText": "localized location"
  }
]
```

### GET `/api/ProjectCards/{id}?lang=en`
- Public

Success `200 OK`: single `ProjectHomePageCardDto`

Errors:
- `404` project card not found

### GET `/api/ProjectCards/full/{id}`
- Requires controller-level auth (no AllowAnonymous)

Success `200 OK` (`ProjectCardDetailDto`):
```json
{
  "id": 1,
  "imageRelativePath": "https://host/images/projects/uuid.jpg",
  "translations": [
    {
      "id": 10,
      "languageCode": "en",
      "title": "...",
      "locationText": "...",
      "projectCardId": 1
    }
  ]
}
```

Errors: `404`, `401/403`.

### POST `/api/ProjectCards`
- Roles: `MasterAdmin,CreateDeleteAdmin`
- Multipart form:
  - `imageRelativePath` optional in DTO
  - `translationsJson` optional JSON array (`ProjectCardTranslationDto[]`)
  - `file` optional image

Success `200 OK`:
```json
{
  "success": true,
  "message": "Project card created successfully",
  "data": {
    "id": 1,
    "imageRelativePath": "/images/projects/...",
    "translations": [ ... ]
  }
}
```

Errors:
- `400` invalid model state
- `401/403` auth-related

### PUT `/api/ProjectCards/{id?}`
- Roles: `MasterAdmin,CreateDeleteAdmin`
- Accepts id via route OR form fields (`Id`, `id`, `updateDto.Id`, `updateDto.id`)
- Multipart form:
  - `id` required effectively (>0)
  - `imageRelativePath` optional
  - `translationsJson` optional
  - `file` optional

Success `200 OK`:
```json
{
  "success": true,
  "message": "Project card updated successfully",
  "data": null
}
```

Errors:
- `400` invalid data
- `404` project card not found
- `401/403` auth-related

### DELETE `/api/ProjectCards/{id}`
- Roles: `MasterAdmin,CreateDeleteAdmin`

Success `200 OK`: success wrapper (`Project card deleted successfully`).

Errors:
- `404` project card not found
- `401/403` auth-related

---

## 8) SectionController

Base route: `/api/Section`

### GET `/api/Section?lang=en`
- Public

Success `200 OK`: array of `SectionDto`
```json
[
  {
    "id": 1,
    "name": "localized name",
    "translations": [
      {
        "id": 10,
        "languageCode": "en",
        "name": "...",
        "sectionId": 1
      }
    ]
  }
]
```

### GET `/api/Section/{id}?lang=en`
- Public

Success `200 OK`: single `SectionDto`

Errors:
- `404`:
```json
{
  "success": false,
  "message": "القسم غير موجود",
  "errors": null
}
```

### POST `/api/Section`
- Roles: `MasterAdmin,CreateDeleteAdmin`

Request body:
```json
{
  "nameEn": "required",
  "nameAr": "required"
}
```

Behavior:
- Creates Section row (without direct name fields)
- Inserts en/ar translations.

Success `200 OK`:
```json
{
  "success": true,
  "message": "تم إنشاء القسم بنجاح",
  "data": {
    "sectionId": 5
  }
}
```

Errors:
- `400` validation error wrapped in custom `ErrorResponseDto` with grouped `errors.validation[]`
- `401/403` auth-related

### PUT `/api/Section`
- Roles configured as: `MasterAdmin,Editor,3,2` (exact string in source; keep parity)

Request body:
```json
{
  "id": 1,
  "nameEn": "required",
  "nameAr": "required"
}
```

Behavior:
- Update existing en/ar translations or create missing ones.

Success `200 OK`:
```json
{
  "success": true,
  "message": "تم تحديث القسم بنجاح",
  "data": null
}
```

Errors:
- `400` invalid data
- `404` section not found for update
- `401/403` auth-related

### DELETE `/api/Section/{id}`
- Roles: `MasterAdmin,CreateDeleteAdmin`

Success `200 OK`:
```json
{
  "success": true,
  "message": "تم حذف القسم بنجاح",
  "data": null
}
```

Errors:
- `404` section not found
- `400` if section contains related products (`لا يمكن حذف قسم يحتوي على منتجات مرتبطة.`)
- `401/403` auth-related

---

## 9) Frontend-Critical Compatibility Checklist for Laravel Implementation

1. Keep route names and HTTP verbs identical.
2. Keep camelCase keys in all JSON responses.
3. Preserve mixed response style (raw DTO vs `success/message/data` wrapper).
4. Preserve multilingual fallback behavior (`lang` lookup then first translation fallback).
5. Preserve absolute image URL generation for GET responses where currently applied.
6. Preserve exact auth role gates per endpoint (including unusual ones like `Section PUT` and `Product PUT` anonymous behavior).
7. Prefer `400` for validation failures to match current API behavior instead of default Laravel `422` (unless frontend is updated).
8. Preserve Arabic/English message texts where currently used by UI flows.

---

## 10) Known Quirks to Replicate (or Intentionally Fix with Frontend Coordination)

- `ProductController.Update` is currently `[AllowAnonymous]` (security risk but behavior exists).
- `SectionController.Update` role string includes `Editor,3,2` in addition to `MasterAdmin`.
- Validation error payload shape is inconsistent across endpoints (`ModelState` vs custom `ErrorResponseDto`).
- Some responses include English messages, others Arabic.

If strict migration parity is required, preserve all quirks above.
