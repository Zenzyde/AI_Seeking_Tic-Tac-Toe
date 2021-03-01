# AI_Seeking_Tic-Tac-Toe
 A small game that came as a result of an AI course, a quite modular tic-tac-toe game with a Minimax-AI
 
 This small game project is more for showing my capability to use an AI algorithm to make a simple Tic-Tac-Toe AI, moreso than actually being a game. As such the goal of this small game is simply to try to win against the Minimax AI in a few rounds of Tic-Tac-Toe.

The player is marked with blue circles, the AI is marked with red circles. The following image shows this as well as a 4x4 tic-tac-toe board.
![Game example](/images/game_example_1.png)

The game is separated into two main components, the mark-placer which has the responsibility of of placing markers and handling the advancement of turns.
![Mark Placer](/images/mark_placer.png)

And the board, which has been implemented with a quite high degree of modularity in mind...simply because i wasn't allowed to make a simple 3x3 tic-tac-toe for the AI course.
![Board](/images/board.png)
