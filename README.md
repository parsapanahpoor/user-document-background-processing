# User Document Background Processing

[![Difficulty](https://img.shields.io/badge/difficulty-medium-brightgreen)]()
[![Languages](https://img.shields.io/badge/languages-C%23-informational)]()
[![Deadline](https://img.shields.io/badge/deadline-2025--12--07-critical)]()

> Build a small user management system that uses **background jobs** to process uploaded documents, send notifications, run nightly cleanup, and handle retries — without blocking the public API.

---

## Table of Contents

* [User Document Background Processing](#user-document-background-processing)

  * [Table of Contents](#table-of-contents)
  * [Requirements](#requirements)
  * [Problem Description](#problem-description)
  * [Rules and Constraints](#rules-and-constraints)
  * [Core Scenarios](#core-scenarios)
  * [Input/Output and Examples](#inputoutput-and-examples)
  * [How to Run and Test](#how-to-run-and-test)
  * [How to Submit (PR)](#how-to-submit-pr)
  * [Evaluation Criteria](#evaluation-criteria)
  * [Timeline](#timeline)
  * [Contact](#contact)

---

## Requirements

* Allowed Language: **C#**
* Recommended Versions: `.NET 6+`
* OS: Any (Windows / Linux / macOS)
* You may use any **background job / task scheduling** library, such as:

  * **Hangfire (recommended)**
  * Quartz.NET
  * BackgroundService + custom scheduling logic
* Basic familiarity with:

  * RESTful APIs
  * Background job processing
  * Retry policies
  * File operations

---

## Problem Description

You are implementing a **User Management System** where users register via an API and upload a **document file** (any type such as image, Word, etc.). The system must behave as follows:

1. Accept user registration with an uploaded document.
2. Immediately return a fast response without waiting for heavy tasks.
3. Handle background jobs:

   * Send a welcome message immediately after registration.
   * Process the uploaded document after a **30-second delay**, converting it to PDF (simulation allowed).
   * After successful processing, send a follow-up completion message.
   * Run a nightly cleanup at **00:00** to remove unused or incomplete files.
4. Apply an automatic **retry policy** for every job:

   * Maximum **2 retries**
   * Retry #1 after **5 minutes**
   * Retry #2 after **10 minutes** (after the previous attempt)
5. Background tasks must **never block the API**.

Your job is to design and implement this workflow cleanly and maintainably.

---

## Rules and Constraints

* The `POST /api/users/register` API:

  * Must store user data + file metadata
  * Must enqueue background jobs
  * Must return immediately (`201 Created`)
* The 30-second delay must be implemented with **delayed job scheduling**.
* The nightly cleanup must be implemented as a **recurring job** (cron: `0 0 * * *`).
* PDF conversion may be simulated (writing/copying dummy PDF file).
* Retry policy must be handled by the job library.
* Code must be clean, modular, and readable.

---

## Core Scenarios

### 1. User Registration API

* Endpoint: `POST /api/users/register`
* Input:

  * name, email
  * document file
* Behavior:

  1. Save user + file metadata
  2. Enqueue **immediate job** → Send Welcome Message
  3. Enqueue **delayed job (30s)** → Process Document
  4. Return success response quickly

### 2. Welcome Message Job

* Runs immediately
* Logs/Notifies: "Welcome <UserName>!"
* Must not block API

### 3. Document Processing Job (Delayed 30s)

* Loads file metadata
* Converts to PDF (simulation allowed)
* Updates document status
* Enqueues completion message job

### 4. Completion Message Job

* Triggered after PDF creation
* Message: "Your document has been processed and is now available."

### 5. Nightly Cleanup Job (00:00)

* Runs daily at midnight
* Removes unused, orphan, stale files

### 6. Retry Policy

* Applies to all jobs
* Failure → retry 5 min → retry 10 min
* After exhaustion → log and persist

---

## Input/Output and Examples

### Registration Request

```http
POST /api/users/register HTTP/1.1
Content-Type: multipart/form-data
...
```

Response:

```json
{
  "userId": "a1f9c570-0f3b-4d7d-9c35-0a0c12345678",
  "status": "Registered",
  "message": "User registered successfully."
}
```

### Expected Timeline

```
T0      : Registration → HTTP 201
T0 + 0s : Welcome message
T0 +30s : Document processing
T0 +40s : PDF ready → completion message
00:00   : Nightly cleanup
```

---

## How to Run and Test

### 1) Clone

```bash
git clone https://github.com/<org>/<repo>.git
cd <repo>
```

### 2) Configure

* Configure Hangfire storage (SQL Server / Redis)
* Setup file storage paths

### 3) Build & Run

```bash
dotnet build
dotnet run
```

### 4) Tests

```bash
dotnet test
```

---

## How to Submit (PR)

1. Fork repository
2. Create branch:

```bash
git checkout -b solution/<username>
```

3. Place solution under:

```
solutions/C#/<username>/
  ├── source
  └── README.md
```

4. Create PR titled:

```
[Solution] User Document Background Processing - <username>
```

---

## Evaluation Criteria

| Criteria            | Weight |
| ------------------- | ------ |
| Correctness         | 40%    |
| Code Quality        | 25%    |
| Retry Logic         | 10%    |
| API & Observability | 10%    |
| Documentation       | 5%     |
| Early Submission    | 5%     |

---

## Timeline

* **Start:** `2025-11-30`
* **PR Submission Deadline:** `2025-12-07`

---

## Contact

* .NET / Backend Community Group
* Open an Issue in the repository for questions.
