# Memory Game

A WPF application written in C# implementing the classic **Memory game**. The app demonstrates core WPF concepts such as **Data Binding** and the **MVVM design pattern**, with additional features for user management, game saving/loading, and statistics.

---

## Features

### User Authentication
- Sign in with existing users or create a new account.
- Associate a user profile with a local image.
- User data and image paths are saved in a persistent format
- Delete users and all associated data (profile image, games, stats)

### Game Window
- **Game Modes**:
  - `Standard`: 4x4 board
  - `Custom`: MxN board (even number of cards, M/N between 2 and 6)
- **Menu Options**:
  - `File > Category`: Choose from 3 image categories
  - `New Game`: Start a fresh game
  - `Open Game`: Load a previously saved game
  - `Save Game`: Save current game state
  - `Statistics`: View player stats
  - `Exit`: Return to login
- **Options**:
  - Select board type (standard/custom)
- **Help > About**:
  - Displays author name, student email, group number, specialization

### Save/Load System
- Save current game with board configuration, chosen category, elapsed and remaining time.
- Open previously saved games (restricted per user).
- Game configuration is randomized each session.

### Player Statistics
- Tracks games played and games won per user.
- Statistics are saved and displayed in a table format.

---

## Technologies Used

- **Language**: C#
- **Framework**: WPF (.NET)
- **Design Pattern**: MVVM
- **Data Binding**: Fully implemented
- **Persistence**: JSON game data storage

---
