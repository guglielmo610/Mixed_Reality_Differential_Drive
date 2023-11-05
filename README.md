# Virtual Robot Interaction with AR (HoloLens)
This project is being developed as part of the course "Robotics Perception and Action" held at University of Trento,Italy.
It consist in an app that can be installed on Microsoft Hololens, an augemented reality visor. The software is written in C# and Unity is the motor. In particular the Mixed Reality Toolkit was used to allow the user to interract with the program.

When starting the app, the user has to scan the room to identify obstacles. Once this is done, the user can place the cube, representing the goal that the robot must achieve, in any place of the grid that is automatically generate. The robot is able to compute a trajectory, using the A* algorithm, and reach the target without colliding with the obstacles.




## Authors

- [Ciresa Simone](https://www.github.com/octokatherine)
- Valeria Grotto
- Guglielmo Del Col
- Iga Pawlak 


## Control Panel
The Control Panel is the main interface for the user. By following the steps and activating the corresponding buttons, the user is able to complete the game and the robot is able to reach the target.
![Control Panel](https://github.com/ciresimo/Mixed_Reality_Differential_Drive/blob/main/ControlPanel.jpg)
