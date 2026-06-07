# Password Brute Force Attack Application

A C# Windows Forms application that cracks passwords using brute force attack with multi-threading support.

---

## Project Structure
BruteForceApp/
├── Program.cs                    # Entry point
├── BruteForceApp.csproj          # .NET 8 / net48 WinForms project
├── README.md                     # GitHub version history
├── Core/
│   ├── PasswordHasher.cs         # SHA256 + static salt
│   ├── PasswordGenerator.cs      # Random password [4,6) chars
│   ├── CombinationGenerator.cs   # All combinations, lazily
│   ├── PasswordValidator.cs      # Independent hash checker
│   └── BruteForceEngine.cs       # Multi + single thread engine
├── Logging/
│   └── PerformanceLogger.cs      # Timing, comparison, log file
└── UI/
├── MainForm.cs               # Full dark WinForms GUI
└── MainForm.Designer.cs      # Auto-generated designer file

---

## Features

- Random password generation with length between 4 and 6 characters
- SHA256 hashing with a constant static salt
- Brute force attack starting from length 1 up to maximum length 6
- Multi-threaded attack using (CPU cores - 1) threads
- Single-threaded mode for performance comparison
- All threads stop immediately when password is found
- Real-time progress bar and elapsed time display
- Performance log comparing single-thread vs multi-thread results

---

## How To Run

1. Clone the repository
git clone [your github link]
2. Open `BruteForceApp.csproj` in Visual Studio
3. Press `F5` to build and run

---

## How It Works

1. Click **Generate Password** — creates a random password and displays its SHA256 hash
2. Click **Start** — launches multi-threaded brute force attack
3. Attack tries all combinations from length 1 to 6 across multiple threads
4. When password is found — all threads stop immediately and result is shown
5. Performance log compares single-thread vs multi-thread execution time

---

## Technologies Used

- C# .NET 8 / .NET Framework 4.8
- Windows Forms (WinForms)
- SHA256 Cryptography (`System.Security.Cryptography`)
- Multi-threading (`Task`, `CancellationToken`)
