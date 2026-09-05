# Auto Mouse Click

A .NET 8 WinForms desktop app for building and replaying mouse click sequences.

## Features

- sequence-first workflow
- record cursor positions into steps
- configurable default delay per captured step
- replay once, fixed count, or until stopped
- edit or remove recorded steps
- tray support
- hotkeys for replay and recording flow

## Hotkeys

- `F6`: replay sequence / stop replay
- `F7`: capture step
- `F8`: arm or stop recording

## Workflow

1. Press `F8` to arm recording.
2. Move the cursor to the target position.
3. Press `F7` to capture a step.
4. Repeat for more steps.
5. Press `F8` to stop recording.
6. Press `F6` or click `Replay sequence` to run the sequence.

## Replay modes

- replay once
- replay fixed number of times
- replay until stopped

## Build

Requirements:

- .NET 8 SDK
- Windows

Build:

`dotnet build`

Run:

`dotnet run`

## License

MIT. See `LICENSE`.
