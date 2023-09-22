graph TD

A[API Endpoints]
B[IUploadedFileHandler]
C[FileValidator]
D[ZipHandler]
E[AzureUploader]

A -->|/api/upload/zip| B
A -->|/api/upload/repo| B
B -->|Handle| C
C -->|Handle| D
D -->|Handle| E
