# Turing Test CSGD 2025

My Unity game project for the Circuit Stream Game Dev Bootcamp.

## Features
- Puzzles based on the 7 Bridges of Konigsberg Mathematics Problem

## Development Setup
- Unity 6 (39f1)
- Built with Universal Render Pipeline (URP)

## Development Log
- Log 1 - Youtube: https://www.youtube.com/watch?v=De06rbDIgo4&list=PLpP84LjZXraw-228K5DtWxbz6lvye3yDk
- Log 2 - Youtube: https://www.youtube.com/watch?v=nBUdVOxZMhA
- Log 3 - Project added to Github: https://github.com/AndrewLILP/
- Log 4 - Critical Bug Fixes and refactor foundation: sprint y, gameEnd no
- ------- Foundation Interfaces (New Files)
- Scripts/Core/Interfaces/IPuzzle.cs
- Scripts/Core/Interfaces/IPuzzleValidator.cs
- Scripts/Core/Interfaces/IPuzzleState.cs
- Scripts/Core/Interfaces/IPuzzleObserver.cs
- Scripts/Core/Events/PuzzleEventBus.cs
- Results After Fixes
- ✅ Bridge cubes invisible in Puzzle 2 (like start of Puzzle 1)
- ✅ UI shows "Puzzle 2: Path" instead of Puzzle 1 content
- ✅ Bridge progress hidden (since Puzzle 2 doesn't use bridges)
- ✅ Clear Puzzle 2 instructions displayed
- ✅ Landmass progress shows 0/4 for new puzzle
- puzzle 2 is basically complete - minor refinements needed with UI (fix later) and a logic decision (in puzzle 1, the player starts with 1 land mass - 
- visited but in puzzle 2 they start with 0 land mass visits which gives the impression they can start and finish at the same point - puzzle 2 should 
- start like puzzle 1