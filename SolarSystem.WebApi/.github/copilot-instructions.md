# Copilot Instructions

## General Guidelines
- Use DTOs for all API endpoints without modifying core business logic.
- Create DTOs (ProductDto, SectionDto, CommonDto) and mapping extensions (ToDto, ToModel, UpdateFromDto, etc.).
- Use DTOs in controllers at the API boundary only.
- Controllers should import DTOs and use extension methods for conversion, ensuring business logic remains untouched.