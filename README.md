# TetrisGym - An ML Agent Training Environment
![TetrisGym](https://i.imgur.com/BMOReN8.gif)

A training environment for machine learning agents to learn Tetris built with Unity & Unity's ml-agents package.
 
This project also includes a rudimentary machine learning agent implementation & an .onnx file of the results of training this agent.

## Configuration

Each instance of the TetrisTrainingGym has its own TetrisAgent and TetrisGame instance. By default, games are not rendered on the UI. To render a game on the UI,
enable the 'Render GUI' checkbox on the TetrisGame instance. Make sure there is only 1 game instance rendering at a time.

## Training

Currently the project is set up to display the actions of the included agent.
To train your own agent, make sure to remove the model in the Model field of the only active TetrisAgent.

### Parallel Training

This project is set up to support parallel training. To train multiple instances of the neural net at once, simply add more instances of the TetrisTrainingGym prefab.
There are already an extra 9 TetrisTrainingGym instances present in the scene, but they are not active by default.
